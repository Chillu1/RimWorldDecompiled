using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class PlaceWorker_ShowInstrumentAoE : PlaceWorker
	{
		private static List<IntVec3> tmpCells = new List<IntVec3>();

		public override AcceptanceReport AllowsPlacing(BuildableDef checkingDef, IntVec3 loc, Rot4 rot, Map map, Thing thingToIgnore = null, Thing thing = null)
		{
			ThingDef thingDef = (ThingDef)checkingDef;
			tmpCells.Clear();
			int num = GenRadial.NumCellsInRadius(thingDef.building.instrumentRange);
			for (int i = 0; i < num; i++)
			{
				IntVec3 intVec = loc + GenRadial.RadialPattern[i];
				if (Building_MusicalInstrument.IsAffectedByInstrument(thingDef, loc, intVec, map))
				{
					tmpCells.Add(intVec);
				}
			}
			GenDraw.DrawFieldEdges(tmpCells);
			return true;
		}
	}
}
