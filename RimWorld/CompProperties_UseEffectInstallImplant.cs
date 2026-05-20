using Verse;

namespace RimWorld;

public class CompProperties_UseEffectInstallImplant : CompProperties_UseEffect
{
	public HediffDef hediffDef;

	public BodyPartDef bodyPart;

	public bool canUpgrade;

	public bool allowNonColonists;

	public bool requiresExistingHediff;

	public float maxSeverity = float.MaxValue;

	public float minSeverity;

	public bool requiresPsychicallySensitive;

	public CompProperties_UseEffectInstallImplant()
	{
		compClass = typeof(CompUseEffect_InstallImplant);
	}
}
