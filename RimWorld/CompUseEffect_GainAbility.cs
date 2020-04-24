using Verse;

namespace RimWorld
{
	public class CompUseEffect_GainAbility : CompUseEffect
	{
		private AbilityDef Ability => parent.GetComp<CompNeurotrainer>().ability;

		public override void DoEffect(Pawn user)
		{
			base.DoEffect(user);
			AbilityDef ability = Ability;
			user.abilities.GainAbility(ability);
			if (PawnUtility.ShouldSendNotificationAbout(user))
			{
				Messages.Message("AbilityNeurotrainerUsed".Translate(user.Named("USER"), ability.LabelCap), user, MessageTypeDefOf.PositiveEvent);
			}
		}

		public override bool CanBeUsedBy(Pawn p, out string failReason)
		{
			if (!p.health.hediffSet.HasHediff(HediffDefOf.PsychicAmplifier))
			{
				failReason = "PsycastNeurotrainerNoPsychicAmplifier".Translate();
				return false;
			}
			if (p.abilities != null && p.abilities.abilities.Any((Ability a) => a.def == Ability))
			{
				failReason = "PsycastNeurotrainerAbilityAlreadyLearned".Translate(p.Named("USER"), Ability.LabelCap);
				return false;
			}
			return base.CanBeUsedBy(p, out failReason);
		}
	}
}
