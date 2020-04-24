using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class WorkGiver_BuildRoof : WorkGiver_Scanner
	{
		public override PathEndMode PathEndMode => PathEndMode.Touch;

		public override bool AllowUnreachable => true;

		public override IEnumerable<IntVec3> PotentialWorkCellsGlobal(Pawn pawn)
		{
			return pawn.Map.areaManager.BuildRoof.ActiveCells;
		}

		public override bool ShouldSkip(Pawn pawn, bool forced = false)
		{
			return pawn.Map.areaManager.BuildRoof.TrueCount == 0;
		}

		public override bool HasJobOnCell(Pawn pawn, IntVec3 c, bool forced = false)
		{
			if (!pawn.Map.areaManager.BuildRoof[c])
			{
				return false;
			}
			if (c.Roofed(pawn.Map))
			{
				return false;
			}
			if (c.IsForbidden(pawn))
			{
				return false;
			}
			if (!pawn.CanReserve(c, 1, -1, ReservationLayerDefOf.Ceiling, forced))
			{
				return false;
			}
			if (!pawn.CanReach(c, PathEndMode.Touch, pawn.NormalMaxDanger()) && BuildingToTouchToBeAbleToBuildRoof(c, pawn) == null)
			{
				return false;
			}
			if (!RoofCollapseUtility.WithinRangeOfRoofHolder(c, pawn.Map))
			{
				return false;
			}
			if (!RoofCollapseUtility.ConnectedToRoofHolder(c, pawn.Map, assumeRoofAtRoot: true))
			{
				return false;
			}
			Thing thing = RoofUtility.FirstBlockingThing(c, pawn.Map);
			if (thing != null)
			{
				return RoofUtility.CanHandleBlockingThing(thing, pawn, forced);
			}
			return true;
		}

		private Building BuildingToTouchToBeAbleToBuildRoof(IntVec3 c, Pawn pawn)
		{
			if (c.Standable(pawn.Map))
			{
				return null;
			}
			Building edifice = c.GetEdifice(pawn.Map);
			if (edifice == null)
			{
				return null;
			}
			if (!pawn.CanReach(edifice, PathEndMode.Touch, pawn.NormalMaxDanger()))
			{
				return null;
			}
			return edifice;
		}

		public override Job JobOnCell(Pawn pawn, IntVec3 c, bool forced = false)
		{
			LocalTargetInfo targetB = c;
			Thing thing = RoofUtility.FirstBlockingThing(c, pawn.Map);
			if (thing != null)
			{
				return RoofUtility.HandleBlockingThingJob(thing, pawn, forced);
			}
			if (!pawn.CanReach(c, PathEndMode.Touch, pawn.NormalMaxDanger()))
			{
				targetB = BuildingToTouchToBeAbleToBuildRoof(c, pawn);
			}
			return JobMaker.MakeJob(JobDefOf.BuildRoof, c, targetB);
		}
	}
}
