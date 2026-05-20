namespace RimWorld.BaseGen;

public class SymbolResolver_AncientComplex_Sketch : SymbolResolver
{
	public override bool CanResolve(ResolveParams rp)
	{
		if (base.CanResolve(rp))
		{
			return rp.ancientLayoutStructureSketch != null;
		}
		return false;
	}

	public override void Resolve(ResolveParams rp)
	{
		rp.ancientLayoutStructureSketch.layoutDef.Worker.Spawn(rp.ancientLayoutStructureSketch, BaseGen.globalSettings.map, rp.rect.Min, rp.threatPoints);
	}
}
