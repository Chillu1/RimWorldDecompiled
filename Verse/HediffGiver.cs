using System.Collections.Generic;
using RimWorld;
using RimWorld.Planet;

namespace Verse
{
	public abstract class HediffGiver
	{
		[TranslationHandle]
		public HediffDef hediff;

		public List<BodyPartDef> partsToAffect;

		public bool canAffectAnyLivePart;

		public bool allowOnLodgers = true;

		public bool allowOnQuestRewardPawns = true;

		public int countToAffect = 1;

		public virtual void OnIntervalPassed(Pawn pawn, Hediff cause)
		{
		}

		public virtual bool OnHediffAdded(Pawn pawn, Hediff hediff)
		{
			return false;
		}

		public bool TryApply(Pawn pawn, List<Hediff> outAddedHediffs = null)
		{
			if (!allowOnLodgers && pawn.IsQuestLodger())
			{
				return false;
			}
			if (!allowOnQuestRewardPawns && pawn.IsWorldPawn() && pawn.IsQuestReward())
			{
				return false;
			}
			return HediffGiverUtility.TryApply(pawn, hediff, partsToAffect, canAffectAnyLivePart, countToAffect, outAddedHediffs);
		}

		protected void SendLetter(Pawn pawn, Hediff cause)
		{
			if (PawnUtility.ShouldSendNotificationAbout(pawn))
			{
				if (cause == null)
				{
					Find.LetterStack.ReceiveLetter("LetterHediffFromRandomHediffGiverLabel".Translate(pawn.LabelShortCap, hediff.LabelCap, pawn.Named("PAWN")).CapitalizeFirst(), "LetterHediffFromRandomHediffGiver".Translate(pawn.LabelShortCap, hediff.LabelCap, pawn.Named("PAWN")).CapitalizeFirst(), LetterDefOf.NegativeEvent, pawn);
				}
				else
				{
					Find.LetterStack.ReceiveLetter("LetterHealthComplicationsLabel".Translate(pawn.LabelShort, hediff.LabelCap, pawn.Named("PAWN")).CapitalizeFirst(), "LetterHealthComplications".Translate(pawn.LabelShortCap, hediff.LabelCap, cause.LabelCap, pawn.Named("PAWN")).CapitalizeFirst(), LetterDefOf.NegativeEvent, pawn);
				}
			}
		}

		public virtual IEnumerable<string> ConfigErrors()
		{
			if (hediff == null)
			{
				yield return "hediff is null";
			}
		}
	}
}
