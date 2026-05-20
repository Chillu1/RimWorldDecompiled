using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class LabyrinthMapComponent : CustomMapComponent
{
	public Building labyrinthObelisk;

	public Building abductorObelisk;

	public Map sourceMap;

	private LayoutStructureSketch structureSketch;

	private List<LayoutRoom> spawnableRooms;

	private bool closing;

	private int nextTeleportTick;

	private static readonly IntRange TeleportDelayTicks = new IntRange(6, 60);

	private const int IntervalCheckCloseTicks = 300;

	private static readonly List<IntVec3> tmpCells = new List<IntVec3>();

	public LabyrinthMapComponent(Map map)
		: base(map)
	{
	}

	public override void MapComponentTick()
	{
		if (!closing && GenTicks.IsTickInterval(300) && abductorObelisk.DestroyedOrNull() && !map.mapPawns.AnyColonistSpawned)
		{
			PocketMapUtility.DestroyPocketMap(map);
		}
		TeleportPawnsClosing();
	}

	private void TeleportPawnsClosing()
	{
		if (!closing || GenTicks.TicksGame < nextTeleportTick)
		{
			return;
		}
		Map dest = sourceMap ?? abductorObelisk?.Map;
		nextTeleportTick = GenTicks.TicksGame + TeleportDelayTicks.RandomInRange;
		if (dest == null)
		{
			foreach (Map map in Find.Maps)
			{
				if (map.IsPlayerHome)
				{
					dest = map;
					break;
				}
			}
		}
		if (dest == null || !CellFinderLoose.TryGetRandomCellWith((IntVec3 pos) => IsValidTeleportCell(pos, dest), dest, 1000, out var result))
		{
			return;
		}
		using (List<Pawn>.Enumerator enumerator2 = base.map.mapPawns.AllPawns.GetEnumerator())
		{
			if (enumerator2.MoveNext())
			{
				Pawn current2 = enumerator2.Current;
				if (SkipUtility.SkipTo(current2, result, dest) is Pawn pawn && PawnUtility.ShouldSendNotificationAbout(pawn))
				{
					Messages.Message("MessagePawnReappeared".Translate(pawn.Named("PAWN")), pawn, MessageTypeDefOf.NeutralEvent, historical: false);
				}
				current2.inventory.UnloadEverything = true;
				return;
			}
		}
		foreach (Thing item in (IEnumerable<Thing>)base.map.spawnedThings)
		{
			if (item.def.category == ThingCategory.Item)
			{
				SkipUtility.SkipTo(item, result, dest);
				return;
			}
		}
		Find.LetterStack.ReceiveLetter("LetterLabelLabyrinthExit".Translate(), "LetterLabyrinthExit".Translate(), LetterDefOf.NeutralEvent);
		PocketMapUtility.DestroyPocketMap(base.map);
		if (abductorObelisk != null)
		{
			abductorObelisk.GetComp<CompObelisk_Abductor>().Notify_MapDestroyed();
			if (abductorObelisk.Spawned)
			{
				EffecterDefOf.Skip_EntryNoDelay.Spawn(abductorObelisk.Position, abductorObelisk.Map, 2f).Cleanup();
				abductorObelisk.Destroy();
			}
		}
	}

	private static bool IsValidTeleportCell(IntVec3 pos, Map dest)
	{
		if (!pos.Fogged(dest) && pos.Standable(dest))
		{
			return dest.reachability.CanReachColony(pos);
		}
		return false;
	}

	public void SetSpawnRooms(List<LayoutRoom> rooms)
	{
		spawnableRooms = rooms;
	}

	public void StartClosing()
	{
		closing = true;
	}

	public Thing TeleportToLabyrinth(Thing thing)
	{
		IntVec3 dropPosition = GetDropPosition();
		Thing thing2 = SkipUtility.SkipTo(thing, dropPosition, map);
		if (thing is Pawn pawn)
		{
			pawn.needs?.mood?.thoughts?.memories?.TryGainMemory(ThoughtDefOf.ObeliskAbduction);
			if (PawnUtility.ShouldSendNotificationAbout(pawn))
			{
				Messages.Message("MessagePawnVanished".Translate(pawn.Named("PAWN")), thing2, MessageTypeDefOf.NeutralEvent, historical: false);
			}
		}
		return thing2;
	}

	private IntVec3 GetDropPosition()
	{
		foreach (CellRect rect in spawnableRooms.RandomElement().rects)
		{
			tmpCells.AddRange(rect.ContractedBy(2));
		}
		IntVec3 root = tmpCells.RandomElement();
		tmpCells.Clear();
		return CellFinder.StandableCellNear(root, map, 5f);
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Values.Look(ref closing, "closing", defaultValue: false);
		Scribe_Values.Look(ref nextTeleportTick, "nextTeleportTick", 0);
		Scribe_References.Look(ref labyrinthObelisk, "labyrinthObelisk");
		Scribe_References.Look(ref abductorObelisk, "abductorObelisk");
		Scribe_References.Look(ref sourceMap, "sourceMap");
		Scribe_Deep.Look(ref structureSketch, "structureSketch");
		Scribe_Collections.Look(ref spawnableRooms, "spawnableRects", LookMode.Deep);
	}
}
