using Verse;

namespace RimWorld;

public class CompProperties_AbilityGiveHediff : CompProperties_AbilityEffectWithDuration
{
	public HediffDef hediffDef;

	public bool onlyBrain;

	public bool applyToSelf;

	public bool onlyApplyToSelf;

	public bool applyToTarget = true;

	public bool replaceExisting;

	public float severity = -1f;

	public bool ignoreSelf;
}
