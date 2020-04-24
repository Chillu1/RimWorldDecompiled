using UnityEngine;
using Verse;

namespace RimWorld.Planet
{
	public class WorldLayer_SelectedTile : WorldLayer_SingleTile
	{
		protected override int Tile => Find.WorldSelector.selectedTile;

		protected override Material Material => WorldMaterials.SelectedTile;
	}
}
