using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class WorkGiver_Train : WorkGiver_InteractAnimal
	{
		public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
		{
			return pawn.Map.mapPawns.SpawnedPawnsInFaction(pawn.Faction);
		}

		public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
		{
			Pawn pawn2 = t as Pawn;
			if (pawn2 == null || !pawn2.RaceProps.Animal)
			{
				return null;
			}
			if (pawn2.Faction != pawn.Faction)
			{
				return null;
			}
			if (TrainableUtility.TrainedTooRecently(pawn2))
			{
				JobFailReason.Is(WorkGiver_InteractAnimal.AnimalInteractedTooRecentlyTrans);
				return null;
			}
			if (pawn2.training == null)
			{
				return null;
			}
			if (pawn2.training.NextTrainableToTrain() == null)
			{
				return null;
			}
			if (!CanInteractWithAnimal(pawn, pawn2, forced))
			{
				return null;
			}
			if (pawn2.RaceProps.EatsFood && !HasFoodToInteractAnimal(pawn, pawn2))
			{
				Job job = TakeFoodForAnimalInteractJob(pawn, pawn2);
				if (job == null)
				{
					JobFailReason.Is(WorkGiver_InteractAnimal.NoUsableFoodTrans);
				}
				return job;
			}
			return JobMaker.MakeJob(JobDefOf.Train, t);
		}
	}
}
