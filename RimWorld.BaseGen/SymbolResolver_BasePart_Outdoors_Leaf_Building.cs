namespace RimWorld.BaseGen;

public class SymbolResolver_BasePart_Outdoors_Leaf_Building : SymbolResolver
{
	public override bool CanResolve(ResolveParams rp)
	{
		if (!base.CanResolve(rp))
		{
			return false;
		}
		if (BaseGen.globalSettings.basePart_landingPadsResolved < BaseGen.globalSettings.minLandingPads && BaseGen.globalSettings.basePart_buildingsResolved >= BaseGen.globalSettings.minBuildings && rp.rect.Width >= 9 && rp.rect.Height >= 9)
		{
			return false;
		}
		if (BaseGen.globalSettings.basePart_emptyNodesResolved < BaseGen.globalSettings.minEmptyNodes && BaseGen.globalSettings.basePart_buildingsResolved >= BaseGen.globalSettings.minBuildings)
		{
			return false;
		}
		return true;
	}

	public override void Resolve(ResolveParams rp)
	{
		ResolveParams resolveParams = rp;
		resolveParams.wallStuff = rp.wallStuff ?? BaseGenUtility.RandomCheapWallStuff(rp.faction);
		resolveParams.floorDef = rp.floorDef ?? BaseGenUtility.RandomBasicFloorDef(rp.faction, allowCarpet: true);
		BaseGen.symbolStack.Push("basePart_indoors", resolveParams);
		BaseGen.globalSettings.basePart_buildingsResolved++;
	}
}
