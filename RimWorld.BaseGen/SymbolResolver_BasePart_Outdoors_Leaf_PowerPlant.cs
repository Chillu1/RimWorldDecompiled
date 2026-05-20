using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld.BaseGen;

public class SymbolResolver_BasePart_Outdoors_Leaf_PowerPlant : SymbolResolver
{
	private static List<ThingDef> availablePowerPlants = new List<ThingDef>();

	private const float MaxCoverage = 0.09f;

	public override bool CanResolve(ResolveParams rp)
	{
		if (!base.CanResolve(rp))
		{
			return false;
		}
		if (BaseGen.globalSettings.basePart_buildingsResolved < BaseGen.globalSettings.minBuildings)
		{
			return false;
		}
		if (BaseGen.globalSettings.basePart_landingPadsResolved < BaseGen.globalSettings.minLandingPads && rp.rect.Width >= 9 && rp.rect.Height >= 9)
		{
			return false;
		}
		if (BaseGen.globalSettings.basePart_emptyNodesResolved < BaseGen.globalSettings.minEmptyNodes)
		{
			return false;
		}
		if (BaseGen.globalSettings.basePart_powerPlantsCoverage + (float)rp.rect.Area / (float)BaseGen.globalSettings.mainRect.Area >= 0.09f)
		{
			return false;
		}
		if (rp.faction != null && (int)rp.faction.def.techLevel < 4)
		{
			return false;
		}
		if (rp.rect.Width > 13 || rp.rect.Height > 13)
		{
			return false;
		}
		CalculateAvailablePowerPlants(rp.rect);
		return availablePowerPlants.Any();
	}

	public override void Resolve(ResolveParams rp)
	{
		CalculateAvailablePowerPlants(rp.rect);
		if (availablePowerPlants.Any())
		{
			BaseGen.symbolStack.Push("refuel", rp);
			ThingDef thingDef = availablePowerPlants.RandomElement();
			ResolveParams resolveParams = rp;
			resolveParams.singleThingDef = thingDef;
			resolveParams.fillWithThingsPadding = rp.fillWithThingsPadding ?? Mathf.Max(5 - thingDef.size.x, 1);
			BaseGen.symbolStack.Push("fillWithThings", resolveParams);
			BaseGen.globalSettings.basePart_powerPlantsCoverage += (float)rp.rect.Area / (float)BaseGen.globalSettings.mainRect.Area;
		}
	}

	private void CalculateAvailablePowerPlants(CellRect rect)
	{
		Map map = BaseGen.globalSettings.map;
		availablePowerPlants.Clear();
		if (rect.Width >= ThingDefOf.SolarGenerator.size.x && rect.Height >= ThingDefOf.SolarGenerator.size.z)
		{
			int num = 0;
			foreach (IntVec3 item in rect)
			{
				if (!item.Roofed(map))
				{
					num++;
				}
			}
			if ((float)num / (float)rect.Area >= 0.5f)
			{
				availablePowerPlants.Add(ThingDefOf.SolarGenerator);
			}
		}
		if (rect.Width >= ThingDefOf.WoodFiredGenerator.size.x && rect.Height >= ThingDefOf.WoodFiredGenerator.size.z)
		{
			availablePowerPlants.Add(ThingDefOf.WoodFiredGenerator);
		}
	}
}
