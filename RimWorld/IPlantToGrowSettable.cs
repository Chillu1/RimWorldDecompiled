using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public interface IPlantToGrowSettable
	{
		Map Map
		{
			get;
		}

		IEnumerable<IntVec3> Cells
		{
			get;
		}

		ThingDef GetPlantDefToGrow();

		void SetPlantDefToGrow(ThingDef plantDef);

		bool CanAcceptSowNow();
	}
}
