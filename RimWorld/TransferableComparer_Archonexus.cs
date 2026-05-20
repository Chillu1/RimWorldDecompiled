using Verse;

namespace RimWorld;

public class TransferableComparer_Archonexus : TransferableComparer
{
	public override int Compare(Transferable lhs, Transferable rhs)
	{
		return Compare(lhs.ThingDef, rhs.ThingDef);
	}

	public static int Compare(ThingDef lhsTh, ThingDef rhsTh)
	{
		float num = TransferableUIUtility.DefaultArchonexusItemListOrderPriority(lhsTh);
		float num2 = TransferableUIUtility.DefaultArchonexusItemListOrderPriority(rhsTh);
		if (num != num2)
		{
			return num.CompareTo(num2);
		}
		return TransferableComparer_Category.Compare(lhsTh, rhsTh);
	}
}
