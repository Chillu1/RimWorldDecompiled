namespace Verse.AI
{
	public static class BreachingNoiseDebugDrawer
	{
		private static BreachingGrid debugGrid;

		private static IntGrid debugDrawGrid;

		public static void DebugDrawNoise(BreachingGrid grid)
		{
			Map currentMap = Find.CurrentMap;
			CheckInitDebugDrawGrid(grid);
			foreach (IntVec3 allCell in currentMap.AllCells)
			{
				if (debugDrawGrid[allCell] > 0)
				{
					CellRenderer.RenderCell(allCell, (float)debugDrawGrid[allCell] / 100f);
				}
			}
		}

		private static void CheckInitDebugDrawGrid(BreachingGrid grid)
		{
			if (grid != debugGrid)
			{
				debugDrawGrid = null;
				debugGrid = grid;
			}
			if (debugDrawGrid != null)
			{
				return;
			}
			debugDrawGrid = new IntGrid(grid.Map);
			debugDrawGrid.Clear();
			foreach (IntVec3 allCell in grid.Map.AllCells)
			{
				if (debugGrid.WithinNoise(allCell))
				{
					debugDrawGrid[allCell] = 1;
				}
			}
		}
	}
}
