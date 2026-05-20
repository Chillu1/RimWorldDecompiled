using Verse;

namespace RimWorld.BaseGen;

public class SymbolResolver_IndoorLighting : SymbolResolver
{
	private const float NeverSpawnTorchesIfTemperatureAbove = 18f;

	public override void Resolve(ResolveParams rp)
	{
		Map map = BaseGen.globalSettings.map;
		ThingDef thingDef = ((rp.faction == null || (int)rp.faction.def.techLevel >= 4) ? ThingDefOf.StandingLamp : ((!(map.mapTemperature.OutdoorTemp > 18f)) ? ThingDefOf.TorchLamp : null));
		if (thingDef != null)
		{
			ResolveParams resolveParams = rp;
			resolveParams.singleThingDef = thingDef;
			BaseGen.symbolStack.Push("edgeThing", resolveParams);
		}
	}
}
