using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class WorkGiver_UnloadCarriers : WorkGiver_Scanner
	{
		public override ThingRequest PotentialWorkThingRequest => ThingRequest.ForGroup(ThingRequestGroup.Pawn);

		public override PathEndMode PathEndMode => PathEndMode.Touch;

		public override bool ShouldSkip(Pawn pawn, bool forced = false)
		{
			List<Pawn> allPawnsSpawned = pawn.Map.mapPawns.AllPawnsSpawned;
			for (int i = 0; i < allPawnsSpawned.Count; i++)
			{
				if (allPawnsSpawned[i].inventory.UnloadEverything)
				{
					return false;
				}
			}
			return true;
		}

		public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
		{
			return pawn.Map.mapPawns.SpawnedPawnsWhoShouldHaveInventoryUnloaded;
		}

		public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
		{
			return UnloadCarriersJobGiverUtility.HasJobOnThing(pawn, t, forced);
		}

		public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
		{
			return JobMaker.MakeJob(JobDefOf.UnloadInventory, t);
		}
	}
}
