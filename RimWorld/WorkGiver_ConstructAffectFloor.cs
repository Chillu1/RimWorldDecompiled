using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public abstract class WorkGiver_ConstructAffectFloor : WorkGiver_Scanner
	{
		protected abstract DesignationDef DesDef
		{
			get;
		}

		public override PathEndMode PathEndMode => PathEndMode.Touch;

		public override IEnumerable<IntVec3> PotentialWorkCellsGlobal(Pawn pawn)
		{
			foreach (Designation item in pawn.Map.designationManager.SpawnedDesignationsOfDef(DesDef))
			{
				yield return item.target.Cell;
			}
		}

		public override bool ShouldSkip(Pawn pawn, bool forced = false)
		{
			return !pawn.Map.designationManager.AnySpawnedDesignationOfDef(DesDef);
		}

		public override bool HasJobOnCell(Pawn pawn, IntVec3 c, bool forced = false)
		{
			if (c.IsForbidden(pawn) || pawn.Map.designationManager.DesignationAt(c, DesDef) == null || !pawn.CanReserve(c, 1, -1, ReservationLayerDefOf.Floor, forced))
			{
				return false;
			}
			return true;
		}
	}
}
