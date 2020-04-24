namespace RimWorld
{
	public class TransferableComparer_MarketValue : TransferableComparer
	{
		public override int Compare(Transferable lhs, Transferable rhs)
		{
			return lhs.AnyThing.MarketValue.CompareTo(rhs.AnyThing.MarketValue);
		}
	}
}
