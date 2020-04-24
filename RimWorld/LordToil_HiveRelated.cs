using System.Collections.Generic;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace RimWorld
{
	public abstract class LordToil_HiveRelated : LordToil
	{
		private LordToil_HiveRelatedData Data => (LordToil_HiveRelatedData)data;

		public LordToil_HiveRelated()
		{
			data = new LordToil_HiveRelatedData();
		}

		protected void FilterOutUnspawnedHives()
		{
			Data.assignedHives.RemoveAll((KeyValuePair<Pawn, Hive> x) => x.Value == null || !x.Value.Spawned);
		}

		protected Hive GetHiveFor(Pawn pawn)
		{
			if (Data.assignedHives.TryGetValue(pawn, out Hive value))
			{
				return value;
			}
			value = FindClosestHive(pawn);
			if (value != null)
			{
				Data.assignedHives.Add(pawn, value);
			}
			return value;
		}

		private Hive FindClosestHive(Pawn pawn)
		{
			return (Hive)GenClosest.ClosestThingReachable(pawn.Position, pawn.Map, ThingRequest.ForDef(ThingDefOf.Hive), PathEndMode.Touch, TraverseParms.For(pawn), 30f, (Thing x) => x.Faction == pawn.Faction, null, 0, 30);
		}
	}
}
