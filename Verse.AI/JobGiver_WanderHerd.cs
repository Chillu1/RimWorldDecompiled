using System;
using System.Collections.Generic;
using RimWorld;

namespace Verse.AI
{
	public class JobGiver_WanderHerd : JobGiver_Wander
	{
		private const int MinDistToHumanlike = 15;

		public JobGiver_WanderHerd()
		{
			wanderRadius = 5f;
			ticksBetweenWandersRange = new IntRange(125, 200);
		}

		protected override IntVec3 GetWanderRoot(Pawn pawn)
		{
			Predicate<Thing> validator = delegate(Thing t)
			{
				if (((Pawn)t).RaceProps != pawn.RaceProps || t == pawn)
				{
					return false;
				}
				if (t.Faction != pawn.Faction)
				{
					return false;
				}
				if (t.Position.IsForbidden(pawn))
				{
					return false;
				}
				if (!pawn.CanReach(t, PathEndMode.OnCell, Danger.Deadly))
				{
					return false;
				}
				if (Rand.Value < 0.5f)
				{
					return false;
				}
				List<Pawn> allPawnsSpawned = pawn.Map.mapPawns.AllPawnsSpawned;
				for (int i = 0; i < allPawnsSpawned.Count; i++)
				{
					Pawn pawn2 = allPawnsSpawned[i];
					if (pawn2.RaceProps.Humanlike && (pawn2.Position - t.Position).LengthHorizontalSquared < 225)
					{
						return false;
					}
				}
				return true;
			};
			return GenClosest.ClosestThingReachable(pawn.Position, pawn.Map, ThingRequest.ForGroup(ThingRequestGroup.Pawn), PathEndMode.OnCell, TraverseParms.For(pawn), 35f, validator, null, 13)?.Position ?? pawn.Position;
		}
	}
}
