using RimWorld.Planet;
using Verse;
using Verse.AI;

namespace RimWorld;

public class CompUseableDatacore : CompUsable
{
	public override LocalTargetInfo GetExtraTarget(Pawn pawn)
	{
		return GenClosest.ClosestThingReachable(pawn.Position, pawn.Map, ThingRequest.ForGroup(ThingRequestGroup.ResearchBench), PathEndMode.InteractionCell, TraverseParms.For(pawn, Danger.Some), 9999f, (Thing thing) => pawn.CanReserve(thing));
	}

	public override AcceptanceReport CanBeUsedBy(Pawn p, bool forced = false, bool ignoreReserveAndReachable = false)
	{
		AcceptanceReport result = base.CanBeUsedBy(p, forced, ignoreReserveAndReachable);
		if (!result.Accepted)
		{
			return result;
		}
		if (!GetExtraTarget(p).HasThing)
		{
			return "NoResearchBench".Translate();
		}
		return true;
	}

	public override void UsedBy(Pawn p)
	{
		base.UsedBy(p);
		Find.History.Notify_MechanoidDatacoreReadOrLost();
	}

	public override void PostDestroy(DestroyMode mode, Map previousMap)
	{
		base.PostDestroy(mode, previousMap);
		Find.History.Notify_MechanoidDatacoreReadOrLost();
	}

	public override void Notify_AbandonedAtTile(PlanetTile tile)
	{
		base.Notify_AbandonedAtTile(tile);
		Find.History.Notify_MechanoidDatacoreReadOrLost();
	}
}
