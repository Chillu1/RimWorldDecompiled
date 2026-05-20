using System.Collections.Generic;
using Verse;

namespace RimWorld.BaseGen;

public class SymbolResolver_BasePart_Outdoors_Leaf_LandingPad : SymbolResolver
{
	private static List<ThingDef> availablePowerPlants = new List<ThingDef>();

	public const int Size = 9;

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
		if (BaseGen.globalSettings.basePart_emptyNodesResolved < BaseGen.globalSettings.minEmptyNodes && BaseGen.globalSettings.basePart_landingPadsResolved >= BaseGen.globalSettings.minLandingPads)
		{
			return false;
		}
		if (BaseGen.globalSettings.basePart_landingPadsResolved != 0 && BaseGen.globalSettings.basePart_landingPadsResolved >= BaseGen.globalSettings.minLandingPads)
		{
			return false;
		}
		if (rp.faction == null || rp.faction != Faction.OfEmpire)
		{
			return false;
		}
		return true;
	}

	public override void Resolve(ResolveParams rp)
	{
		if (rp.rect.TryFindRandomInnerRect(new IntVec2(9, 9), out var rect))
		{
			ResolveParams resolveParams = rp;
			resolveParams.rect = rect;
			BaseGen.symbolStack.Push("landingPad", resolveParams);
			BaseGen.globalSettings.basePart_landingPadsResolved++;
		}
	}
}
