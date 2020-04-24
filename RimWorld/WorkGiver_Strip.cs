using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class WorkGiver_Strip : WorkGiver_Scanner
	{
		public override PathEndMode PathEndMode => PathEndMode.ClosestTouch;

		public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
		{
			foreach (Designation item in pawn.Map.designationManager.SpawnedDesignationsOfDef(DesignationDefOf.Strip))
			{
				if (!item.target.HasThing)
				{
					Log.ErrorOnce("Strip designation has no target.", 63126);
				}
				else
				{
					yield return item.target.Thing;
				}
			}
		}

		public override Danger MaxPathDanger(Pawn pawn)
		{
			return Danger.Deadly;
		}

		public override bool ShouldSkip(Pawn pawn, bool forced = false)
		{
			return !pawn.Map.designationManager.AnySpawnedDesignationOfDef(DesignationDefOf.Strip);
		}

		public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
		{
			if (t.Map.designationManager.DesignationOn(t, DesignationDefOf.Strip) == null)
			{
				return false;
			}
			if (!pawn.CanReserve(t, 1, -1, null, forced))
			{
				return false;
			}
			if (!StrippableUtility.CanBeStrippedByColony(t))
			{
				return false;
			}
			return true;
		}

		public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
		{
			return JobMaker.MakeJob(JobDefOf.Strip, t);
		}
	}
}
