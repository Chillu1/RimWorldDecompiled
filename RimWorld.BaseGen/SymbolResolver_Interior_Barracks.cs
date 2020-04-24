namespace RimWorld.BaseGen
{
	public class SymbolResolver_Interior_Barracks : SymbolResolver
	{
		public override void Resolve(ResolveParams rp)
		{
			InteriorSymbolResolverUtility.PushBedroomHeatersCoolersAndLightSourcesSymbols(rp);
			BaseGen.symbolStack.Push("fillWithBeds", rp);
		}
	}
}
