namespace RimWorld.BaseGen;

public class SymbolResolver_BasePart_Indoors_Leaf_BatteryRoom : SymbolResolver
{
	private const float MaxCoverage = 0.06f;

	public override bool CanResolve(ResolveParams rp)
	{
		if (!base.CanResolve(rp))
		{
			return false;
		}
		if (BaseGen.globalSettings.basePart_throneRoomsResolved < BaseGen.globalSettings.minThroneRooms)
		{
			return false;
		}
		if (BaseGen.globalSettings.basePart_barracksResolved < BaseGen.globalSettings.minBarracks)
		{
			return false;
		}
		if (BaseGen.globalSettings.basePart_worshippedTerminalsResolved < BaseGen.globalSettings.requiredWorshippedTerminalRooms && SymbolResolver_BasePart_Indoors_Leaf_WorshippedTerminal.CanResolve("basePart_indoors_leaf", rp))
		{
			return false;
		}
		if (BaseGen.globalSettings.basePart_gravcoresResolved < BaseGen.globalSettings.requiredGravcoreRooms && SymbolResolver_BasePart_Indoors_Leaf_Gravcore.CanResolve("basePart_indoors_leaf", rp))
		{
			return false;
		}
		if (BaseGen.globalSettings.basePart_batteriesCoverage + (float)rp.rect.Area / (float)BaseGen.globalSettings.mainRect.Area >= 0.06f)
		{
			return false;
		}
		if (rp.faction != null)
		{
			return (int)rp.faction.def.techLevel >= 4;
		}
		return true;
	}

	public override void Resolve(ResolveParams rp)
	{
		BaseGen.symbolStack.Push("batteryRoom", rp);
		BaseGen.globalSettings.basePart_batteriesCoverage += (float)rp.rect.Area / (float)BaseGen.globalSettings.mainRect.Area;
	}
}
