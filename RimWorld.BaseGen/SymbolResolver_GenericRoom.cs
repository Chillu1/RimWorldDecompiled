using Verse;

namespace RimWorld.BaseGen
{
	public class SymbolResolver_GenericRoom : SymbolResolver
	{
		public string interior;

		public override void Resolve(ResolveParams rp)
		{
			BaseGen.symbolStack.Push("doors", rp);
			if (!interior.NullOrEmpty())
			{
				ResolveParams resolveParams = rp;
				resolveParams.rect = rp.rect.ContractedBy(1);
				BaseGen.symbolStack.Push(interior, resolveParams);
			}
			BaseGen.symbolStack.Push("emptyRoom", rp);
		}
	}
}
