using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld.BaseGen
{
	public class SymbolResolver_ThingSet : SymbolResolver
	{
		public override void Resolve(ResolveParams rp)
		{
			Map map = BaseGen.globalSettings.map;
			ThingSetMakerDef obj = rp.thingSetMakerDef ?? ThingSetMakerDefOf.MapGen_DefaultStockpile;
			ThingSetMakerParams parms;
			if (rp.thingSetMakerParams.HasValue)
			{
				parms = rp.thingSetMakerParams.Value;
			}
			else
			{
				int num = rp.rect.Cells.Count((IntVec3 x) => x.Standable(map) && x.GetFirstItem(map) == null);
				parms = new ThingSetMakerParams
				{
					countRange = new IntRange(num, num),
					techLevel = ((rp.faction != null) ? rp.faction.def.techLevel : TechLevel.Undefined)
				};
			}
			parms.makingFaction = rp.faction;
			List<Thing> list = obj.root.Generate(parms);
			for (int i = 0; i < list.Count; i++)
			{
				ResolveParams resolveParams = rp;
				resolveParams.singleThingToSpawn = list[i];
				BaseGen.symbolStack.Push("thing", resolveParams);
			}
		}
	}
}
