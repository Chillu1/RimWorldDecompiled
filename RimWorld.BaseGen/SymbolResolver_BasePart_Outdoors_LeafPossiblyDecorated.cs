using Verse;

namespace RimWorld.BaseGen
{
	public class SymbolResolver_BasePart_Outdoors_LeafPossiblyDecorated : SymbolResolver
	{
		public override void Resolve(ResolveParams rp)
		{
			if (rp.rect.Width >= 10 && rp.rect.Height >= 10 && Rand.Chance(0.25f))
			{
				BaseGen.symbolStack.Push("basePart_outdoors_leafDecorated", rp);
			}
			else
			{
				BaseGen.symbolStack.Push("basePart_outdoors_leaf", rp);
			}
		}
	}
}
