using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class RitualAttachableOutcomeEffectWorker_FarmAnimalsWanderIn : RitualAttachableOutcomeEffectWorker
{
	public const float PositiveOutcomeBodysize = 2f;

	public const float BestOutcomeBodysize = 3f;

	public override void Apply(Dictionary<Pawn, int> totalPresence, LordJob_Ritual jobRitual, RitualOutcomePossibility outcome, out string extraOutcomeDesc, ref LookTargets letterLookTargets)
	{
		extraOutcomeDesc = null;
		IncidentParms parms = new IncidentParms
		{
			target = jobRitual.Map,
			totalBodySize = (outcome.BestPositiveOutcome(jobRitual) ? 3f : 2f),
			customLetterText = "RitualAttachedOutcome_FarmAnimalsWanderIn_Desc".Translate(jobRitual.RitualLabel)
		};
		if (IncidentDefOf.FarmAnimalsWanderIn.Worker.TryExecute(parms))
		{
			extraOutcomeDesc = def.letterInfoText;
		}
	}
}
