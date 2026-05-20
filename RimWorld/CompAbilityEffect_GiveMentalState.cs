using Verse;

namespace RimWorld;

public class CompAbilityEffect_GiveMentalState : CompAbilityEffect
{
	public new CompProperties_AbilityGiveMentalState Props => (CompProperties_AbilityGiveMentalState)props;

	public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
	{
		base.Apply(target, dest);
		Pawn pawn = (Props.applyToSelf ? parent.pawn : (target.Thing as Pawn));
		if (pawn != null && !pawn.InMentalState)
		{
			TryGiveMentalState(pawn.RaceProps.IsMechanoid ? (Props.stateDefForMechs ?? Props.stateDef) : Props.stateDef, pawn, parent.def, Props.durationMultiplier, parent.pawn, Props.forced);
			RestUtility.WakeUp(pawn);
			if (Props.casterEffect != null)
			{
				Effecter effecter = Props.casterEffect.SpawnAttached(parent.pawn, parent.pawn.MapHeld);
				effecter.Trigger(parent.pawn, null);
				effecter.Cleanup();
			}
			if (Props.targetEffect != null)
			{
				Effecter effecter2 = Props.targetEffect.SpawnAttached(parent.pawn, parent.pawn.MapHeld);
				effecter2.Trigger(pawn, null);
				effecter2.Cleanup();
			}
		}
	}

	public override bool Valid(LocalTargetInfo target, bool throwMessages = false)
	{
		if (!base.Valid(target, throwMessages))
		{
			return false;
		}
		Pawn pawn = target.Pawn;
		if (pawn != null)
		{
			if (!AbilityUtility.ValidateNoMentalState(pawn, throwMessages, parent))
			{
				return false;
			}
			if (Props.excludeNPCFactions && pawn.Faction != null && !pawn.Faction.IsPlayer)
			{
				if (throwMessages)
				{
					Messages.Message("CannotUseAbility".Translate(parent.def.label) + ": " + "TargetBelongsToNPCFaction".Translate(), pawn, MessageTypeDefOf.RejectInput, historical: false);
				}
				return false;
			}
		}
		return true;
	}

	public static void TryGiveMentalState(MentalStateDef def, Pawn p, AbilityDef ability, StatDef multiplierStat, Pawn caster, bool forced = false)
	{
		if (p.mindState.mentalStateHandler.TryStartMentalState(def, null, forced, forceWake: true, causedByMood: false, caster, transitionSilently: false, causedByDamage: false, ability.IsPsycast))
		{
			float num = ability.GetStatValueAbstract(StatDefOf.Ability_Duration, caster);
			if (multiplierStat != null)
			{
				num *= p.GetStatValue(multiplierStat);
			}
			if (num > 0f)
			{
				p.mindState.mentalStateHandler.CurState.forceRecoverAfterTicks = num.SecondsToTicks();
			}
			p.mindState.mentalStateHandler.CurState.sourceFaction = caster.Faction;
		}
	}
}
