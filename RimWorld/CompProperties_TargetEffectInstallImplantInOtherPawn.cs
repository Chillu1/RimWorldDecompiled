using Verse;

namespace RimWorld;

public class CompProperties_TargetEffectInstallImplantInOtherPawn : CompProperties
{
	public HediffDef hediffDef;

	public BodyPartDef bodyPart;

	public bool canUpgrade;

	public bool requiresExistingHediff;

	public SoundDef soundOnUsed;

	public CompProperties_TargetEffectInstallImplantInOtherPawn()
	{
		compClass = typeof(CompTargetEffect_InstallImplantInOtherPawn);
	}
}
