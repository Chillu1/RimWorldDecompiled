using Verse;
using Verse.AI;

namespace RimWorld;

internal class WorkGiver_FightFires : WorkGiver_Scanner
{
	public const int NearbyPawnRadius = 15;

	private const int MaxReservationCheckDistance = 15;

	private const float HandledDistance = 5f;

	public override ThingRequest PotentialWorkThingRequest => ThingRequest.ForDef(ThingDefOf.Fire);

	public override PathEndMode PathEndMode => PathEndMode.Touch;

	public override Danger MaxPathDanger(Pawn pawn)
	{
		return Danger.Deadly;
	}

	public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
	{
		if (!(t is Fire fire))
		{
			return false;
		}
		if (fire.parent is Pawn pawn2)
		{
			if (pawn2 == pawn)
			{
				return false;
			}
			if ((pawn2.Faction == null || pawn2.Faction != pawn.Faction) && (pawn2.HostFaction == null || (pawn2.HostFaction != pawn.Faction && pawn2.HostFaction != pawn.HostFaction)))
			{
				return false;
			}
			if (!pawn.Map.areaManager.Home[fire.Position] && IntVec3Utility.ManhattanDistanceFlat(pawn.Position, pawn2.Position) > 15)
			{
				return false;
			}
			if (!pawn.CanReach(pawn2, PathEndMode.Touch, Danger.Deadly))
			{
				return false;
			}
		}
		else
		{
			if (pawn.WorkTagIsDisabled(WorkTags.Firefighting))
			{
				JobFailReason.Is("IncapableOfFirefighting".Translate());
				return false;
			}
			if (!pawn.Map.areaManager.Home[fire.Position])
			{
				JobFailReason.Is(WorkGiver_FixBrokenDownBuilding.NotInHomeAreaTrans);
				return false;
			}
		}
		if ((pawn.Position - fire.Position).LengthHorizontalSquared > 225 && !pawn.CanReserve(fire, 1, -1, null, forced))
		{
			return false;
		}
		if (!forced && FireIsBeingHandled(fire, pawn))
		{
			return false;
		}
		return true;
	}

	public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
	{
		return JobMaker.MakeJob(JobDefOf.BeatFire, t);
	}

	public static bool FireIsBeingHandled(Fire f, Pawn potentialHandler)
	{
		if (!f.Spawned)
		{
			return false;
		}
		return f.Map.reservationManager.FirstRespectedReserver(f, potentialHandler)?.Position.InHorDistOf(f.Position, 5f) ?? false;
	}
}
