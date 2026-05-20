namespace RimWorld.BaseGen;

public class SymbolResolver_AncientComplex_Mechanitor : SymbolResolver_AncientComplex_Base
{
	protected override LayoutDef DefaultLayoutDef => LayoutDefOf.AncientComplex_Mechanitor_Loot;

	public override void Resolve(ResolveParams rp)
	{
		ResolveParams resolveParams = rp;
		resolveParams.floorDef = TerrainDefOf.PackedDirt;
		BaseGen.symbolStack.Push("outdoorsPath", resolveParams);
		BaseGen.symbolStack.Push("ensureCanReachMapEdge", rp);
		ResolveComplex(rp);
	}
}
