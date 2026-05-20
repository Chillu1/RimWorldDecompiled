using Verse;

namespace RimWorld
{
	public class CompTargetEffect_Manhunter : CompTargetEffect
	{
		public override void DoEffectOn(Pawn user, Thing target)
		{
			Pawn pawn = (Pawn)target;
			if (!pawn.Dead)
			{
				if (!pawn.Awake())
				{
					RestUtility.WakeUp(pawn);
				}
				pawn.mindState.mentalStateHandler.TryStartMentalState(MentalStateDefOf.Manhunter);
			}
		}
	}
}
