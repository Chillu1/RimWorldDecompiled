using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class RitualOutcomeEffectWorker_Execution : RitualOutcomeEffectWorker_FromQuality
{
	public override bool SupportsAttachableOutcomeEffect => false;

	public RitualOutcomeEffectWorker_Execution()
	{
	}

	public RitualOutcomeEffectWorker_Execution(RitualOutcomeEffectDef def)
		: base(def)
	{
	}

	public override void Apply(float progress, Dictionary<Pawn, int> totalPresence, LordJob_Ritual jobRitual)
	{
		float quality = GetQuality(jobRitual, progress);
		RitualOutcomePossibility outcome = GetOutcome(quality, jobRitual);
		LookTargets letterLookTargets = jobRitual.selectedTarget;
		string extraLetterText = null;
		if (jobRitual.Ritual != null)
		{
			ApplyAttachableOutcome(totalPresence, jobRitual, outcome, out extraLetterText, ref letterLookTargets);
		}
		bool flag = false;
		foreach (Pawn key in totalPresence.Keys)
		{
			if (key.IsSlave)
			{
				if (key.needs.TryGetNeed(out Need_Suppression need))
				{
					need.CurLevel = 1f;
				}
				flag = true;
			}
			else
			{
				GiveMemoryToPawn(key, outcome.memory, jobRitual);
			}
		}
		string text = outcome.description.Formatted(jobRitual.Ritual.Label).CapitalizeFirst() + "\n\n" + OutcomeQualityBreakdownDesc(quality, progress, jobRitual);
		string text2 = def.OutcomeMoodBreakdown(outcome);
		if (!text2.NullOrEmpty())
		{
			text = text + "\n\n" + text2;
		}
		if (flag)
		{
			text += "\n\n" + "RitualOutcomeExtraDesc_Execution".Translate();
		}
		if (extraLetterText != null)
		{
			text = text + "\n\n" + extraLetterText;
		}
		ApplyDevelopmentPoints(jobRitual.Ritual, outcome, out var extraOutcomeDesc);
		if (extraOutcomeDesc != null)
		{
			text = text + "\n\n" + extraOutcomeDesc;
		}
		Find.LetterStack.ReceiveLetter("OutcomeLetterLabel".Translate(outcome.label.Named("OUTCOMELABEL"), jobRitual.Ritual.Label.Named("RITUALLABEL")), text, outcome.Positive ? LetterDefOf.RitualOutcomePositive : LetterDefOf.RitualOutcomeNegative, letterLookTargets);
	}
}
