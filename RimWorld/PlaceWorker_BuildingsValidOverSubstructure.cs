using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class PlaceWorker_BuildingsValidOverSubstructure : PlaceWorker
{
	public override AcceptanceReport AllowsPlacing(BuildableDef checkingDef, IntVec3 loc, Rot4 rot, Map map, Thing thingToIgnore = null, Thing thing = null)
	{
		if (!ModsConfig.OdysseyActive)
		{
			return AcceptanceReport.WasAccepted;
		}
		foreach (Thing thing2 in loc.GetThingList(map))
		{
			if (thing2.def == ThingDefOf.GravEngine)
			{
				return AcceptanceReport.WasAccepted;
			}
			if (thing2.def.IsEdifice() && thing2.Faction != Faction.OfPlayer)
			{
				return "MessageSubstructureBlocked".Translate();
			}
			List<PlaceWorker> list = ((!(thing2 is Blueprint blueprint)) ? thing2.def.PlaceWorkers : blueprint.def.entityDefToBuild.PlaceWorkers);
			if (list == null)
			{
				continue;
			}
			foreach (PlaceWorker item in list)
			{
				if (item is PlaceWorker_InvalidOverSubstructure)
				{
					return "MessageSubstructureBlocked".Translate();
				}
			}
		}
		return AcceptanceReport.WasAccepted;
	}
}
