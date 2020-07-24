using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public abstract class WorkGiver_Grower : WorkGiver_Scanner
	{
		protected static ThingDef wantedPlantDef;

		public override bool AllowUnreachable => true;

		protected virtual bool ExtraRequirements(IPlantToGrowSettable settable, Pawn pawn)
		{
			return true;
		}

		public override IEnumerable<IntVec3> PotentialWorkCellsGlobal(Pawn pawn)
		{
			Danger maxDanger = pawn.NormalMaxDanger();
			List<Building> bList = pawn.Map.listerBuildings.allBuildingsColonist;
			for (int j = 0; j < bList.Count; j++)
			{
				Building_PlantGrower building_PlantGrower = bList[j] as Building_PlantGrower;
				if (building_PlantGrower == null || !ExtraRequirements(building_PlantGrower, pawn) || building_PlantGrower.IsForbidden(pawn) || !pawn.CanReach(building_PlantGrower, PathEndMode.OnCell, maxDanger) || building_PlantGrower.IsBurning())
				{
					continue;
				}
				foreach (IntVec3 item in building_PlantGrower.OccupiedRect())
				{
					yield return item;
				}
				wantedPlantDef = null;
			}
			wantedPlantDef = null;
			List<Zone> zonesList = pawn.Map.zoneManager.AllZones;
			for (int j = 0; j < zonesList.Count; j++)
			{
				Zone_Growing growZone = zonesList[j] as Zone_Growing;
				if (growZone == null)
				{
					continue;
				}
				if (growZone.cells.Count == 0)
				{
					Log.ErrorOnce("Grow zone has 0 cells: " + growZone, -563487);
				}
				else if (ExtraRequirements(growZone, pawn) && !growZone.ContainsStaticFire && pawn.CanReach(growZone.Cells[0], PathEndMode.OnCell, maxDanger))
				{
					for (int k = 0; k < growZone.cells.Count; k++)
					{
						yield return growZone.cells[k];
					}
					wantedPlantDef = null;
				}
			}
			wantedPlantDef = null;
		}

		public static ThingDef CalculateWantedPlantDef(IntVec3 c, Map map)
		{
			return c.GetPlantToGrowSettable(map)?.GetPlantDefToGrow();
		}
	}
}
