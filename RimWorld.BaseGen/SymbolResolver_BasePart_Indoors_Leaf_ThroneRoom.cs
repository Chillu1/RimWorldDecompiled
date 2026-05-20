namespace RimWorld.BaseGen;

public class SymbolResolver_BasePart_Indoors_Leaf_ThroneRoom : SymbolResolver
{
	public override bool CanResolve(ResolveParams rp)
	{
		if (!base.CanResolve(rp))
		{
			return false;
		}
		if (rp.faction == null || rp.faction != Faction.OfEmpire)
		{
			return false;
		}
		if (rp.allowGeneratingThronerooms.HasValue && !rp.allowGeneratingThronerooms.Value)
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
		if (BaseGen.globalSettings.basePart_barracksResolved < BaseGen.globalSettings.minBarracks && BaseGen.globalSettings.basePart_throneRoomsResolved >= BaseGen.globalSettings.minThroneRooms)
		{
			return false;
		}
		if (BaseGen.globalSettings.basePart_throneRoomsResolved != 0 && BaseGen.globalSettings.basePart_throneRoomsResolved >= BaseGen.globalSettings.minThroneRooms)
		{
			return false;
		}
		return true;
	}

	public override void Resolve(ResolveParams rp)
	{
		BaseGen.symbolStack.Push("throneRoom", rp);
		BaseGen.globalSettings.basePart_throneRoomsResolved++;
	}
}
