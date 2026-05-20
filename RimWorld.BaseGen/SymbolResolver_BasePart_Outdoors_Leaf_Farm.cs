namespace RimWorld.BaseGen;

public class SymbolResolver_BasePart_Outdoors_Leaf_Farm : SymbolResolver
{
	private const float MaxCoverage = 0.55f;

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
		if (BaseGen.globalSettings.basePart_emptyNodesResolved < BaseGen.globalSettings.minEmptyNodes)
		{
			return false;
		}
		if (BaseGen.globalSettings.basePart_farmsCoverage + (float)rp.rect.Area / (float)BaseGen.globalSettings.mainRect.Area >= 0.55f || (BaseGen.globalSettings.maxFarms > -1 && BaseGen.globalSettings.basePart_farmsCount >= BaseGen.globalSettings.maxFarms))
		{
			return false;
		}
		if (rp.rect.Width <= 15 && rp.rect.Height <= 15)
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
		BaseGen.symbolStack.Push("farm", rp);
		BaseGen.globalSettings.basePart_farmsCoverage += (float)rp.rect.Area / (float)BaseGen.globalSettings.mainRect.Area;
		BaseGen.globalSettings.basePart_farmsCount++;
	}
}
