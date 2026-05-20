using Verse;
using Verse.AI;

namespace RimWorld;

public class BabyPlayGiver_PlayToys : BabyPlayGiver
{
	private const float MaxToyBoxDistance = 15.9f;

	public override bool CanDo(Pawn pawn, Pawn baby)
	{
		Thing thing = FindNearbyUseableToyBox(pawn, baby);
		if (thing == null)
		{
			return false;
		}
		if (!pawn.IsCarryingPawn(baby) && !pawn.CanReserveAndReach(baby, PathEndMode.Touch, Danger.Some))
		{
			return false;
		}
		if (!pawn.CanReserveAndReach(thing, PathEndMode.Touch, Danger.Some))
		{
			return false;
		}
		return true;
	}

	public override Job TryGiveJob(Pawn pawn, Pawn baby)
	{
		Thing thing = FindNearbyUseableToyBox(pawn, baby);
		if (thing != null)
		{
			Job job = JobMaker.MakeJob(def.jobDef, baby, thing);
			job.count = 1;
			return job;
		}
		return null;
	}

	private bool IsValidToybox(Thing toybox, Pawn hauler, Pawn baby)
	{
		if (toybox.def != ThingDefOf.ToyBox)
		{
			return false;
		}
		if (toybox.IsForbidden(hauler) || toybox.IsForbidden(baby))
		{
			return false;
		}
		if (toybox.IsBurning())
		{
			return false;
		}
		if (!hauler.CanReserveAndReach(toybox, PathEndMode.Touch, Danger.None))
		{
			return false;
		}
		if (hauler.Position.DistanceTo(toybox.Position) > 15.9f)
		{
			return false;
		}
		return true;
	}

	private Thing FindNearbyUseableToyBox(Pawn pawn, Pawn baby)
	{
		Room room = baby.GetRoom();
		if (room != null)
		{
			foreach (Thing item in room.ContainedThings(ThingDefOf.ToyBox))
			{
				if (IsValidToybox(item, pawn, baby))
				{
					return item;
				}
			}
		}
		return GenClosest.ClosestThingReachable(baby.PositionHeld, baby.MapHeld, ThingRequest.ForDef(ThingDefOf.ToyBox), PathEndMode.OnCell, TraverseParms.For(pawn), 15.9f, (Thing t) => IsValidToybox(t, pawn, baby));
	}
}
