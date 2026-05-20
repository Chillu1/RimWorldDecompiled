using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class CompAutoCutWindTurbine : CompAutoCut
{
	private ThingFilter fixedAutoCutFilter;

	public ThingFilter defaultAutoCutFilter;

	public override IEnumerable<IntVec3> GetAutoCutCells()
	{
		return WindTurbineUtility.CalculateWindCells(parent.Position, parent.Rotation, parent.def.size);
	}

	public override ThingFilter GetDefaultAutoCutFilter()
	{
		if (defaultAutoCutFilter == null)
		{
			defaultAutoCutFilter = new ThingFilter();
			foreach (ThingDef allowedThingDef in GetFixedAutoCutFilter().AllowedThingDefs)
			{
				if (allowedThingDef.blockWind)
				{
					defaultAutoCutFilter.SetAllow(allowedThingDef, allow: true);
				}
			}
		}
		return defaultAutoCutFilter;
	}

	public override ThingFilter GetFixedAutoCutFilter()
	{
		if (fixedAutoCutFilter == null)
		{
			fixedAutoCutFilter = new ThingFilter();
			foreach (ThingDef allWildPlant in parent.Map.wildPlantSpawner.AllWildPlants)
			{
				if (allWildPlant.plant.allowAutoCut)
				{
					fixedAutoCutFilter.SetAllow(allWildPlant, allow: true);
				}
			}
			fixedAutoCutFilter.SetAllow(ThingCategoryDefOf.Stumps, allow: true);
		}
		return fixedAutoCutFilter;
	}

	public override void CompTick()
	{
		base.CompTick();
		if (autoCut && parent.IsHashIntervalTick(2000))
		{
			DesignatePlantsToCut();
		}
	}
}
