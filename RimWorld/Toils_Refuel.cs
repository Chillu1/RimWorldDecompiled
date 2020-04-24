using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class Toils_Refuel
	{
		public static Toil FinalizeRefueling(TargetIndex refuelableInd, TargetIndex fuelInd)
		{
			Toil toil = new Toil();
			toil.initAction = delegate
			{
				Job curJob = toil.actor.CurJob;
				Thing thing = curJob.GetTarget(refuelableInd).Thing;
				if (toil.actor.CurJob.placedThings.NullOrEmpty())
				{
					thing.TryGetComp<CompRefuelable>().Refuel(new List<Thing>
					{
						curJob.GetTarget(fuelInd).Thing
					});
				}
				else
				{
					thing.TryGetComp<CompRefuelable>().Refuel(toil.actor.CurJob.placedThings.Select((ThingCountClass p) => p.thing).ToList());
				}
			};
			toil.defaultCompleteMode = ToilCompleteMode.Instant;
			return toil;
		}
	}
}
