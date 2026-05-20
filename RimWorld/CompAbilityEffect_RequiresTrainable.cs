using Verse;

namespace RimWorld
{
	public class CompAbilityEffect_RequiresTrainable : CompAbilityEffect
	{
		public new CompProperties_AbilityRequiresTrainable Props => (CompProperties_AbilityRequiresTrainable)props;

		private bool HasLearnedTrainable => parent.pawn?.training?.HasLearned(Props.trainableDef) == true;

		public override bool ShouldHideGizmo => !HasLearnedTrainable;

		public override bool AICanTargetNow(LocalTargetInfo target)
		{
			if (HasLearnedTrainable)
			{
				return false;
			}
			return Props.aiCanCastWithoutTrainable;
		}
	}
}
