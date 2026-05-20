using Verse;

namespace RimWorld.BaseGen
{
	public class SymbolResolver_BasePart_Indoors : SymbolResolver
	{
		public override void Resolve(ResolveParams rp)
		{
			int? minLengthAfterSplit = null;
			bool flag;
			if (BaseGen.globalSettings.basePart_worshippedTerminalsResolved >= BaseGen.globalSettings.requiredWorshippedTerminalRooms)
			{
				flag = rp.rect.Width > 13 || rp.rect.Height > 13 || ((rp.rect.Width >= 9 || rp.rect.Height >= 9) && Rand.Chance(0.3f));
			}
			else
			{
				minLengthAfterSplit = 7;
				flag = ((rp.rect.Width >= 14 && rp.rect.Height >= 14) ? true : false);
			}
			if (flag)
			{
				ResolveParams resolveParams = rp;
				resolveParams.minLengthAfterSplit = minLengthAfterSplit;
				BaseGen.symbolStack.Push("basePart_indoors_division", resolveParams);
			}
			else
			{
				BaseGen.symbolStack.Push("basePart_indoors_leaf", rp);
			}
		}
	}
}
