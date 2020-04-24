namespace RimWorld
{
	public class TransferableComparer_Mass : TransferableComparer
	{
		public override int Compare(Transferable lhs, Transferable rhs)
		{
			return lhs.AnyThing.GetStatValue(StatDefOf.Mass).CompareTo(rhs.AnyThing.GetStatValue(StatDefOf.Mass));
		}
	}
}
