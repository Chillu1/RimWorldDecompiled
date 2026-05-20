using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class RitualAttachableOutcomeEffectWorker_PsyfocusRecharge : RitualAttachableOutcomeEffectWorker
{
	public override void Apply(Dictionary<Pawn, int> totalPresence, LordJob_Ritual jobRitual, RitualOutcomePossibility outcome, out string extraOutcomeDesc, ref LookTargets letterLookTargets)
	{
		extraOutcomeDesc = def.letterInfoText;
		foreach (Pawn key in totalPresence.Keys)
		{
			key.psychicEntropy?.RechargePsyfocus();
		}
	}
}
