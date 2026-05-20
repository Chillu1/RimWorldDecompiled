using System.Collections.Generic;
using LudeonTK;
using RimWorld;
using Verse.AI.Group;

namespace Verse.AI;

public static class BreachingGridDebug
{
	private static IntVec3 debugStartCell = IntVec3.Invalid;

	private static BreachingGrid debugBeachGridForDrawing = null;

	private static void DebugBreachPickTwoPoints(int breachRadius, int walkMargin, bool useAvoidGrid)
	{
		if (!debugStartCell.IsValid)
		{
			debugStartCell = UI.MouseCell();
			return;
		}
		IntVec3 destCall = UI.MouseCell();
		DebugCreateBreachPath(breachRadius, walkMargin, useAvoidGrid, debugStartCell, destCall);
	}

	private static void DebugBreachPickOnePoint(int breachRadius, int walkMargin, bool useAvoidGrid)
	{
		IntVec3 intVec = UI.MouseCell();
		IntVec3 destCall = GenAI.RandomRaidDest(intVec, Find.CurrentMap);
		if (!destCall.IsValid)
		{
			Messages.Message("Could not find a destination for breach path", MessageTypeDefOf.RejectInput, historical: false);
		}
		else
		{
			DebugCreateBreachPath(breachRadius, walkMargin, useAvoidGrid, intVec, destCall);
		}
	}

	private static void DebugCreateBreachPath(int breachRadius, int walkMargin, bool useAvoidGrid, IntVec3 startCell, IntVec3 destCall)
	{
		DebugViewSettings.drawBreachingGrid = true;
		debugBeachGridForDrawing = new BreachingGrid(Find.CurrentMap, null);
		debugBeachGridForDrawing.CreateBreachPath(startCell, destCall, breachRadius, walkMargin, useAvoidGrid);
		debugStartCell = IntVec3.Invalid;
	}

	public static void ClearDebugPath()
	{
		debugBeachGridForDrawing = null;
	}

	public static void DebugDrawAllOnMap(Map map)
	{
		if (!DebugViewSettings.drawBreachingGrid && !DebugViewSettings.drawBreachingNoise)
		{
			return;
		}
		if (debugBeachGridForDrawing?.Map == map)
		{
			DebugDrawBreachingGrid(debugBeachGridForDrawing);
		}
		List<Lord> lords = map.lordManager.lords;
		for (int i = 0; i < lords.Count; i++)
		{
			LordToilData_AssaultColonyBreaching lordToilData_AssaultColonyBreaching = BreachingUtility.LordDataFor(lords[i]);
			if (lordToilData_AssaultColonyBreaching != null)
			{
				DebugDrawBreachingGrid(lordToilData_AssaultColonyBreaching.breachingGrid);
				if (lordToilData_AssaultColonyBreaching.currentTarget != null)
				{
					CellRenderer.RenderSpot(lordToilData_AssaultColonyBreaching.currentTarget.Position, 0.9f, 0.4f);
				}
			}
		}
	}

	private static void DebugDrawMarkerGrid(BreachingGrid grid, Map map)
	{
		for (int i = 0; i < map.Size.x; i++)
		{
			for (int j = 0; j < map.Size.z; j++)
			{
				IntVec3 c = new IntVec3(i, 0, j);
				switch (grid.MarkerGrid[c])
				{
				case 180:
					CellRenderer.RenderSpot(c, 0.1f);
					break;
				case 10:
					CellRenderer.RenderCell(c, 0.1f);
					break;
				}
				if (grid.ReachableGrid[c])
				{
					CellRenderer.RenderSpot(c, 0.5f, 0.03f);
				}
			}
		}
	}

	private static void DebugDrawBreachingGrid(BreachingGrid grid)
	{
		if (DebugViewSettings.drawBreachingNoise)
		{
			BreachingNoiseDebugDrawer.DebugDrawNoise(grid);
		}
		if (!DebugViewSettings.drawBreachingGrid)
		{
			return;
		}
		DebugDrawMarkerGrid(grid, grid.Map);
		foreach (IntVec3 activeCell in grid.WalkGrid.ActiveCells)
		{
			Building firstBuilding = activeCell.GetFirstBuilding(grid.Map);
			float colorPct = 0.3f;
			if (grid.BreachGrid[activeCell])
			{
				colorPct = 0.4f;
				if (firstBuilding != null && BreachingUtility.ShouldBreachBuilding(firstBuilding))
				{
					colorPct = 0.1f;
					if (BreachingUtility.IsWorthBreachingBuilding(grid, firstBuilding))
					{
						colorPct = 0.8f;
						if (BreachingUtility.CountReachableAdjacentCells(grid, firstBuilding) > 0)
						{
							CellRenderer.RenderSpot(activeCell, colorPct);
						}
					}
				}
			}
			CellRenderer.RenderCell(activeCell, colorPct);
		}
	}

	[DebugAction("Pawns", "Draw breach path...", false, false, false, false, false, 0, false, actionType = DebugActionType.Action, allowedGameStates = AllowedGameStates.PlayingOnMap, hideInSubMenu = true)]
	private static void DebugDrawBreachPath()
	{
		List<DebugMenuOption> list = new List<DebugMenuOption>();
		for (int i = 1; i <= 5; i++)
		{
			int widthLocal = i;
			list.Add(new DebugMenuOption("width: " + i, DebugMenuOptionMode.Action, delegate
			{
				List<DebugMenuOption> list2 = new List<DebugMenuOption>();
				for (int j = 1; j < 5; j++)
				{
					int marginLocal = j;
					list2.Add(new DebugMenuOption("margin: " + j, DebugMenuOptionMode.Action, delegate
					{
						List<DebugMenuOption> options = new List<DebugMenuOption>
						{
							new DebugMenuOption("Draw from...", DebugMenuOptionMode.Tool, delegate
							{
								DebugBreachPickOnePoint(widthLocal, marginLocal, useAvoidGrid: false);
							}),
							new DebugMenuOption("Draw from (with avoid grid)...", DebugMenuOptionMode.Tool, delegate
							{
								DebugBreachPickOnePoint(widthLocal, marginLocal, useAvoidGrid: true);
							}),
							new DebugMenuOption("Draw between...", DebugMenuOptionMode.Tool, delegate
							{
								DebugBreachPickTwoPoints(widthLocal, marginLocal, useAvoidGrid: false);
							}),
							new DebugMenuOption("Draw between (with avoid grid)...", DebugMenuOptionMode.Tool, delegate
							{
								DebugBreachPickTwoPoints(widthLocal, marginLocal, useAvoidGrid: true);
							})
						};
						Find.WindowStack.Add(new Dialog_DebugOptionListLister(options));
					}));
				}
				Find.WindowStack.Add(new Dialog_DebugOptionListLister(list2));
			}));
		}
		Find.WindowStack.Add(new Dialog_DebugOptionListLister(list));
	}

	public static void Notify_BuildingStateChanged(Building b)
	{
		debugBeachGridForDrawing?.Notify_BuildingStateChanged(b);
	}
}
