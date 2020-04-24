using UnityEngine;
using Verse;

namespace RimWorld.Planet
{
	public class WorldLayer_MouseTile : WorldLayer_SingleTile
	{
		protected override int Tile
		{
			get
			{
				if (Find.World.UI.selector.dragBox.IsValidAndActive)
				{
					return -1;
				}
				if (Find.WorldTargeter.IsTargeting)
				{
					return -1;
				}
				if (Find.ScreenshotModeHandler.Active)
				{
					return -1;
				}
				return GenWorld.MouseTile();
			}
		}

		protected override Material Material => WorldMaterials.MouseTile;
	}
}
