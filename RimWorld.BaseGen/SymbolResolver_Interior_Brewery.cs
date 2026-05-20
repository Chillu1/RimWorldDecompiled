using Verse;

namespace RimWorld.BaseGen;

public class SymbolResolver_Interior_Brewery : SymbolResolver
{
	private const float SpawnHeaterIfTemperatureBelow = 7f;

	private float SpawnPassiveCoolerIfTemperatureAbove => ThingDefOf.FermentingBarrel.GetCompProperties<CompProperties_TemperatureRuinable>().maxSafeTemperature;

	public override void Resolve(ResolveParams rp)
	{
		Map map = BaseGen.globalSettings.map;
		if (map.mapTemperature.OutdoorTemp > SpawnPassiveCoolerIfTemperatureAbove)
		{
			ResolveParams resolveParams = rp;
			resolveParams.singleThingDef = ThingDefOf.PassiveCooler;
			BaseGen.symbolStack.Push("edgeThing", resolveParams);
		}
		if (map.mapTemperature.OutdoorTemp < 7f)
		{
			ThingDef singleThingDef = ((rp.faction != null && (int)rp.faction.def.techLevel < 4) ? ThingDefOf.Campfire : ThingDefOf.Heater);
			ResolveParams resolveParams2 = rp;
			resolveParams2.singleThingDef = singleThingDef;
			BaseGen.symbolStack.Push("edgeThing", resolveParams2);
		}
		BaseGen.symbolStack.Push("addWortToFermentingBarrels", rp);
		ResolveParams resolveParams3 = rp;
		resolveParams3.singleThingDef = ThingDefOf.FermentingBarrel;
		resolveParams3.thingRot = (Rand.Bool ? Rot4.North : Rot4.East);
		resolveParams3.fillWithThingsPadding = rp.fillWithThingsPadding ?? 1;
		BaseGen.symbolStack.Push("fillWithThings", resolveParams3);
	}
}
