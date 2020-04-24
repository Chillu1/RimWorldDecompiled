namespace RimWorld.BaseGen
{
	public class SymbolResolver_Interior_PrisonCell : SymbolResolver
	{
		private const int FoodStockpileSize = 3;

		public override void Resolve(ResolveParams rp)
		{
			ThingSetMakerParams value = default(ThingSetMakerParams);
			value.techLevel = ((rp.faction != null) ? rp.faction.def.techLevel : TechLevel.Spacer);
			ResolveParams resolveParams = rp;
			resolveParams.thingSetMakerDef = ThingSetMakerDefOf.MapGen_PrisonCellStockpile;
			resolveParams.thingSetMakerParams = value;
			resolveParams.innerStockpileSize = 3;
			BaseGen.symbolStack.Push("innerStockpile", resolveParams);
			InteriorSymbolResolverUtility.PushBedroomHeatersCoolersAndLightSourcesSymbols(rp, hasToSpawnLightSource: false);
			BaseGen.symbolStack.Push("prisonerBed", rp);
		}
	}
}
