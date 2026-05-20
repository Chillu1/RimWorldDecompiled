using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld;

public class RitualOutcomeEffectWorker_Consumable : RitualOutcomeEffectWorker_FromQuality
{
	public const float InspirationGainChance = 0.5f;

	public const float InspirationGainChanceBestOutcome = 1f;

	public RitualOutcomeEffectWorker_Consumable()
	{
	}

	public RitualOutcomeEffectWorker_Consumable(RitualOutcomeEffectDef def)
		: base(def)
	{
	}

	protected override void ApplyExtraOutcome(Dictionary<Pawn, int> totalPresence, LordJob_Ritual jobRitual, RitualOutcomePossibility outcome, out string extraOutcomeDesc, ref LookTargets letterLookTargets)
	{
		extraOutcomeDesc = null;
		if (!outcome.Positive || !Rand.Chance(outcome.BestPositiveOutcome(jobRitual) ? 1f : 0.5f))
		{
			return;
		}
		Pawn inspiredPawn = totalPresence.Keys.Where((Pawn p) => !p.Inspired && DefDatabase<InspirationDef>.AllDefsListForReading.Any((InspirationDef i) => i.Worker.InspirationCanOccur(p))).RandomElementWithFallback();
		if (inspiredPawn != null)
		{
			InspirationDef inspirationDef = DefDatabase<InspirationDef>.AllDefsListForReading.Where((InspirationDef i) => i.Worker.InspirationCanOccur(inspiredPawn)).RandomElementWithFallback();
			if (inspirationDef == null)
			{
				Log.Error("Could not find inspiration for pawn " + inspiredPawn.Name.ToStringFull);
			}
			else if (!inspiredPawn.mindState.inspirationHandler.TryStartInspiration(inspirationDef, null, sendLetter: false))
			{
				Log.Error("Inspiring " + inspiredPawn.Name.ToStringFull + " failed, but the inspiration worker claimed it can occur!");
			}
			else
			{
				extraOutcomeDesc = "RitualOutcomeExtraDesc_ConsumableInspiration".Translate(inspiredPawn.Named("PAWN"), inspirationDef.LabelCap.Named("INSPIRATION"), jobRitual.Ritual.Label.Named("RITUAL")).CapitalizeFirst() + " " + inspiredPawn.Inspiration.LetterText;
			}
		}
	}
}
