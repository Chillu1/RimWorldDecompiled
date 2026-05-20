using RimWorld;

namespace Verse.AI;

public class Verb_CastTargetEffect : Verb_CastBase
{
	protected override bool TryCastShot()
	{
		Pawn casterPawn = CasterPawn;
		Thing thing = currentTarget.Thing;
		if (casterPawn == null || thing == null)
		{
			return false;
		}
		foreach (CompTargetEffect comp in base.EquipmentSource.GetComps<CompTargetEffect>())
		{
			comp.DoEffectOn(casterPawn, thing);
		}
		base.ReloadableCompSource?.UsedOnce();
		return true;
	}
}
