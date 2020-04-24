using Verse;

namespace RimWorld.BaseGen
{
	public class SymbolResolver_BasePart_Indoors_Division_Split : SymbolResolver
	{
		private const int MinLengthAfterSplit = 5;

		private const int MinWidthOrHeight = 9;

		public override bool CanResolve(ResolveParams rp)
		{
			if (base.CanResolve(rp))
			{
				if (rp.rect.Width < 9)
				{
					return rp.rect.Height >= 9;
				}
				return true;
			}
			return false;
		}

		public override void Resolve(ResolveParams rp)
		{
			if (rp.rect.Width < 9 && rp.rect.Height < 9)
			{
				Log.Warning("Too small rect. params=" + rp);
			}
			else if ((Rand.Bool && rp.rect.Height >= 9) || rp.rect.Width < 9)
			{
				int num = Rand.RangeInclusive(4, rp.rect.Height - 5);
				ResolveParams resolveParams = rp;
				resolveParams.rect = new CellRect(rp.rect.minX, rp.rect.minZ, rp.rect.Width, num + 1);
				BaseGen.symbolStack.Push("basePart_indoors", resolveParams);
				ResolveParams resolveParams2 = rp;
				resolveParams2.rect = new CellRect(rp.rect.minX, rp.rect.minZ + num, rp.rect.Width, rp.rect.Height - num);
				BaseGen.symbolStack.Push("basePart_indoors", resolveParams2);
			}
			else
			{
				int num2 = Rand.RangeInclusive(4, rp.rect.Width - 5);
				ResolveParams resolveParams3 = rp;
				resolveParams3.rect = new CellRect(rp.rect.minX, rp.rect.minZ, num2 + 1, rp.rect.Height);
				BaseGen.symbolStack.Push("basePart_indoors", resolveParams3);
				ResolveParams resolveParams4 = rp;
				resolveParams4.rect = new CellRect(rp.rect.minX + num2, rp.rect.minZ, rp.rect.Width - num2, rp.rect.Height);
				BaseGen.symbolStack.Push("basePart_indoors", resolveParams4);
			}
		}
	}
}
