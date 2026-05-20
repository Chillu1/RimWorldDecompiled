using Verse;

namespace RimWorld.BaseGen;

public static class InteriorSymbolResolverUtility
{
	private const float SpawnHeaterIfTemperatureBelow = 3f;

	private const float SpawnSecondHeaterIfTemperatureBelow = -45f;

	private const float NonIndustrial_SpawnCampfireIfTemperatureBelow = -20f;

	private const float SpawnPassiveCoolerIfTemperatureAbove = 22f;

	public static void PushBedroomHeatersCoolersAndLightSourcesSymbols(ResolveParams rp, bool hasToSpawnLightSource = true)
	{
		Map map = BaseGen.globalSettings.map;
		if (map.mapTemperature.OutdoorTemp > 22f)
		{
			ResolveParams resolveParams = rp;
			resolveParams.singleThingDef = ThingDefOf.PassiveCooler;
			BaseGen.symbolStack.Push("edgeThing", resolveParams);
		}
		bool flag = false;
		if (map.mapTemperature.OutdoorTemp < 3f)
		{
			ThingDef singleThingDef;
			if (rp.faction == null || (int)rp.faction.def.techLevel >= 4)
			{
				singleThingDef = ThingDefOf.Heater;
			}
			else
			{
				singleThingDef = ((map.mapTemperature.OutdoorTemp < -20f) ? ThingDefOf.Campfire : ThingDefOf.TorchLamp);
				flag = true;
			}
			int num = ((!(map.mapTemperature.OutdoorTemp < -45f)) ? 1 : 2);
			for (int i = 0; i < num; i++)
			{
				ResolveParams resolveParams2 = rp;
				resolveParams2.singleThingDef = singleThingDef;
				BaseGen.symbolStack.Push("edgeThing", resolveParams2);
			}
		}
		if (!flag && hasToSpawnLightSource)
		{
			BaseGen.symbolStack.Push("indoorLighting", rp);
		}
	}
}
