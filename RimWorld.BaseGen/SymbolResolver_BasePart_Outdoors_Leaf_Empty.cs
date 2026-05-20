namespace RimWorld.BaseGen;

public class SymbolResolver_BasePart_Outdoors_Leaf_Empty : SymbolResolver
{
	public override bool CanResolve(ResolveParams rp)
	{
		if (!base.CanResolve(rp))
		{
			return false;
		}
		if (BaseGen.globalSettings.basePart_buildingsResolved < BaseGen.globalSettings.minBuildings)
		{
			return false;
		}
		if (BaseGen.globalSettings.basePart_landingPadsResolved < BaseGen.globalSettings.minLandingPads && rp.rect.Width >= 9 && rp.rect.Height >= 9)
		{
			return false;
		}
		return true;
	}

	public override void Resolve(ResolveParams rp)
	{
		BaseGen.globalSettings.basePart_emptyNodesResolved++;
	}
}
