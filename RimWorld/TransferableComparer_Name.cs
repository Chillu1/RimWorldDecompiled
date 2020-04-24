namespace RimWorld
{
	public class TransferableComparer_Name : TransferableComparer
	{
		public override int Compare(Transferable lhs, Transferable rhs)
		{
			return lhs.Label.CompareTo(rhs.Label);
		}
	}
}
