using UnityEngine;
using Verse;

namespace RimWorld;

public class PlaceWorker_InRangeOfGravEngine : PlaceWorker
{
	public override void DrawGhost(ThingDef def, IntVec3 center, Rot4 rot, Color ghostCol, Thing thing = null)
	{
		Map currentMap = Find.CurrentMap;
		CompProperties_GravshipFacility compProperties = def.GetCompProperties<CompProperties_GravshipFacility>();
		foreach (Thing item in currentMap.listerThings.ThingsOfDef(ThingDefOf.GravEngine))
		{
			GenDraw.DrawLineBetween(color: (!center.InHorDistOf(item.Position, compProperties.maxDistance)) ? SimpleColor.Red : SimpleColor.Green, A: center.ToVector3Shifted(), B: item.TrueCenter());
		}
	}

	public override AcceptanceReport AllowsPlacing(BuildableDef checkingDef, IntVec3 loc, Rot4 rot, Map map, Thing thingToIgnore = null, Thing thing = null)
	{
		if (!(checkingDef is ThingDef thingDef))
		{
			return AcceptanceReport.WasRejected;
		}
		CompProperties_GravshipFacility compProperties = thingDef.GetCompProperties<CompProperties_GravshipFacility>();
		foreach (Thing item in map.listerThings.ThingsOfDef(ThingDefOf.GravEngine))
		{
			if (loc.InHorDistOf(item.Position, compProperties.maxDistance))
			{
				return AcceptanceReport.WasAccepted;
			}
		}
		return "MessageMustBePlacedInRangeOfGravEngine".Translate();
	}
}
