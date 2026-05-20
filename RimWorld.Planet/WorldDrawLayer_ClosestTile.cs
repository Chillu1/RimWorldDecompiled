using UnityEngine;
using Verse;

namespace RimWorld.Planet;

public class WorldDrawLayer_ClosestTile : WorldDrawLayer_SingleTile
{
	protected override PlanetTile Tile
	{
		get
		{
			if (Find.WorldTargeter.IsTargeting)
			{
				return Find.WorldTargeter.ClosestLayerTile;
			}
			return Find.TilePicker.ClosestLayerTile;
		}
	}

	protected override Material Material => WorldMaterials.ClosestTile;

	public override bool VisibleWhenLayerNotSelected => false;

	public override bool VisibleInBackground => false;
}
