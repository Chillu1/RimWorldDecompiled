using System.Collections.Generic;
using Verse;
using Verse.AI.Group;

namespace RimWorld
{
	public class LordToilData_Gathering : LordToilData
	{
		public Dictionary<Pawn, int> presentForTicks = new Dictionary<Pawn, int>();

		private List<Pawn> tmpPresentPawns;

		private List<int> tmpPresentPawnsTicks;

		public override void ExposeData()
		{
			if (Scribe.mode == LoadSaveMode.Saving)
			{
				presentForTicks.RemoveAll((KeyValuePair<Pawn, int> x) => x.Key.Destroyed);
			}
			Scribe_Collections.Look(ref presentForTicks, "presentForTicks", LookMode.Reference, LookMode.Value, ref tmpPresentPawns, ref tmpPresentPawnsTicks);
		}
	}
}
