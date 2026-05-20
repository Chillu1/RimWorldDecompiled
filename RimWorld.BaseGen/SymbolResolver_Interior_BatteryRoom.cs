using Verse;

namespace RimWorld.BaseGen;

public class SymbolResolver_Interior_BatteryRoom : SymbolResolver
{
	public override void Resolve(ResolveParams rp)
	{
		BaseGen.symbolStack.Push("indoorLighting", rp);
		BaseGen.symbolStack.Push("chargeBatteries", rp);
		ResolveParams resolveParams = rp;
		resolveParams.singleThingDef = ThingDefOf.Battery;
		resolveParams.thingRot = (Rand.Bool ? Rot4.North : Rot4.East);
		resolveParams.fillWithThingsPadding = rp.fillWithThingsPadding ?? 1;
		BaseGen.symbolStack.Push("fillWithThings", resolveParams);
	}
}
