using UnityEngine;
using Verse;

namespace RimWorld.Planet;

public class WorldDrawLayer_MouseTile : WorldDrawLayer_SingleTile
{
	protected override PlanetTile Tile
	{
		get
		{
			if (Find.World.UI.selector.dragBox.IsValidAndActive)
			{
				return PlanetTile.Invalid;
			}
			if (Find.WorldTargeter.IsTargeting)
			{
				return PlanetTile.Invalid;
			}
			if (Find.ScreenshotModeHandler.Active)
			{
				return PlanetTile.Invalid;
			}
			return GenWorld.MouseTile();
		}
	}

	protected override Material Material => WorldMaterials.MouseTile;

	public override bool VisibleWhenLayerNotSelected => false;

	public override bool VisibleInBackground => false;
}
