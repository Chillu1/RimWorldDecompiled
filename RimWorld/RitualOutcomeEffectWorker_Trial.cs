using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class RitualOutcomeEffectWorker_Trial : RitualOutcomeEffectWorker_FromQuality
{
	public const int ConvictGuiltyForDays = 15;

	public override bool SupportsAttachableOutcomeEffect => false;

	public RitualOutcomeEffectWorker_Trial()
	{
	}

	public RitualOutcomeEffectWorker_Trial(RitualOutcomeEffectDef def)
		: base(def)
	{
	}

	public override void Apply(float progress, Dictionary<Pawn, int> totalPresence, LordJob_Ritual jobRitual)
	{
		float quality = GetQuality(jobRitual, progress);
		RitualOutcomePossibility outcome = GetOutcome(quality, jobRitual);
		Pawn pawn = jobRitual.PawnWithRole("leader");
		Pawn pawn2 = jobRitual.PawnWithRole("convict");
		LookTargets letterLookTargets = pawn2;
		string extraLetterText = null;
		if (jobRitual.Ritual != null)
		{
			ApplyAttachableOutcome(totalPresence, jobRitual, outcome, out extraLetterText, ref letterLookTargets);
		}
		string text = pawn2.LabelShort + " " + outcome.label;
		TaggedString text2 = outcome.description.Formatted(pawn2.Named("PAWN"), pawn.Named("PROSECUTOR"));
		string text3 = def.OutcomeMoodBreakdown(outcome);
		if (!text3.NullOrEmpty())
		{
			text2 += "\n\n" + text3;
		}
		text2 += "\n\n" + OutcomeQualityBreakdownDesc(quality, progress, jobRitual);
		if (extraLetterText != null)
		{
			text2 += "\n\n" + extraLetterText;
		}
		if (outcome.Positive)
		{
			ChoiceLetter choiceLetter = LetterMaker.MakeLetter(text, text2, LetterDefOf.RitualOutcomePositive, letterLookTargets);
			Find.LetterStack.ReceiveLetter(choiceLetter);
			pawn2.guilt.Notify_Guilty(900000);
			pawn2.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOf.TrialConvicted);
		}
		else
		{
			Find.LetterStack.ReceiveLetter(text, text2, LetterDefOf.RitualOutcomeNegative, letterLookTargets);
			pawn.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOf.TrialFailed);
			pawn2.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOf.TrialExonerated);
		}
	}

	public override RitualOutcomePossibility GetOutcome(float quality, LordJob_Ritual ritual)
	{
		if (!Rand.Chance(quality))
		{
			return def.outcomeChances[0];
		}
		return def.outcomeChances[1];
	}

	public override string ExpectedQualityLabel()
	{
		return "ExpectedConvictionChance".Translate();
	}
}
