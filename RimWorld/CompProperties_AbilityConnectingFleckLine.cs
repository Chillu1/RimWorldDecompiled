using Verse;

namespace RimWorld;

public class CompProperties_AbilityConnectingFleckLine : CompProperties_AbilityEffect
{
	public FleckDef fleckDef;

	public CompProperties_AbilityConnectingFleckLine()
	{
		compClass = typeof(CompAbilityEffect_ConnectingFleckLine);
	}
}
