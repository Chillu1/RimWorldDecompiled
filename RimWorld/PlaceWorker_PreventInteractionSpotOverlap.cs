using Verse;

namespace RimWorld
{
	public class PlaceWorker_PreventInteractionSpotOverlap : PlaceWorker
	{
		public override AcceptanceReport AllowsPlacing(BuildableDef checkingDef, IntVec3 loc, Rot4 rot, Map map, Thing thingToIgnore = null, Thing thingToPlace = null)
		{
			ThingDef thingDef = checkingDef as ThingDef;
			if (thingDef == null || !thingDef.hasInteractionCell)
			{
				return true;
			}
			IntVec3 intVec = ThingUtility.InteractionCellWhenAt(thingDef, loc, rot, map);
			for (int i = -1; i <= 1; i++)
			{
				for (int j = -1; j <= 1; j++)
				{
					IntVec3 c = intVec;
					c.x += i;
					c.z += j;
					if (!c.InBounds(map))
					{
						continue;
					}
					foreach (Thing item in map.thingGrid.ThingsListAtFast(c))
					{
						if (item != thingToIgnore)
						{
							ThingDef thingDef2 = item.def;
							if (item.def.entityDefToBuild != null)
							{
								thingDef2 = (item.def.entityDefToBuild as ThingDef);
							}
							if (thingDef2 != null && thingDef2.hasInteractionCell && ThingUtility.InteractionCellWhenAt(thingDef2, item.Position, item.Rotation, item.Map) == intVec)
							{
								return new AcceptanceReport(((item.def.entityDefToBuild == null) ? "InteractionSpotOverlaps" : "InteractionSpotWillOverlap").Translate(item.LabelNoCount, item));
							}
						}
					}
				}
			}
			return true;
		}
	}
}
