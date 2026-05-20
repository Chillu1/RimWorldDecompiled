using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld;

public class RitualOutcomeEffectWorker_Blinding : RitualOutcomeEffectWorker_FromQuality
{
	public const float PsylinkGainChance = 0.5f;

	public RitualOutcomeEffectWorker_Blinding()
	{
	}

	public RitualOutcomeEffectWorker_Blinding(RitualOutcomeEffectDef def)
		: base(def)
	{
	}

	protected override void ApplyExtraOutcome(Dictionary<Pawn, int> totalPresence, LordJob_Ritual jobRitual, RitualOutcomePossibility outcome, out string extraOutcomeDesc, ref LookTargets letterLookTargets)
	{
		extraOutcomeDesc = null;
		if (ModsConfig.RoyaltyActive && outcome.Positive && (outcome.BestPositiveOutcome(jobRitual) || Rand.Chance(0.5f)))
		{
			Pawn pawn = ((LordJob_Ritual_Mutilation)jobRitual).mutilatedPawns[0];
			extraOutcomeDesc = "RitualOutcomeExtraDesc_BlindingPsylink".Translate(pawn.Named("PAWN"));
			List<Ability> existingAbils = pawn.abilities.AllAbilitiesForReading.ToList();
			pawn.ChangePsylinkLevel(1);
			Ability ability = pawn.abilities.AllAbilitiesForReading.FirstOrDefault((Ability a) => !existingAbils.Contains(a));
			if (ability != null)
			{
				extraOutcomeDesc += " " + "RitualOutcomeExtraDesc_BlindingPsylinkAbility".Translate(ability.def.LabelCap, pawn.Named("PAWN"));
			}
		}
	}
}
