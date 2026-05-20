using Verse;

namespace RimWorld
{
	public class CompAbilityEffect_StartTrial : CompAbilityEffect_StartRitualOnPawn
	{
		public new CompProperties_AbilityStartTrial Props => (CompProperties_AbilityStartTrial)props;

		protected override Precept_Ritual RitualForTarget(Pawn pawn)
		{
			PreceptDef preceptDef = (pawn.InMentalState ? Props.ritualDefForMentalState : (pawn.IsPrisonerOfColony ? Props.ritualDefForPrisoner : null));
			if (preceptDef != null)
			{
				for (int i = 0; i < parent.pawn.Ideo.PreceptsListForReading.Count; i++)
				{
					if (parent.pawn.Ideo.PreceptsListForReading[i].def == preceptDef)
					{
						return (Precept_Ritual)parent.pawn.Ideo.PreceptsListForReading[i];
					}
				}
			}
			return base.RitualForTarget(pawn);
		}

		public override bool Valid(LocalTargetInfo target, bool throwMessages = false)
		{
			Pawn pawn = target.Pawn;
			if (pawn == null)
			{
				return false;
			}
			if (!AbilityUtility.ValidateCanWalk(pawn, throwMessages, parent))
			{
				return false;
			}
			if (!AbilityUtility.ValidateNotGuilty(pawn, throwMessages, parent))
			{
				return false;
			}
			return base.Valid(target, throwMessages);
		}
	}
}
