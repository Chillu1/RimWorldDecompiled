namespace RimWorld;

public class TransferableComparer_Quality : TransferableComparer
{
	public override int Compare(Transferable lhs, Transferable rhs)
	{
		return GetValueFor(lhs).CompareTo(GetValueFor(rhs));
	}

	private int GetValueFor(Transferable t)
	{
		if (!t.AnyThing.TryGetQuality(out var qc))
		{
			return -1;
		}
		return (int)qc;
	}
}
