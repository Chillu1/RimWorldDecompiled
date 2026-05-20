using Verse;

namespace RimWorld;

public class CompUseEffect_GainAbility : CompUseEffect
{
	public CompProperties_UseEffect_GainAbility Props => (CompProperties_UseEffect_GainAbility)props;

	public override void DoEffect(Pawn user)
	{
		base.DoEffect(user);
		user.abilities.GainAbility(Props.ability);
		if (PawnUtility.ShouldSendNotificationAbout(user))
		{
			Messages.Message("AbilityNeurotrainerUsed".Translate(user.Named("USER"), Props.ability.LabelCap), user, MessageTypeDefOf.PositiveEvent);
		}
	}

	public override AcceptanceReport CanBeUsedBy(Pawn p)
	{
		if (!p.health.hediffSet.HasHediff(HediffDefOf.PsychicAmplifier))
		{
			return "PsycastNeurotrainerNoPsylink".TranslateWithBackup("PsycastNeurotrainerNoPsychicAmplifier");
		}
		if (p.abilities != null && p.abilities.abilities.Any((Ability a) => a.def == Props.ability))
		{
			return "PsycastNeurotrainerAbilityAlreadyLearned".Translate(p.Named("USER"), Props.ability.LabelCap);
		}
		return base.CanBeUsedBy(p);
	}

	public override TaggedString ConfirmMessage(Pawn p)
	{
		if (!Props.ability.IsPsycast)
		{
			return null;
		}
		Hediff firstHediffOfDef = p.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.PsychicAmplifier);
		if (firstHediffOfDef == null)
		{
			return null;
		}
		if (Props.ability.level > ((Hediff_Level)firstHediffOfDef).level)
		{
			return "PsylinkTooLowForGainAbility".Translate(p.Named("PAWN"), Props.ability.label.Named("ABILITY"));
		}
		return null;
	}

	public override bool AllowStackWith(Thing other)
	{
		if (!base.AllowStackWith(other))
		{
			return false;
		}
		CompUseEffect_GainAbility compUseEffect_GainAbility = other.TryGetComp<CompUseEffect_GainAbility>();
		if (compUseEffect_GainAbility == null || compUseEffect_GainAbility.Props.ability != Props.ability)
		{
			return false;
		}
		return true;
	}
}
