using UnityEngine;
using Verse;

namespace RimWorld.Planet
{
	public class WorldLayer_SettleTile : WorldLayer_SingleTile
	{
		protected override int Tile
		{
			get
			{
				if (!(Find.WorldInterface.inspectPane.mouseoverGizmo is Command_Settle))
				{
					return -1;
				}
				return (Find.WorldSelector.SingleSelectedObject as Caravan)?.Tile ?? (-1);
			}
		}

		protected override Material Material => WorldMaterials.CurrentMapTile;

		protected override float Alpha => Mathf.Abs(Time.time % 2f - 1f);
	}
}
