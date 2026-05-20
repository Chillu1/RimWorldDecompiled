using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class RitualOutcomeComp_DataThingPresence : RitualOutcomeComp_Data
	{
		public Dictionary<Thing, float> presentForTicks = new Dictionary<Thing, float>();

		private List<Thing> tmpPresentThing;

		private List<float> tmpPresentTicks;

		public override void Reset()
		{
			presentForTicks.Clear();
		}

		public override void ExposeData()
		{
			if (Scribe.mode == LoadSaveMode.Saving)
			{
				presentForTicks.RemoveAll((KeyValuePair<Thing, float> x) => x.Key.Destroyed);
			}
			Scribe_Collections.Look(ref presentForTicks, "presentForTicks", LookMode.Reference, LookMode.Value, ref tmpPresentThing, ref tmpPresentTicks);
		}
	}
}
