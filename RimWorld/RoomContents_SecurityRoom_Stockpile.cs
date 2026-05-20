using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld;

public class RoomContents_SecurityRoom_Stockpile : RoomContentsWorker
{
	private static readonly FloatRange DestroyedConsolesPer10EdgeCells = new FloatRange(1f, 3f);

	private static readonly IntRange DestroyedConsolesGroupSize = new IntRange(2, 3);

	public override void FillRoom(Map map, LayoutRoom room, Faction faction, float? threatPoints = null)
	{
		base.FillRoom(map, room, faction, threatPoints);
		SpawnAncientMachine(map, room);
		SpawnDestroyedConsoles(map, room);
	}

	private static void SpawnAncientMachine(Map map, LayoutRoom room)
	{
		RoomGenUtility.FillWithPadding(ThingDefOf.AncientMachine, 1, room, map);
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
}
