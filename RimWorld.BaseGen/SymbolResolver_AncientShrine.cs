using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld.BaseGen;

public class SymbolResolver_AncientShrine : SymbolResolver
{
	public float techprintChance;

	public float bladelinkChance;

	public float psychicChance;

	public override void Resolve(ResolveParams rp)
	{
		IntVec3 min = rp.rect.Min;
		Map map = BaseGen.globalSettings.map;
		CellRect rect = new CellRect(min.x + rp.rect.Width / 2 - 1, min.z + rp.rect.Height / 2, 2, 1);
		foreach (IntVec3 item in rect)
		{
			List<Thing> thingList = item.GetThingList(map);
			for (int i = 0; i < thingList.Count; i++)
			{
				if (!thingList[i].def.destroyable)
				{
					return;
				}
			}
		}
		if (Rand.Chance(techprintChance))
		{
			ResolveParams resolveParams = rp;
			if (DefDatabase<ThingDef>.AllDefsListForReading.Where((ThingDef t) => t.tradeTags != null && t.tradeTags.Contains("Techprint")).TryRandomElement(out resolveParams.singleThingDef))
			{
				BaseGen.symbolStack.Push("thing", resolveParams);
			}
		}
		if (ModsConfig.RoyaltyActive && Rand.Chance(bladelinkChance))
		{
			ResolveParams resolveParams2 = rp;
			if (DefDatabase<ThingDef>.AllDefsListForReading.Where((ThingDef t) => t.weaponTags != null && t.weaponTags.Contains("Bladelink")).TryRandomElement(out resolveParams2.singleThingDef))
			{
				BaseGen.symbolStack.Push("thing", resolveParams2);
			}
		}
		if (ModsConfig.RoyaltyActive && Rand.Chance(psychicChance))
		{
			ResolveParams resolveParams3 = rp;
			if (DefDatabase<ThingDef>.AllDefsListForReading.Where((ThingDef t) => t.tradeTags != null && t.tradeTags.Contains("Psychic")).TryRandomElement(out resolveParams3.singleThingDef))
			{
				BaseGen.symbolStack.Push("thing", resolveParams3);
			}
		}
		ResolveParams resolveParams4 = rp;
		resolveParams4.rect = CellRect.SingleCell(rect.Min);
		resolveParams4.thingRot = Rot4.East;
		BaseGen.symbolStack.Push("ancientCryptosleepCasket", resolveParams4);
		ResolveParams resolveParams5 = rp;
		resolveParams5.rect = rect;
		resolveParams5.floorDef = TerrainDefOf.AncientConcrete;
		BaseGen.symbolStack.Push("floor", resolveParams5);
		ResolveParams resolveParams6 = rp;
		resolveParams6.floorDef = rp.floorDef ?? TerrainDefOf.AncientTile;
		BaseGen.symbolStack.Push("floor", resolveParams6);
	}
}
