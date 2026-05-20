using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class RitualOutcomeEffectWorker_GiveMemoryBelievers : RitualOutcomeEffectWorker
{
	public RitualOutcomeEffectWorker_GiveMemoryBelievers()
	{
	}

	public RitualOutcomeEffectWorker_GiveMemoryBelievers(RitualOutcomeEffectDef def)
		: base(def)
	{
	}

	public override void Apply(float progress, Dictionary<Pawn, int> totalPresence, LordJob_Ritual jobRitual)
	{
		foreach (Pawn allMapsCaravansAndTravellingTransporters_Alive_Colonist in PawnsFinder.AllMapsCaravansAndTravellingTransporters_Alive_Colonists)
		{
			if (allMapsCaravansAndTravellingTransporters_Alive_Colonist.Ideo == jobRitual.Ritual.ideo)
			{
				allMapsCaravansAndTravellingTransporters_Alive_Colonist.needs.mood.thoughts.memories.TryGainMemory(MakeMemory(allMapsCaravansAndTravellingTransporters_Alive_Colonist, jobRitual));
			}
		}
	}
}
