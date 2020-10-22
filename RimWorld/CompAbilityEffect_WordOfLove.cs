using Verse;

namespace RimWorld
{
	public class CompAbilityEffect_WordOfLove : CompAbilityEffect_WithDest
	{
		public override TargetingParameters targetParams => new TargetingParameters
		{
			canTargetSelf = true,
			canTargetBuildings = false,
			canTargetAnimals = false,
			canTargetMechs = false
		};

		public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
		{
			base.Apply(target, dest);
			Pawn pawn = target.Pawn;
			Hediff firstHediffOfDef = pawn.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.PsychicLove);
			if (firstHediffOfDef != null)
			{
				pawn.health.RemoveHediff(firstHediffOfDef);
			}
			Hediff_PsychicLove hediff_PsychicLove = (Hediff_PsychicLove)HediffMaker.MakeHediff(HediffDefOf.PsychicLove, pawn, pawn.health.hediffSet.GetBrain());
			hediff_PsychicLove.target = dest.Thing;
			HediffComp_Disappears hediffComp_Disappears = hediff_PsychicLove.TryGetComp<HediffComp_Disappears>();
			if (hediffComp_Disappears != null)
			{
				float effectDuration = parent.def.EffectDuration;
				effectDuration *= pawn.GetStatValue(StatDefOf.PsychicSensitivity);
				hediffComp_Disappears.ticksToDisappear = effectDuration.SecondsToTicks();
			}
			pawn.health.AddHediff(hediff_PsychicLove);
		}

		public override bool CanApplyOn(LocalTargetInfo target, LocalTargetInfo dest)
		{
			return Valid(target);
		}

		public override bool ValidateTarget(LocalTargetInfo target)
		{
			Pawn pawn = selectedTarget.Pawn;
			Pawn pawn2 = target.Pawn;
			if (pawn == pawn2)
			{
				return false;
			}
			if (pawn != null && pawn2 != null && !pawn.story.traits.HasTrait(TraitDefOf.Bisexual))
			{
				Gender gender = pawn.gender;
				Gender gender2 = (pawn.story.traits.HasTrait(TraitDefOf.Gay) ? gender : gender.Opposite());
				if (pawn2.gender != gender2)
				{
					Messages.Message("AbilityCantApplyWrongAttractionGender".Translate(pawn, pawn2), pawn, MessageTypeDefOf.RejectInput, historical: false);
					return false;
				}
			}
			return base.ValidateTarget(target);
		}

		public override bool Valid(LocalTargetInfo target, bool throwMessages = false)
		{
			Pawn pawn = target.Pawn;
			if (pawn != null)
			{
				if (pawn.story.traits.HasTrait(TraitDefOf.Asexual))
				{
					if (throwMessages)
					{
						Messages.Message("AbilityCantApplyOnAsexual".Translate(parent.def.label), pawn, MessageTypeDefOf.RejectInput, historical: false);
					}
					return false;
				}
				if (!AbilityUtility.ValidateNoMentalState(pawn, throwMessages))
				{
					return false;
				}
			}
			return true;
		}

		public override string ExtraLabel(LocalTargetInfo target)
		{
			if (selectedTarget.IsValid)
			{
				return "PsychicLoveFor".Translate();
			}
			return "PsychicLoveInduceIn".Translate();
		}
	}
}
