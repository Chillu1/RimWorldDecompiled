using System;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld;

public abstract class JoyGiver_InteractBuilding : JoyGiver
{
	private static List<Thing> tmpCandidates = new List<Thing>();

	protected virtual bool CanDoDuringGathering => false;

	public override Job TryGiveJob(Pawn pawn)
	{
		Thing thing = FindBestGame(pawn, inBed: false, IntVec3.Invalid);
		if (thing != null)
		{
			return TryGivePlayJob(pawn, thing);
		}
		return null;
	}

	public override Job TryGiveJobWhileInBed(Pawn pawn)
	{
		Thing thing = FindBestGame(pawn, inBed: true, IntVec3.Invalid);
		if (thing != null)
		{
			return TryGivePlayJobWhileInBed(pawn, thing);
		}
		return null;
	}

	public override Job TryGiveJobInGatheringArea(Pawn pawn, IntVec3 gatheringSpot, float maxRadius = -1f)
	{
		if (!CanDoDuringGathering)
		{
			return null;
		}
		Thing thing = FindBestGame(pawn, inBed: false, gatheringSpot);
		if (thing != null)
		{
			return TryGivePlayJob(pawn, thing);
		}
		return null;
	}

	private Thing FindBestGame(Pawn pawn, bool inBed, IntVec3 gatheringSpot)
	{
		tmpCandidates.Clear();
		GetSearchSet(pawn, tmpCandidates);
		if (tmpCandidates.Count == 0)
		{
			return null;
		}
		Predicate<Thing> predicate = (Thing t) => CanInteractWith(pawn, t, inBed);
		if (gatheringSpot.IsValid)
		{
			Predicate<Thing> oldValidator = predicate;
			predicate = (Thing x) => GatheringsUtility.InGatheringArea(x.Position, gatheringSpot, pawn.Map) && oldValidator(x);
		}
		Thing result = GenClosest.ClosestThing_Global_Reachable(pawn.Position, pawn.Map, tmpCandidates, PathEndMode.OnCell, TraverseParms.For(pawn), 9999f, predicate);
		tmpCandidates.Clear();
		return result;
	}

	protected virtual bool CanInteractWith(Pawn pawn, Thing t, bool inBed)
	{
		if (!pawn.CanReserve(t, def.jobDef.joyMaxParticipants))
		{
			return false;
		}
		if (t.IsForbidden(pawn))
		{
			return false;
		}
		if (t.Fogged())
		{
			return false;
		}
		if (!t.IsSociallyProper(pawn))
		{
			return false;
		}
		if (!t.IsPoliticallyProper(pawn))
		{
			return false;
		}
		if (t.VacuumConcernTo(pawn))
		{
			return false;
		}
		CompPowerTrader compPowerTrader = t.TryGetComp<CompPowerTrader>();
		if (compPowerTrader != null && !compPowerTrader.PowerOn)
		{
			return false;
		}
		if (def.unroofedOnly && t.Position.Roofed(t.Map))
		{
			return false;
		}
		return true;
	}

	protected abstract Job TryGivePlayJob(Pawn pawn, Thing bestGame);

	protected virtual Job TryGivePlayJobWhileInBed(Pawn pawn, Thing bestGame)
	{
		Building_Bed building_Bed = pawn.CurrentBed();
		return JobMaker.MakeJob(def.jobDef, bestGame, pawn.Position, building_Bed);
	}
}
