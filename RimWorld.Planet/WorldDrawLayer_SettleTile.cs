using UnityEngine;
using Verse;

namespace RimWorld.Planet;

public class WorldDrawLayer_SettleTile : WorldDrawLayer_SingleTile
{
	protected override PlanetTile Tile
	{
		get
		{
			if (!(WorldGizmoUtility.LastMouseOverGizmo is Command_Settle))
			{
				return PlanetTile.Invalid;
			}
			if (!(Find.WorldSelector.SingleSelectedObject is Caravan caravan))
			{
				return PlanetTile.Invalid;
			}
			return caravan.Tile;
		}
	}

	protected override Material Material => WorldMaterials.CurrentMapTile;

	protected override float Alpha => Mathf.Abs(Time.time % 2f - 1f);
}
