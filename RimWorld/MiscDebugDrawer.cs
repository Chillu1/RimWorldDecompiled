using Verse;

namespace RimWorld;

public static class MiscDebugDrawer
{
	public static void DebugDrawInteractionCells()
	{
		if (Find.CurrentMap == null || !DebugViewSettings.drawInteractionCells)
		{
			return;
		}
		foreach (object selectedObject in Find.Selector.SelectedObjects)
		{
			if (selectedObject is Thing thing)
			{
				CellRenderer.RenderCell(thing.InteractionCell);
			}
		}
	}
}
