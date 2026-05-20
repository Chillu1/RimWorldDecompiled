using UnityEngine;
using Verse;

namespace RimWorld.Planet;

public class WorldDrawLayer_SelectedTile : WorldDrawLayer_SingleTile
{
	protected override PlanetTile Tile => Find.WorldSelector.SelectedTile;

	protected override Material Material => WorldMaterials.SelectedTile;

	public override bool VisibleWhenLayerNotSelected => false;

	public override bool VisibleInBackground => false;
}
