using Verse;

namespace RimWorld.BaseGen
{
	public class SymbolResolver_BasePart_Outdoors : SymbolResolver
	{
		public override void Resolve(ResolveParams rp)
		{
			bool flag = rp.rect.Width > 23 || rp.rect.Height > 23 || ((rp.rect.Width >= 11 || rp.rect.Height >= 11) && Rand.Bool);
			ResolveParams resolveParams = rp;
			resolveParams.pathwayFloorDef = rp.pathwayFloorDef ?? BaseGenUtility.RandomBasicFloorDef(rp.faction);
			if (flag)
			{
				BaseGen.symbolStack.Push("basePart_outdoors_division", resolveParams);
			}
			else
			{
				BaseGen.symbolStack.Push("basePart_outdoors_leafPossiblyDecorated", resolveParams);
			}
		}
	}
}
