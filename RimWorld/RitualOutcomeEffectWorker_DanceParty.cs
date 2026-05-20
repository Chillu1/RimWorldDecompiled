using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class RitualOutcomeEffectWorker_DanceParty : RitualOutcomeEffectWorker_FromQuality
{
	public const float WorkFocusChance = 0.1f;

	public RitualOutcomeEffectWorker_DanceParty()
	{
	}

	public RitualOutcomeEffectWorker_DanceParty(RitualOutcomeEffectDef def)
		: base(def)
	{
	}

	protected override void ApplyExtraOutcome(Dictionary<Pawn, int> totalPresence, LordJob_Ritual jobRitual, RitualOutcomePossibility outcome, out string extraOutcomeDesc, ref LookTargets letterLookTargets)
	{
		extraOutcomeDesc = null;
		if (!outcome.Positive || !Rand.Chance(0.1f))
		{
			return;
		}
		float statFactorFromList = HediffDefOf.WorkFocus.stages[0].statOffsets.GetStatFactorFromList(StatDefOf.WorkSpeedGlobal);
		extraOutcomeDesc = "RitualOutcomeExtraDesc_DancePartyWorkFocus".Translate((((statFactorFromList > 0f) ? "+" : "") + statFactorFromList.ToStringPercent()).Named("PERCENTAGE"));
		foreach (Pawn key in totalPresence.Keys)
		{
			key.health.AddHediff(HediffDefOf.WorkFocus);
		}
	}
}
