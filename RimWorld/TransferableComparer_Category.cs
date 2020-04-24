using Verse;

namespace RimWorld
{
	public class TransferableComparer_Category : TransferableComparer
	{
		public override int Compare(Transferable lhs, Transferable rhs)
		{
			return Compare(lhs.ThingDef, rhs.ThingDef);
		}

		public static int Compare(ThingDef lhsTh, ThingDef rhsTh)
		{
			if (lhsTh.category != rhsTh.category)
			{
				return lhsTh.category.CompareTo(rhsTh.category);
			}
			float num = TransferableUIUtility.DefaultListOrderPriority(lhsTh);
			float num2 = TransferableUIUtility.DefaultListOrderPriority(rhsTh);
			if (num != num2)
			{
				return num.CompareTo(num2);
			}
			int num3 = 0;
			if (!lhsTh.thingCategories.NullOrEmpty())
			{
				num3 = lhsTh.thingCategories[0].index;
			}
			int value = 0;
			if (!rhsTh.thingCategories.NullOrEmpty())
			{
				value = rhsTh.thingCategories[0].index;
			}
			return num3.CompareTo(value);
		}
	}
}
