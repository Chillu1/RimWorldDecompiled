using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JoyGiver_InteractBuildingSitAdjacent : JoyGiver_InteractBuilding
	{
		private static List<IntVec3> tmpCells = new List<IntVec3>();

		protected override Job TryGivePlayJob(Pawn pawn, Thing t)
		{
			tmpCells.Clear();
			tmpCells.AddRange(GenAdjFast.AdjacentCellsCardinal(t));
			tmpCells.Shuffle();
			for (int i = 0; i < 2; i++)
			{
				for (int j = 0; j < tmpCells.Count; j++)
				{
					IntVec3 intVec = tmpCells[j];
					if (intVec.IsForbidden(pawn) || !pawn.CanReserveSittableOrSpot(intVec))
					{
						continue;
					}
					if (i == 0)
					{
						Building edifice = intVec.GetEdifice(pawn.Map);
						if (edifice == null || !edifice.def.building.isSittable)
						{
							continue;
						}
					}
					return JobMaker.MakeJob(def.jobDef, t, intVec);
				}
				if (def.requireChair)
				{
					break;
				}
			}
			return null;
		}
	}
}
