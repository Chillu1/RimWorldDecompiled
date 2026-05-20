using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class RitualOutcomeEffectWorker_Speech : RitualOutcomeEffectWorker_FromQuality
{
	private static readonly float InspirationChanceFromInspirationalSpeech = 0.05f;

	private static readonly float ConversionChanceFromInspirationalSpeech = 0.02f;

	public override bool SupportsAttachableOutcomeEffect => false;

	public RitualOutcomeEffectWorker_Speech()
	{
	}

	public RitualOutcomeEffectWorker_Speech(RitualOutcomeEffectDef def)
		: base(def)
	{
	}

	public override void Apply(float progress, Dictionary<Pawn, int> totalPresence, LordJob_Ritual jobRitual)
	{
		Pawn organizer = jobRitual.Organizer;
		float quality = GetQuality(jobRitual, progress);
		RitualOutcomePossibility outcome = GetOutcome(quality, jobRitual);
		ThoughtDef memory = outcome.memory;
		LookTargets letterLookTargets = organizer;
		string extraLetterText = null;
		if (jobRitual.Ritual != null)
		{
			ApplyAttachableOutcome(totalPresence, jobRitual, outcome, out extraLetterText, ref letterLookTargets);
		}
		string text = "";
		string text2 = "";
		foreach (KeyValuePair<Pawn, int> item in totalPresence)
		{
			Pawn key = item.Key;
			if (key == organizer || !organizer.Position.InHorDistOf(key.Position, 18f))
			{
				continue;
			}
			Thought_Memory thought_Memory = MakeMemory(key, jobRitual, memory);
			thought_Memory.otherPawn = organizer;
			thought_Memory.moodPowerFactor = ((key.Ideo == organizer.Ideo) ? 1f : 0.5f);
			key.needs.mood.thoughts.memories.TryGainMemory(thought_Memory);
			if (memory != ThoughtDefOf.InspirationalSpeech)
			{
				continue;
			}
			if (Rand.Chance(InspirationChanceFromInspirationalSpeech))
			{
				InspirationDef randomAvailableInspirationDef = key.mindState.inspirationHandler.GetRandomAvailableInspirationDef();
				if (randomAvailableInspirationDef != null && key.mindState.inspirationHandler.TryStartInspiration(randomAvailableInspirationDef, "LetterSpeechInspiration".Translate(key.Named("PAWN"), organizer.Named("SPEAKER"))))
				{
					text = text + "  - " + key.NameShortColored.Resolve() + "\n";
				}
			}
			if (ModsConfig.IdeologyActive && key.Ideo != organizer.Ideo && Rand.Chance(ConversionChanceFromInspirationalSpeech))
			{
				key.ideo.SetIdeo(organizer.Ideo);
				text2 = text2 + "  - " + key.NameShortColored.Resolve() + "\n";
			}
		}
		TaggedString text3 = "LetterFinishedSpeech".Translate(organizer.Named("ORGANIZER")).CapitalizeFirst() + " " + ("Letter" + memory.defName).Translate() + "\n\n" + OutcomeQualityBreakdownDesc(quality, progress, jobRitual);
		if (!text2.NullOrEmpty())
		{
			text3 += "\n\n" + "LetterSpeechConvertedListeners".Translate(organizer.Named("PAWN"), organizer.Ideo.Named("IDEO")).CapitalizeFirst() + ":\n\n" + text2.TrimEndNewlines();
		}
		if (!text.NullOrEmpty())
		{
			text3 += "\n\n" + "LetterSpeechInspiredListeners".Translate() + "\n\n" + text.TrimEndNewlines();
		}
		if (progress < 1f)
		{
			text3 += "\n\n" + "LetterSpeechInterrupted".Translate(progress.ToStringPercent(), organizer.Named("ORGANIZER"));
		}
		if (extraLetterText != null)
		{
			text3 += "\n\n" + extraLetterText;
		}
		ApplyDevelopmentPoints(jobRitual.Ritual, outcome, out var extraOutcomeDesc);
		if (extraOutcomeDesc != null)
		{
			text3 += "\n\n" + extraOutcomeDesc;
		}
		Find.LetterStack.ReceiveLetter("OutcomeLetterLabel".Translate(outcome.label.Named("OUTCOMELABEL"), jobRitual.Ritual.Label.Named("RITUALLABEL")), text3, PositiveOutcome(memory) ? LetterDefOf.RitualOutcomePositive : LetterDefOf.RitualOutcomeNegative, letterLookTargets);
		if (jobRitual.Ritual.def == PreceptDefOf.ThroneSpeech)
		{
			Ability ability = organizer.abilities.GetAbility(AbilityDefOf.Speech, includeTemporary: true);
			RoyalTitle mostSeniorTitle = organizer.royalty.MostSeniorTitle;
			if (ability != null && mostSeniorTitle != null)
			{
				ability.StartCooldown(mostSeniorTitle.def.speechCooldown.RandomInRange);
			}
		}
	}

	private static bool PositiveOutcome(ThoughtDef outcome)
	{
		if (outcome != ThoughtDefOf.EncouragingSpeech)
		{
			return outcome == ThoughtDefOf.InspirationalSpeech;
		}
		return true;
	}
}
