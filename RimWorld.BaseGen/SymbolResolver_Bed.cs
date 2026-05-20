using Verse;

namespace RimWorld.BaseGen;

public class SymbolResolver_Bed : SymbolResolver
{
	public override void Resolve(ResolveParams rp)
	{
		ThingDef singleThingDef = rp.singleThingDef ?? Rand.Element(ThingDefOf.Bed, ThingDefOf.Bedroll, ThingDefOf.SleepingSpot);
		ResolveParams resolveParams = rp;
		resolveParams.singleThingDef = singleThingDef;
		resolveParams.skipSingleThingIfHasToWipeBuildingOrDoesntFit = rp.skipSingleThingIfHasToWipeBuildingOrDoesntFit ?? true;
		BaseGen.symbolStack.Push("thing", resolveParams);
	}
}
