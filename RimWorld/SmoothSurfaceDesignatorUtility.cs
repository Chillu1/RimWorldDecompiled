using Verse;

namespace RimWorld
{
	public static class SmoothSurfaceDesignatorUtility
	{
		public static bool CanSmoothFloorUnder(Building b)
		{
			if (b.def.Fillage == FillCategory.Full)
			{
				return b.def.passability != Traversability.Impassable;
			}
			return true;
		}

		public static void Notify_BuildingSpawned(Building b)
		{
			if (CanSmoothFloorUnder(b))
			{
				return;
			}
			foreach (IntVec3 item in b.OccupiedRect())
			{
				Designation designation = b.Map.designationManager.DesignationAt(item, DesignationDefOf.SmoothFloor);
				if (designation != null)
				{
					b.Map.designationManager.RemoveDesignation(designation);
				}
			}
		}

		public static void Notify_BuildingDespawned(Building b, Map map)
		{
			foreach (IntVec3 item in b.OccupiedRect())
			{
				Designation designation = map.designationManager.DesignationAt(item, DesignationDefOf.SmoothWall);
				if (designation != null)
				{
					map.designationManager.RemoveDesignation(designation);
				}
			}
		}
	}
}
