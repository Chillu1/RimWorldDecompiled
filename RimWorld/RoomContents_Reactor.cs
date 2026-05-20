using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld;

public class RoomContents_Reactor : RoomContentsWorker
{
	private static readonly FloatRange DestroyedConsolesPer10EdgeCells = new FloatRange(1f, 3f);

	private static readonly IntRange DestroyedConsolesGroupSize = new IntRange(2, 3);

	public override void FillRoom(Map map, LayoutRoom room, Faction faction, float? threatPoints = null)
	{
		CellRect cellRect = (from r in room.rects
			where r.Width >= 6 && r.Height >= 6
			orderby r.Area descending
			select r).FirstOrDefault();
		if (cellRect == default(CellRect))
		{
			Log.Error("Failed to place generator.");
			return;
		}
		SpawnReactor(map, cellRect, faction);
		base.FillRoom(map, room, faction, threatPoints);
		SpawnDestroyedConsoles(map, room);
	}

	private static void SpawnDestroyedConsoles(Map map, LayoutRoom room)
	{
		float num = (float)room.rects.Sum((CellRect r) => r.ContractedBy(1).EdgeCellsCount) / 10f;
		int count = Mathf.Max(Mathf.RoundToInt(DestroyedConsolesPer10EdgeCells.RandomInRange * num), 1);
		RoomGenUtility.FillAroundEdges(ThingDefOf.AncientDestroyedConsole, count, DestroyedConsolesGroupSize, room, map, ConsoleValidator);
		bool ConsoleValidator(IntVec3 cell, Rot4 rot, CellRect cRect)
		{
			foreach (IntVec3 cell in cRect.ExpandedBy(1).Cells)
			{
				Building edifice = cell.GetEdifice(map);
				if (room.Contains(cell, 1) && edifice != null)
				{
					return false;
				}
			}
			return true;
		}
	}

	private static void SpawnReactor(Map map, CellRect largest, Faction faction)
	{
		IntVec3 zero = IntVec3.Zero;
		if (largest.Width % 2 == 0)
		{
			zero.x = 1;
		}
		if (largest.Height % 2 == 0)
		{
			zero.z = 1;
		}
		Thing thing = ThingMaker.MakeThing(ThingDefOf.AncientGravReactor);
		thing.SetFaction(faction ?? Faction.OfAncientsHostile);
		GenSpawn.Spawn(thing, largest.CenterCell - zero, map, Rot4.Random);
	}
}
