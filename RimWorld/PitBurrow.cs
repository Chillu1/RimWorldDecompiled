using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI.Group;
using Verse.Sound;

namespace RimWorld;

public class PitBurrow : Crater
{
	public List<Pawn> emergingFleshbeasts;

	public int emergeDelay;

	public bool assaultColony = true;

	private int collapseTick;

	private const int SpawnIntervalSeconds = 1;

	private static readonly IntRange CollapseTicksRange = new IntRange(720000, 1200000);

	protected override EffecterDef FilledInEffecter => EffecterDefOf.ImpactSmallDustCloud;

	protected override SoundDef FilledInSound => SoundDefOf.PitBurrow_Collapse;

	public override void SpawnSetup(Map map, bool respawningAfterLoad)
	{
		if (!ModLister.CheckAnomaly("Pit burrow"))
		{
			Destroy();
			return;
		}
		base.SpawnSetup(map, respawningAfterLoad);
		if (!respawningAfterLoad)
		{
			collapseTick = Find.TickManager.TicksGame + CollapseTicksRange.RandomInRange;
			if (!emergingFleshbeasts.NullOrEmpty())
			{
				FlushSpawnQueue();
			}
		}
	}

	private void FlushSpawnQueue()
	{
		CellRect cellRect = GenAdj.OccupiedRect(base.Position, Rot4.North, ThingDefOf.PitBurrow.Size).ContractedBy(2);
		List<PawnFlyer> list = new List<PawnFlyer>();
		List<IntVec3> list2 = new List<IntVec3>();
		foreach (Pawn emergingFleshbeast in emergingFleshbeasts)
		{
			IntVec3 randomCell = cellRect.RandomCell;
			GenSpawn.Spawn(emergingFleshbeast, randomCell, base.Map);
			if (CellFinder.TryFindRandomCellNear(base.Position, base.Map, def.size.x / 2 + 1, (IntVec3 c) => !c.Fogged(base.Map) && c.Walkable(base.Map) && !c.Impassable(base.Map), out var result))
			{
				emergingFleshbeast.rotationTracker.FaceCell(result);
				list.Add(PawnFlyer.MakeFlyer(ThingDefOf.PawnFlyer_Stun, emergingFleshbeast, result, null, null, flyWithCarriedThing: false, randomCell.ToVector3() + new Vector3(0f, 0f, -1f)));
				list2.Add(randomCell);
			}
		}
		if (list2.Count != 0)
		{
			SpawnRequest spawnRequest = new SpawnRequest(list.Cast<Thing>().ToList(), list2, 1, 1f);
			spawnRequest.initialDelay = emergeDelay;
			if (assaultColony)
			{
				spawnRequest.lord = LordMaker.MakeNewLord(Faction.OfEntities, new LordJob_FleshbeastAssault(), base.Map);
			}
			base.Map.deferredSpawner.AddRequest(spawnRequest);
			SoundDefOf.Pawn_Fleshbeast_EmergeFromPitGate.PlayOneShot(this);
			emergingFleshbeasts.Clear();
		}
	}

	protected override void Tick()
	{
		base.Tick();
		if (Find.TickManager.TicksGame > collapseTick)
		{
			Messages.Message("MessagePitBurrowCollapsed".Translate(), new TargetInfo(base.Position, base.Map), MessageTypeDefOf.NeutralEvent, historical: false);
			Collapse();
		}
	}

	private void Collapse()
	{
		EffecterDefOf.ImpactSmallDustCloud.Spawn(base.Position, base.Map).Cleanup();
		FillIn();
	}

	public override IEnumerable<Gizmo> GetGizmos()
	{
		foreach (Gizmo gizmo in base.GetGizmos())
		{
			yield return gizmo;
		}
		if (!DebugSettings.ShowDevGizmos)
		{
			yield break;
		}
		yield return new Command_Action
		{
			action = delegate
			{
				if (emergingFleshbeasts == null)
				{
					emergingFleshbeasts = new List<Pawn>();
				}
				PawnKindDef fingerspike = PawnKindDefOf.Fingerspike;
				Pawn item = PawnGenerator.GeneratePawn(fingerspike, FactionUtility.DefaultFactionFrom(fingerspike.defaultFactionDef));
				emergingFleshbeasts.Add(item);
				FlushSpawnQueue();
			},
			defaultLabel = "DEV: Spawn Fleshbeast"
		};
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Values.Look(ref emergeDelay, "emergeDelay", 0);
		Scribe_Collections.Look(ref emergingFleshbeasts, "emergingFleshbeasts", LookMode.Deep);
		Scribe_Values.Look(ref assaultColony, "assaultColony", defaultValue: true);
		Scribe_Values.Look(ref collapseTick, "collapseTick", 0);
	}
}
