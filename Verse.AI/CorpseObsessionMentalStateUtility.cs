using RimWorld;

namespace Verse.AI
{
	public static class CorpseObsessionMentalStateUtility
	{
		public static Corpse GetClosestCorpseToDigUp(Pawn pawn)
		{
			if (!pawn.Spawned)
			{
				return null;
			}
			return ((Building_Grave)GenClosest.ClosestThingReachable(pawn.Position, pawn.Map, ThingRequest.ForGroup(ThingRequestGroup.Grave), PathEndMode.InteractionCell, TraverseParms.For(pawn), 9999f, delegate(Thing x)
			{
				Building_Grave building_Grave = (Building_Grave)x;
				return building_Grave.HasCorpse && IsCorpseValid(building_Grave.Corpse, pawn, ignoreReachability: true);
			}))?.Corpse;
		}

		public static bool IsCorpseValid(Corpse corpse, Pawn pawn, bool ignoreReachability = false)
		{
			if (corpse == null || corpse.Destroyed || !corpse.InnerPawn.RaceProps.Humanlike)
			{
				return false;
			}
			if (pawn.carryTracker.CarriedThing == corpse)
			{
				return true;
			}
			if (corpse.Spawned)
			{
				if (!pawn.CanReserve(corpse))
				{
					return false;
				}
				if (!ignoreReachability)
				{
					return pawn.CanReach(corpse, PathEndMode.Touch, Danger.Deadly);
				}
				return true;
			}
			Building_Grave building_Grave = corpse.ParentHolder as Building_Grave;
			if (building_Grave != null && building_Grave.Spawned)
			{
				if (!pawn.CanReserve(building_Grave))
				{
					return false;
				}
				if (!ignoreReachability)
				{
					return pawn.CanReach(building_Grave, PathEndMode.InteractionCell, Danger.Deadly);
				}
				return true;
			}
			return false;
		}
	}
}
