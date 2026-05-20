using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class PlaceWorker_BandNode : PlaceWorker
	{
		public override AcceptanceReport AllowsPlacing(BuildableDef def, IntVec3 center, Rot4 rot, Map map, Thing thingToIgnore = null, Thing thing = null)
		{
			foreach (IntVec3 item in GenAdj.OccupiedRect(center, rot, def.Size).ExpandedBy(1))
			{
				if (!item.InBounds(map))
				{
					continue;
				}
				List<Thing> thingList = item.GetThingList(map);
				for (int i = 0; i < thingList.Count; i++)
				{
					if (thingList[i].def == def || thingList[i].def.entityDefToBuild == def)
					{
						return "MustNotBePlacedCloseToAnother".Translate(def);
					}
				}
			}
			return true;
		}
	}
}
