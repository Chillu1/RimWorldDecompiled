using UnityEngine;
using Verse;

namespace RimWorld
{
	public class Designator_RemoveBridge : Designator_RemoveFloor
	{
		public Designator_RemoveBridge()
		{
			defaultLabel = "DesignatorRemoveBridge".Translate();
			defaultDesc = "DesignatorRemoveBridgeDesc".Translate();
			icon = ContentFinder<Texture2D>.Get("UI/Designators/RemoveBridge");
			hotKey = KeyBindingDefOf.Misc5;
		}

		public override AcceptanceReport CanDesignateCell(IntVec3 c)
		{
			if (c.InBounds(base.Map) && c.GetTerrain(base.Map) != TerrainDefOf.Bridge)
			{
				return false;
			}
			return base.CanDesignateCell(c);
		}
	}
}
