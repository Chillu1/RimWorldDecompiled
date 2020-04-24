using RimWorld.Planet;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public abstract class WorkGiver_Warden : WorkGiver_Scanner
	{
		public override ThingRequest PotentialWorkThingRequest => ThingRequest.ForGroup(ThingRequestGroup.Pawn);

		public override PathEndMode PathEndMode => PathEndMode.OnCell;

		public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
		{
			return pawn.Map.mapPawns.PrisonersOfColonySpawned;
		}

		public override bool ShouldSkip(Pawn pawn, bool forced = false)
		{
			return pawn.Map.mapPawns.PrisonersOfColonySpawnedCount == 0;
		}

		protected bool ShouldTakeCareOfPrisoner(Pawn warden, Thing prisoner)
		{
			Pawn pawn = prisoner as Pawn;
			if (pawn == null || !pawn.IsPrisonerOfColony || !pawn.guest.PrisonerIsSecure || !pawn.Spawned || pawn.InAggroMentalState || prisoner.IsForbidden(warden) || pawn.IsFormingCaravan() || !warden.CanReserveAndReach(pawn, PathEndMode.OnCell, warden.NormalMaxDanger()))
			{
				return false;
			}
			return true;
		}
	}
}
