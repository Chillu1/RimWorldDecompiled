using System.Collections.Generic;
using Verse;
using Verse.AI.Group;

namespace RimWorld;

public static class StealAIDebugDrawer
{
	private static List<Thing> tmpToSteal = new List<Thing>();

	private static BoolGrid debugDrawGrid;

	private static Lord debugDrawLord = null;

	public static void DebugDraw()
	{
		if (!DebugViewSettings.drawStealDebug)
		{
			debugDrawLord = null;
			return;
		}
		Lord lord = debugDrawLord;
		debugDrawLord = FindHostileLord();
		if (debugDrawLord == null)
		{
			return;
		}
		CheckInitDebugDrawGrid();
		float num = StealAIUtility.StartStealingMarketValueThreshold(debugDrawLord);
		if (lord != debugDrawLord)
		{
			foreach (IntVec3 allCell in Find.CurrentMap.AllCells)
			{
				debugDrawGrid[allCell] = TotalMarketValueAround(allCell, Find.CurrentMap, debugDrawLord.ownedPawns.Count) > num;
			}
		}
		foreach (IntVec3 allCell2 in Find.CurrentMap.AllCells)
		{
			if (debugDrawGrid[allCell2])
			{
				CellRenderer.RenderCell(allCell2);
			}
		}
		tmpToSteal.Clear();
		for (int i = 0; i < debugDrawLord.ownedPawns.Count; i++)
		{
			Pawn pawn = debugDrawLord.ownedPawns[i];
			if (StealAIUtility.TryFindBestItemToSteal(pawn.Position, pawn.Map, 7f, out var item, pawn, tmpToSteal))
			{
				GenDraw.DrawLineBetween(pawn.TrueCenter(), item.TrueCenter());
				tmpToSteal.Add(item);
			}
		}
		tmpToSteal.Clear();
	}

	public static void Notify_ThingChanged(Thing thing)
	{
		if (debugDrawLord == null)
		{
			return;
		}
		CheckInitDebugDrawGrid();
		if (thing.def.category != ThingCategory.Building && thing.def.category != ThingCategory.Item && thing.def.passability != Traversability.Impassable)
		{
			return;
		}
		if (thing.def.passability == Traversability.Impassable)
		{
			debugDrawLord = null;
			return;
		}
		int num = GenRadial.NumCellsInRadius(8f);
		float num2 = StealAIUtility.StartStealingMarketValueThreshold(debugDrawLord);
		for (int i = 0; i < num; i++)
		{
			IntVec3 intVec = thing.Position + GenRadial.RadialPattern[i];
			if (intVec.InBounds(thing.Map))
			{
				debugDrawGrid[intVec] = TotalMarketValueAround(intVec, Find.CurrentMap, debugDrawLord.ownedPawns.Count) > num2;
			}
		}
	}

	private static float TotalMarketValueAround(IntVec3 center, Map map, int pawnsCount)
	{
		if (center.Impassable(map))
		{
			return 0f;
		}
		float num = 0f;
		tmpToSteal.Clear();
		for (int i = 0; i < pawnsCount; i++)
		{
			IntVec3 intVec = center + GenRadial.RadialPattern[i];
			if (!intVec.InBounds(map) || intVec.Impassable(map) || !GenSight.LineOfSight(center, intVec, map))
			{
				intVec = center;
			}
			if (StealAIUtility.TryFindBestItemToSteal(intVec, map, 7f, out var item, null, tmpToSteal))
			{
				num += StealAIUtility.GetValue(item);
				tmpToSteal.Add(item);
			}
		}
		tmpToSteal.Clear();
		return num;
	}

	private static Lord FindHostileLord()
	{
		Lord lord = null;
		List<Lord> lords = Find.CurrentMap.lordManager.lords;
		for (int i = 0; i < lords.Count; i++)
		{
			if (lords[i].faction != null && lords[i].faction.HostileTo(Faction.OfPlayer) && (lord == null || lords[i].ownedPawns.Count > lord.ownedPawns.Count))
			{
				lord = lords[i];
			}
		}
		return lord;
	}

	private static void CheckInitDebugDrawGrid()
	{
		if (debugDrawGrid == null)
		{
			debugDrawGrid = new BoolGrid(Find.CurrentMap);
		}
		else if (!debugDrawGrid.MapSizeMatches(Find.CurrentMap))
		{
			debugDrawGrid.ClearAndResizeTo(Find.CurrentMap);
		}
	}
}
