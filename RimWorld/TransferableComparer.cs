using System.Collections.Generic;

namespace RimWorld
{
	public abstract class TransferableComparer : IComparer<Transferable>
	{
		public abstract int Compare(Transferable lhs, Transferable rhs);
	}
}
