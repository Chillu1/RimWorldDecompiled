using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class RitualOutcomeEffectWorker_Duel : RitualOutcomeEffectWorker_FromQuality
{
	public const float RecreationGainGood = 0.25f;

	public const float RecreationGainBest = 0.5f;

	public const float MeleeXPGainParticipantsGood = 2500f;

	public const float MeleeXPGainSpectatorsGood = 1000f;

	public const float MeleeXPGainParticipantsBest = 5000f;

	public const float MeleeXPGainSpectatorsBest = 2000f;

	public RitualOutcomeEffectWorker_Duel()
	{
	}

	public RitualOutcomeEffectWorker_Duel(RitualOutcomeEffectDef def)
		: base(def)
	{
	}

	protected override bool OutcomePossible(RitualOutcomePossibility chance, LordJob_Ritual ritual)
	{
		if (!chance.BestPositiveOutcome(ritual))
		{
			return true;
		}
		return ((LordJob_Ritual_Duel)ritual).duelists.Any((Pawn d) => d.Dead);
	}

	protected override void ApplyExtraOutcome(Dictionary<Pawn, int> totalPresence, LordJob_Ritual jobRitual, RitualOutcomePossibility outcome, out string extraOutcomeDesc, ref LookTargets letterLookTargets)
	{
		extraOutcomeDesc = null;
		if (!outcome.Positive)
		{
			return;
		}
		float amount = (outcome.BestPositiveOutcome(jobRitual) ? 0.5f : 0.25f);
		float xp = (outcome.BestPositiveOutcome(jobRitual) ? 5000f : 2500f);
		float xp2 = (outcome.BestPositiveOutcome(jobRitual) ? 2000f : 1000f);
		LordJob_Ritual_Duel lordJob_Ritual_Duel = (LordJob_Ritual_Duel)jobRitual;
		foreach (Pawn key in totalPresence.Keys)
		{
			if (lordJob_Ritual_Duel.duelists.Contains(key))
			{
				key.skills.Learn(SkillDefOf.Melee, xp);
				continue;
			}
			key.skills.Learn(SkillDefOf.Melee, xp2);
			if (key.needs.joy != null)
			{
				key.needs.joy.GainJoy(amount, JoyKindDefOf.Social);
			}
		}
	}
}
