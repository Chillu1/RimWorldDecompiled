using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class RitualAttachableOutcomeEffectWorker_InsectJellyTunnels : RitualAttachableOutcomeEffectWorker
{
	public const int PositiveOutcomePoints = 210;

	public const int BestOutcomePoints = 320;

	public override void Apply(Dictionary<Pawn, int> totalPresence, LordJob_Ritual jobRitual, RitualOutcomePossibility outcome, out string extraOutcomeDesc, ref LookTargets letterLookTargets)
	{
		extraOutcomeDesc = null;
		IncidentParms parms = new IncidentParms
		{
			target = jobRitual.Map,
			points = (outcome.BestPositiveOutcome(jobRitual) ? 320 : 210)
		};
		if (IncidentDefOf.Infestation_Jelly.Worker.TryExecute(parms))
		{
			extraOutcomeDesc = def.letterInfoText;
		}
	}
}
