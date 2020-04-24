using Verse;

namespace RimWorld
{
	public class CompTargetEffect_Berserk : CompTargetEffect
	{
		public override void DoEffectOn(Pawn user, Thing target)
		{
			Pawn pawn = (Pawn)target;
			if (!pawn.Dead)
			{
				pawn.mindState.mentalStateHandler.TryStartMentalState(MentalStateDefOf.Berserk, null, forceWake: true);
			}
		}
	}
}
