namespace RimWorld.BaseGen
{
	public class SymbolResolver_BasePart_Indoors_Leaf_DiningRoom : SymbolResolver
	{
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
			return true;
		}

		public override void Resolve(ResolveParams rp)
		{
			BaseGen.symbolStack.Push("diningRoom", rp);
		}
	}
}
