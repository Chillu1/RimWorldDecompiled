namespace RimWorld.BaseGen
{
	public class SymbolResolver_BasePart_Indoors_Leaf_Barracks : SymbolResolver
	{
		public override void Resolve(ResolveParams rp)
		{
			BaseGen.symbolStack.Push("barracks", rp);
			BaseGen.globalSettings.basePart_barracksResolved++;
		}
	}
}
