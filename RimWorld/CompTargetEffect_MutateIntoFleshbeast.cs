using Verse;

namespace RimWorld;

public class CompTargetEffect_MutateIntoFleshbeast : CompTargetEffect
{
	private static readonly IntRange BloodFilth = new IntRange(3, 4);

	public override void DoEffectOn(Pawn user, Thing target)
	{
		if (target is Pawn pawn)
		{
			FleshbeastUtility.MeatSplatter(BloodFilth.RandomInRange, pawn.PositionHeld, pawn.MapHeld, FleshbeastUtility.MeatExplosionSize.Large);
			FleshbeastUtility.SpawnFleshbeastFromPawn(pawn, false, true);
		}
	}
}
