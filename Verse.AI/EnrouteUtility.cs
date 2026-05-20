using RimWorld;
using UnityEngine;

namespace Verse.AI;

public static class EnrouteUtility
{
	public static int GetSpaceRemainingWithEnroute(this IHaulEnroute enroute, ThingDef stuff, Pawn excludeEnrouteFor = null)
	{
		int enroute2 = enroute.Map.enrouteManager.GetEnroute(enroute, stuff, excludeEnrouteFor);
		return Mathf.Max(enroute.SpaceRemainingFor(stuff) - enroute2, 0);
	}
}
