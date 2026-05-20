using System.Collections.Generic;
using RimWorld;
using RimWorld.Planet;

namespace Verse;

public abstract class HediffGiver
{
	[TranslationHandle]
	public HediffDef hediff;

	public List<BodyPartDef> partsToAffect;

	public bool canAffectAnyLivePart;

	public bool allowOnLodgers = true;

	public bool allowOnQuestRewardPawns = true;

	public bool allowOnQuestReservedPawns = true;

	public bool allowOnBeggars = true;

	public int countToAffect = 1;

	public virtual void OnIntervalPassed(Pawn pawn, Hediff cause)
	{
	}

	public virtual float ChanceFactor(Pawn pawn)
	{
		if (ModsConfig.BiotechActive && hediff == HediffDefOf.Carcinoma)
		{
			return pawn.GetStatValue(StatDefOf.CancerRate);
		}
		return 1f;
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
		if (!allowOnQuestReservedPawns && pawn.IsWorldPawn() && Find.WorldPawns.GetSituation(pawn) == WorldPawnSituation.ReservedByQuest)
		{
			return false;
		}
		if (ModsConfig.IdeologyActive && !allowOnBeggars && pawn.kindDef == PawnKindDefOf.Beggar)
		{
			return false;
		}
		if (pawn.ageTracker.CurLifeStage == LifeStageDefOf.HumanlikeBaby && Find.Storyteller.difficulty.babiesAreHealthy)
		{
			return false;
		}
		if (pawn.genes != null && !pawn.genes.HediffGiversCanGive(hediff))
		{
			return false;
		}
		if (pawn.IsMutant && !pawn.mutant.HediffGiversCanGive(hediff))
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
