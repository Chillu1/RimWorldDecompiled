using System.Collections.Generic;
using Verse;
using Verse.AI.Group;

namespace RimWorld
{
	public class LordToil_HiveRelatedData : LordToilData
	{
		public Dictionary<Pawn, Hive> assignedHives = new Dictionary<Pawn, Hive>();

		public override void ExposeData()
		{
			if (Scribe.mode == LoadSaveMode.Saving)
			{
				assignedHives.RemoveAll((KeyValuePair<Pawn, Hive> x) => x.Key.Destroyed);
			}
			Scribe_Collections.Look(ref assignedHives, "assignedHives", LookMode.Reference, LookMode.Reference);
			if (Scribe.mode == LoadSaveMode.PostLoadInit)
			{
				assignedHives.RemoveAll((KeyValuePair<Pawn, Hive> x) => x.Value == null);
			}
		}
	}
}
