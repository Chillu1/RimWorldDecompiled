using UnityEngine;
using Verse;

namespace RimWorld.Planet;

public class WorldDrawLayer_CurrentMapTile : WorldDrawLayer_SingleTile
{
	protected override PlanetTile Tile
	{
		get
		{
			if (Current.ProgramState != ProgramState.Playing)
			{
				return PlanetTile.Invalid;
			}
			if (Find.CurrentMap == null)
			{
				return PlanetTile.Invalid;
			}
			return Find.CurrentMap.Tile;
		}
	}

	protected override Material Material => WorldMaterials.CurrentMapTile;

	public override bool VisibleWhenLayerNotSelected => false;

	public override bool VisibleInBackground => false;
}
