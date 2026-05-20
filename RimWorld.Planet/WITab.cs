using System.Linq;
using Verse;

namespace RimWorld.Planet;

public abstract class WITab : InspectTabBase
{
	protected WorldObject SelObject => Find.WorldSelector.SingleSelectedObject;

	protected PlanetTile SelPlanetTile => SelObject?.Tile ?? Find.WorldSelector.SelectedTile;

	protected Tile SelTile => Find.WorldGrid[SelPlanetTile];

	protected Caravan SelCaravan => SelObject as Caravan;

	private WorldInspectPane InspectPane => Find.World.UI.inspectPane;

	protected override bool StillValid
	{
		get
		{
			if (!WorldRendererUtility.WorldSelected)
			{
				return false;
			}
			if (!Find.WindowStack.IsOpen<WorldInspectPane>())
			{
				return false;
			}
			if (InspectPane.CurTabs != null)
			{
				return InspectPane.CurTabs.Contains(this);
			}
			return false;
		}
	}

	protected override float PaneTopY => InspectPane.PaneTopY;

	protected override void CloseTab()
	{
		InspectPane.CloseOpenTab();
	}
}
