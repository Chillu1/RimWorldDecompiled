using System;
using System.Collections.Generic;
using RimWorld.Planet;
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

		[Obsolete("Will be removed in the future")]
		protected bool ShouldTakeCareOfPrisoner(Pawn warden, Thing prisoner)
		{
			return ShouldTakeCareOfPrisoner_NewTemp(warden, prisoner);
		}

		protected bool ShouldTakeCareOfPrisoner_NewTemp(Pawn warden, Thing prisoner, bool forced = false)
		{
			Pawn pawn = prisoner as Pawn;
			if (pawn == null || !pawn.IsPrisonerOfColony || !pawn.guest.PrisonerIsSecure || !pawn.Spawned || pawn.InAggroMentalState || prisoner.IsForbidden(warden) || pawn.IsFormingCaravan() || !warden.CanReserveAndReach(pawn, PathEndMode.OnCell, warden.NormalMaxDanger(), 1, -1, null, forced))
			{
				return false;
			}
			return true;
		}
	}
}
