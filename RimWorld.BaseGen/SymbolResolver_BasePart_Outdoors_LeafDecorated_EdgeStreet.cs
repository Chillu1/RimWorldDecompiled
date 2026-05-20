namespace RimWorld.BaseGen;

public class SymbolResolver_BasePart_Outdoors_LeafDecorated_EdgeStreet : SymbolResolver
{
	public override void Resolve(ResolveParams rp)
	{
		ResolveParams resolveParams = rp;
		resolveParams.floorDef = rp.pathwayFloorDef ?? BaseGenUtility.RandomBasicFloorDef(rp.faction);
		BaseGen.symbolStack.Push("edgeStreet", resolveParams);
		ResolveParams resolveParams2 = rp;
		resolveParams2.rect = rp.rect.ContractedBy(1);
		BaseGen.symbolStack.Push("basePart_outdoors_leaf", resolveParams2);
	}
}
