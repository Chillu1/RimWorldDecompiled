using UnityEngine;
using Verse;

namespace RimWorld.BaseGen
{
	public class SymbolResolver_Farm : SymbolResolver
	{
		public override bool CanResolve(ResolveParams rp)
		{
			if (base.CanResolve(rp))
			{
				if (rp.cultivatedPlantDef == null)
				{
					return SymbolResolver_CultivatedPlants.DeterminePlantDef(rp.rect) != null;
				}
				return true;
			}
			return false;
		}

		public override void Resolve(ResolveParams rp)
		{
			ThingDef thingDef = rp.cultivatedPlantDef ?? SymbolResolver_CultivatedPlants.DeterminePlantDef(rp.rect);
			if (rp.rect.Width >= 7 && rp.rect.Height >= 7 && (rp.rect.Width > 12 || rp.rect.Height > 12 || Rand.Bool) && thingDef.plant.Harvestable)
			{
				CellRect rect = new CellRect(rp.rect.maxX - 3, rp.rect.maxZ - 3, 4, 4);
				ThingDef harvestedThingDef = thingDef.plant.harvestedThingDef;
				int num = Rand.RangeInclusive(2, 3);
				for (int i = 0; i < num; i++)
				{
					ResolveParams resolveParams = rp;
					resolveParams.rect = rect.ContractedBy(1);
					resolveParams.singleThingDef = harvestedThingDef;
					resolveParams.singleThingStackCount = Rand.RangeInclusive(Mathf.Min(10, harvestedThingDef.stackLimit), Mathf.Min(50, harvestedThingDef.stackLimit));
					BaseGen.symbolStack.Push("thing", resolveParams);
				}
				ResolveParams resolveParams2 = rp;
				resolveParams2.rect = rect;
				BaseGen.symbolStack.Push("ensureCanReachMapEdge", resolveParams2);
				ResolveParams resolveParams3 = rp;
				resolveParams3.rect = rect;
				BaseGen.symbolStack.Push("emptyRoom", resolveParams3);
			}
			ResolveParams resolveParams4 = rp;
			resolveParams4.cultivatedPlantDef = thingDef;
			BaseGen.symbolStack.Push("cultivatedPlants", resolveParams4);
		}
	}
}
