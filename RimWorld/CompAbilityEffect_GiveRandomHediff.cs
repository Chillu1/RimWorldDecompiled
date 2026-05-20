using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld
{
	public class CompAbilityEffect_GiveRandomHediff : CompAbilityEffect
	{
		public new CompProperties_AbilityGiveRandomHediff Props => (CompProperties_AbilityGiveRandomHediff)props;

		public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
		{
			base.Apply(target, dest);
			HediffOption hediffOption = GetApplicableHediffs(target.Pawn).RandomElement();
			BodyPartRecord partRecord = GetAcceptablePartsForHediff(target.Pawn, hediffOption).RandomElement();
			Hediff hediff = HediffMaker.MakeHediff(hediffOption.hediffDef, target.Pawn, partRecord);
			target.Pawn.health.AddHediff(hediff);
			if (base.SendLetter)
			{
				Find.LetterStack.ReceiveLetter(Props.customLetterLabel.Formatted(hediff.def.LabelCap), Props.customLetterText.Formatted(parent.pawn, target.Pawn, hediff.def.label), LetterDefOf.PositiveEvent, new LookTargets(target.Pawn));
			}
		}

		public override bool CanApplyOn(LocalTargetInfo target, LocalTargetInfo dest)
		{
			if (target.Pawn == null)
			{
				return false;
			}
			if (GetApplicableHediffs(target.Pawn).Any())
			{
				return base.CanApplyOn(target, dest);
			}
			return false;
		}

		private List<HediffOption> GetApplicableHediffs(Pawn target)
		{
			List<HediffOption> list = new List<HediffOption>();
			foreach (HediffOption option in Props.options)
			{
				if (!GetAcceptablePartsForHediff(target, option).EnumerableNullOrEmpty())
				{
					list.Add(option);
				}
			}
			return list;
		}

		private IEnumerable<BodyPartRecord> GetAcceptablePartsForHediff(Pawn target, HediffOption option)
		{
			if (!Props.allowDuplicates && target.health.hediffSet.hediffs.Where((Hediff x) => x.def == option.hediffDef).Any())
			{
				return null;
			}
			return from p in target.health.hediffSet.GetNotMissingParts()
				where (option.bodyPart == null || p.def == option.bodyPart) && !target.health.hediffSet.PartOrAnyAncestorHasDirectlyAddedParts(p)
				select p;
		}
	}
}
