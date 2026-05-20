namespace RimWorld.BaseGen
{
	public class SymbolResolver_WorkSite : SymbolResolver
	{
		public override void Resolve(ResolveParams rp)
		{
			BaseGen.symbolStack.Push("storage", rp);
		}
	}
}
