using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class RitualOutcomeEffectWorker_GiveMemoryAttended : RitualOutcomeEffectWorker
	{
		public RitualOutcomeEffectWorker_GiveMemoryAttended()
		{
		}

		public RitualOutcomeEffectWorker_GiveMemoryAttended(RitualOutcomeEffectDef def)
			: base(def)
		{
		}

		public override void Apply(float progress, Dictionary<Pawn, int> totalPresence, LordJob_Ritual jobRitual)
		{
			if (progress < 1f)
			{
				return;
			}
			foreach (KeyValuePair<Pawn, int> item in totalPresence)
			{
				item.Key.needs.mood.thoughts.memories.TryGainMemory(MakeMemory(item.Key, jobRitual));
			}
		}
	}
}
