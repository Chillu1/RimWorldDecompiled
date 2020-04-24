using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class WorkGiver_Tame : WorkGiver_InteractAnimal
	{
		public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
		{
			foreach (Designation item in pawn.Map.designationManager.SpawnedDesignationsOfDef(DesignationDefOf.Tame))
			{
				yield return item.target.Thing;
			}
		}

		public override bool ShouldSkip(Pawn pawn, bool forced = false)
		{
			return !pawn.Map.designationManager.AnySpawnedDesignationOfDef(DesignationDefOf.Tame);
		}

		public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
		{
			Pawn pawn2 = t as Pawn;
			if (pawn2 == null || !TameUtility.CanTame(pawn2))
			{
				return null;
			}
			if (pawn.Map.designationManager.DesignationOn(t, DesignationDefOf.Tame) == null)
			{
				return null;
			}
			if (TameUtility.TriedToTameTooRecently(pawn2))
			{
				JobFailReason.Is(WorkGiver_InteractAnimal.AnimalInteractedTooRecentlyTrans);
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
			return JobMaker.MakeJob(JobDefOf.Tame, t);
		}
	}
}
