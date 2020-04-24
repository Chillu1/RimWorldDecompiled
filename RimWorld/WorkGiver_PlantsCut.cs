using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class WorkGiver_PlantsCut : WorkGiver_Scanner
	{
		public override PathEndMode PathEndMode => PathEndMode.Touch;

		public override Danger MaxPathDanger(Pawn pawn)
		{
			return Danger.Deadly;
		}

		public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
		{
			List<Designation> desList = pawn.Map.designationManager.allDesignations;
			for (int i = 0; i < desList.Count; i++)
			{
				Designation designation = desList[i];
				if (designation.def == DesignationDefOf.CutPlant || designation.def == DesignationDefOf.HarvestPlant)
				{
					yield return designation.target.Thing;
				}
			}
		}

		public override bool ShouldSkip(Pawn pawn, bool forced = false)
		{
			if (!pawn.Map.designationManager.AnySpawnedDesignationOfDef(DesignationDefOf.CutPlant))
			{
				return !pawn.Map.designationManager.AnySpawnedDesignationOfDef(DesignationDefOf.HarvestPlant);
			}
			return false;
		}

		public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
		{
			if (t.def.category != ThingCategory.Plant)
			{
				return null;
			}
			if (!pawn.CanReserve(t, 1, -1, null, forced))
			{
				return null;
			}
			if (t.IsForbidden(pawn))
			{
				return null;
			}
			if (t.IsBurning())
			{
				return null;
			}
			foreach (Designation item in pawn.Map.designationManager.AllDesignationsOn(t))
			{
				if (item.def == DesignationDefOf.HarvestPlant)
				{
					if (!((Plant)t).HarvestableNow)
					{
						return null;
					}
					return JobMaker.MakeJob(JobDefOf.HarvestDesignated, t);
				}
				if (item.def == DesignationDefOf.CutPlant)
				{
					return JobMaker.MakeJob(JobDefOf.CutPlantDesignated, t);
				}
			}
			return null;
		}
	}
}
