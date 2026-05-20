using System.Collections.Generic;
using Verse;

namespace RimWorld.BaseGen;

public class SymbolResolver_LootScatter : SymbolResolver
{
	private static readonly IntRange DefaultLootCountRange = new IntRange(3, 10);

	public override void Resolve(ResolveParams rp)
	{
		float? lootMarketValue = rp.lootMarketValue;
		if (!lootMarketValue.HasValue || !(lootMarketValue.GetValueOrDefault() <= 0f))
		{
			Map map = BaseGen.globalSettings.map;
			IList<Thing> list = rp.lootConcereteContents;
			if (list == null)
			{
				ThingSetMakerParams parms = ((!rp.thingSetMakerParams.HasValue) ? new ThingSetMakerParams
				{
					countRange = DefaultLootCountRange,
					techLevel = ((rp.faction != null) ? rp.faction.def.techLevel : TechLevel.Undefined)
				} : rp.thingSetMakerParams.Value);
				parms.makingFaction = rp.faction;
				parms.totalMarketValueRange = new FloatRange(rp.lootMarketValue.Value, rp.lootMarketValue.Value);
				list = rp.thingSetMakerDef.root.Generate(parms);
			}
			MapGenUtility.GenerateLoot(map, rp.rect, list);
		}
	}
}
