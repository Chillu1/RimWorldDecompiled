using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class PawnObserver
	{
		private Pawn pawn;

		private int intervalsUntilObserve;

		private const int IntervalsBetweenObservations = 4;

		private const float SampleNumCells = 100f;

		public PawnObserver(Pawn pawn)
		{
			this.pawn = pawn;
			intervalsUntilObserve = Rand.Range(0, 4);
		}

		public void ObserverInterval()
		{
			if (pawn.Spawned)
			{
				intervalsUntilObserve--;
				if (intervalsUntilObserve <= 0)
				{
					ObserveSurroundingThings();
					intervalsUntilObserve = 4 + Rand.RangeInclusive(-1, 1);
				}
			}
		}

		private void ObserveSurroundingThings()
		{
			if (!pawn.health.capacities.CapableOf(PawnCapacityDefOf.Sight) || pawn.needs.mood == null)
			{
				return;
			}
			Map map = pawn.Map;
			for (int i = 0; (float)i < 100f; i++)
			{
				IntVec3 intVec = pawn.Position + GenRadial.RadialPattern[i];
				if (!intVec.InBounds(map) || !GenSight.LineOfSight(intVec, pawn.Position, map, skipFirstCell: true))
				{
					continue;
				}
				List<Thing> thingList = intVec.GetThingList(map);
				for (int j = 0; j < thingList.Count; j++)
				{
					IThoughtGiver thoughtGiver = thingList[j] as IThoughtGiver;
					if (thoughtGiver != null)
					{
						Thought_Memory thought_Memory = thoughtGiver.GiveObservedThought();
						if (thought_Memory != null)
						{
							pawn.needs.mood.thoughts.memories.TryGainMemory(thought_Memory);
						}
					}
				}
			}
		}
	}
}
