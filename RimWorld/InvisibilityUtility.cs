using System.Collections.Generic;
using Verse;

namespace RimWorld;

public static class InvisibilityUtility
{
	public static bool IsPsychologicallyInvisible(this Pawn pawn)
	{
		List<Hediff> hediffs = pawn.health.hediffSet.hediffs;
		for (int i = 0; i < hediffs.Count; i++)
		{
			HediffComp_Invisibility hediffComp_Invisibility = hediffs[i].TryGetComp<HediffComp_Invisibility>();
			if (hediffComp_Invisibility != null && !hediffComp_Invisibility.PsychologicallyVisible)
			{
				return true;
			}
		}
		return false;
	}

	public static bool IsHiddenFromPlayer(this Pawn pawn)
	{
		if (DebugSettings.showHiddenPawns)
		{
			return false;
		}
		if (pawn.Faction == Faction.OfPlayer)
		{
			return false;
		}
		List<Hediff> hediffs = pawn.health.hediffSet.hediffs;
		for (int i = 0; i < hediffs.Count; i++)
		{
			HediffComp_Invisibility hediffComp_Invisibility = hediffs[i].TryGetComp<HediffComp_Invisibility>();
			if (hediffComp_Invisibility != null && !hediffComp_Invisibility.Props.visibleToPlayer && !hediffComp_Invisibility.PsychologicallyVisible)
			{
				return true;
			}
		}
		return false;
	}

	public static float GetAlpha(Pawn pawn)
	{
		if (DebugSettings.showHiddenPawns)
		{
			return 1f;
		}
		List<Hediff> hediffs = pawn.health.hediffSet.hediffs;
		if (hediffs.NullOrEmpty())
		{
			return 1f;
		}
		foreach (Hediff item in hediffs)
		{
			HediffComp_Invisibility hediffComp_Invisibility = item.TryGetComp<HediffComp_Invisibility>();
			if (hediffComp_Invisibility != null)
			{
				return hediffComp_Invisibility.GetAlpha();
			}
		}
		return 1f;
	}

	public static HediffComp_Invisibility GetInvisibilityComp(this Pawn pawn)
	{
		List<Hediff> hediffs = pawn.health.hediffSet.hediffs;
		if (hediffs.NullOrEmpty())
		{
			return null;
		}
		foreach (Hediff item in hediffs)
		{
			HediffComp_Invisibility hediffComp_Invisibility = item.TryGetComp<HediffComp_Invisibility>();
			if (hediffComp_Invisibility != null)
			{
				return hediffComp_Invisibility;
			}
		}
		return null;
	}
}
