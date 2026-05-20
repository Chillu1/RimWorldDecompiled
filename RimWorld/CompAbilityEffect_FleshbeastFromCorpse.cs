using Verse;

namespace RimWorld;

public class CompAbilityEffect_FleshbeastFromCorpse : CompAbilityEffect
{
	public new CompProperties_FleshbeastFromCorpse Props => (CompProperties_FleshbeastFromCorpse)props;

	public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
	{
		base.Apply(target, dest);
		if (target.Thing is Corpse { InnerPawn: not null } corpse)
		{
			FleshbeastUtility.SpawnFleshbeastFromPawn(corpse.InnerPawn, false, true);
		}
	}

	public override bool AICanTargetNow(LocalTargetInfo target)
	{
		return false;
	}
}
