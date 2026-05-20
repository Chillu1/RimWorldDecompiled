using Verse;

namespace RimWorld.BaseGen;

public class SymbolResolver_BasePart_Indoors_Division_Split : SymbolResolver
{
	private const int MinLengthAfterSplit = 5;

	private int ResolveMinLengthAfterSplit(ResolveParams rp)
	{
		return rp.minLengthAfterSplit ?? 5;
	}

	private int ResolveMinWidthOrHeight(int minLengthAfterSplit)
	{
		return minLengthAfterSplit * 2 - 1;
	}

	public override bool CanResolve(ResolveParams rp)
	{
		int num = ResolveMinWidthOrHeight(ResolveMinLengthAfterSplit(rp));
		if (base.CanResolve(rp))
		{
			if (rp.rect.Width < num)
			{
				return rp.rect.Height >= num;
			}
			return true;
		}
		return false;
	}

	public override void Resolve(ResolveParams rp)
	{
		int num = ResolveMinLengthAfterSplit(rp);
		int num2 = ResolveMinWidthOrHeight(num);
		if (rp.rect.Width < num2 && rp.rect.Height < num2)
		{
			ResolveParams resolveParams = rp;
			Log.Warning("Too small rect. params=" + resolveParams.ToString());
		}
		else if ((Rand.Bool && rp.rect.Height >= num2) || rp.rect.Width < num2)
		{
			int num3 = Rand.RangeInclusive(num - 1, rp.rect.Height - num);
			ResolveParams resolveParams2 = rp;
			resolveParams2.rect = new CellRect(rp.rect.minX, rp.rect.minZ, rp.rect.Width, num3 + 1);
			BaseGen.symbolStack.Push("basePart_indoors", resolveParams2);
			ResolveParams resolveParams3 = rp;
			resolveParams3.rect = new CellRect(rp.rect.minX, rp.rect.minZ + num3, rp.rect.Width, rp.rect.Height - num3);
			BaseGen.symbolStack.Push("basePart_indoors", resolveParams3);
		}
		else
		{
			int num4 = Rand.RangeInclusive(num - 1, rp.rect.Width - num);
			ResolveParams resolveParams4 = rp;
			resolveParams4.rect = new CellRect(rp.rect.minX, rp.rect.minZ, num4 + 1, rp.rect.Height);
			BaseGen.symbolStack.Push("basePart_indoors", resolveParams4);
			ResolveParams resolveParams5 = rp;
			resolveParams5.rect = new CellRect(rp.rect.minX + num4, rp.rect.minZ, rp.rect.Width - num4, rp.rect.Height);
			BaseGen.symbolStack.Push("basePart_indoors", resolveParams5);
		}
	}
}
