using Verse;
using Verse.AI;

namespace RimWorld;

public class JobGiver_LayEgg : ThinkNode_JobGiver
{
	private const float LayRadius = 5f;

	private const float MaxSearchRadius = 30f;

	private const int MinSearchRegions = 10;

	protected override Job TryGiveJob(Pawn pawn)
	{
		CompEggLayer compEggLayer = pawn.TryGetComp<CompEggLayer>();
		if (compEggLayer == null || !compEggLayer.CanLayNow)
		{
			return null;
		}
		ThingDef singleDef = compEggLayer.NextEggType();
		PathEndMode peMode = PathEndMode.OnCell;
		TraverseParms traverseParms = TraverseParms.For(pawn, Danger.Some);
		if (pawn.Faction == Faction.OfPlayer)
		{
			Thing bestEggBox = GetBestEggBox(pawn, peMode, traverseParms);
			if (bestEggBox != null)
			{
				return JobMaker.MakeJob(JobDefOf.LayEgg, bestEggBox);
			}
		}
		Thing thing = GenClosest.ClosestThingReachable(pawn.Position, pawn.Map, ThingRequest.ForDef(singleDef), peMode, traverseParms, 30f, (Thing x) => pawn.GetRoom() == null || x.GetRoom() == pawn.GetRoom());
		return JobMaker.MakeJob(JobDefOf.LayEgg, thing?.Position ?? RCellFinder.RandomWanderDestFor(pawn, pawn.Position, 5f, null, Danger.Some));
	}

	private Thing GetBestEggBox(Pawn pawn, PathEndMode peMode, TraverseParms tp)
	{
		ThingDef eggDef = pawn.TryGetComp<CompEggLayer>().NextEggType();
		return GenClosest.ClosestThing_Regionwise_ReachablePrioritized(pawn.Position, pawn.Map, ThingRequest.ForDef(ThingDefOf.EggBox), peMode, tp, 30f, IsUsableBox, GetScore, 10);
		static float GetScore(Thing thing)
		{
			CompEggContainer compEggContainer = thing.TryGetComp<CompEggContainer>();
			if (compEggContainer == null || compEggContainer.Full)
			{
				return 0f;
			}
			return ((float?)compEggContainer.ContainedThing?.stackCount * 5f) ?? 0.5f;
		}
		bool IsUsableBox(Thing thing)
		{
			if (!thing.Spawned)
			{
				return false;
			}
			if (thing.IsForbidden(pawn) || !pawn.CanReserve(thing) || !pawn.Position.InHorDistOf(thing.Position, 30f))
			{
				return false;
			}
			CompEggContainer compEggContainer = thing.TryGetComp<CompEggContainer>();
			if (compEggContainer == null || !compEggContainer.Accepts(eggDef))
			{
				return false;
			}
			return true;
		}
	}
}
