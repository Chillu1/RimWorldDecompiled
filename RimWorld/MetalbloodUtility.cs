using System.Collections.Generic;
using Verse;

namespace RimWorld;

public static class MetalbloodUtility
{
	public static bool HasMetalblood(this Pawn pawn)
	{
		if (!ModsConfig.AnomalyActive)
		{
			return false;
		}
		List<Hediff> hediffs = pawn.health.hediffSet.hediffs;
		for (int i = 0; i < hediffs.Count; i++)
		{
			if (hediffs[i].def == HediffDefOf.Metalblood)
			{
				return true;
			}
		}
		return false;
	}
}
