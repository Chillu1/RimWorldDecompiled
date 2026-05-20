using Verse;

namespace RimWorld;

public class CompProperties_Targetable : CompProperties_UseEffect
{
	public bool psychicSensitiveTargetsOnly;

	public bool fleshCorpsesOnly;

	public bool nonDessicatedCorpsesOnly;

	public bool nonDownedPawnOnly;

	public bool ignoreQuestLodgerPawns;

	public bool ignorePlayerFactionPawns;

	public MutantDef mutantFilter;

	public ThingDef moteOnTarget;

	public ThingDef moteConnecting;

	public FleckDef fleckOnTarget;

	public FleckDef fleckConnecting;

	public HediffDef cannotHaveHediff;

	public CompProperties_Targetable()
	{
		compClass = typeof(CompTargetable);
	}
}
