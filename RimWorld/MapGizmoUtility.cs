using System.Collections.Generic;
using Verse;

namespace RimWorld;

[StaticConstructorOnStartup]
public static class MapGizmoUtility
{
	private static Gizmo mouseoverGizmo;

	private static Gizmo lastMouseOverGizmo;

	private static int cacheFrame;

	private static readonly List<object> tmpObjectsList = new List<object>();

	public static Gizmo LastMouseOverGizmo => lastMouseOverGizmo;

	public static void MapUIOnGUI()
	{
		if (Find.MainTabsRoot.OpenTab == null || Find.MainTabsRoot.OpenTab == MainButtonDefOf.Inspect)
		{
			tmpObjectsList.Clear();
			tmpObjectsList.AddRange(Find.Selector.SelectedObjects);
			GizmoGridDrawer.DrawGizmoGridFor(tmpObjectsList, out mouseoverGizmo);
		}
	}

	public static void MapUIUpdate()
	{
		lastMouseOverGizmo = mouseoverGizmo;
		if (mouseoverGizmo != null)
		{
			mouseoverGizmo.GizmoUpdateOnMouseover();
		}
	}
}
