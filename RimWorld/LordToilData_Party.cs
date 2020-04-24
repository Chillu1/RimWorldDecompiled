using System.Collections.Generic;
using Verse;
using Verse.AI.Group;

namespace RimWorld
{
	public class LordToilData_Party : LordToilData
	{
		public Dictionary<Pawn, int> presentForTicks = new Dictionary<Pawn, int>();

		public override void ExposeData()
		{
			if (Scribe.mode == LoadSaveMode.Saving)
			{
				presentForTicks.RemoveAll((KeyValuePair<Pawn, int> x) => x.Key.Destroyed);
			}
			Scribe_Collections.Look(ref presentForTicks, "presentForTicks", LookMode.Reference);
		}
	}
}
