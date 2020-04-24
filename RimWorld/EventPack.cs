using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public struct EventPack
	{
		private string tagInt;

		private IntVec3 cellInt;

		private IEnumerable<IntVec3> cellsInt;

		public string Tag => tagInt;

		public IntVec3 Cell => cellInt;

		public IEnumerable<IntVec3> Cells => cellsInt;

		public EventPack(string tag)
		{
			tagInt = tag;
			cellInt = IntVec3.Invalid;
			cellsInt = null;
		}

		public EventPack(string tag, IntVec3 cell)
		{
			tagInt = tag;
			cellInt = cell;
			cellsInt = null;
		}

		public EventPack(string tag, IEnumerable<IntVec3> cells)
		{
			tagInt = tag;
			cellInt = IntVec3.Invalid;
			cellsInt = cells;
		}

		public static implicit operator EventPack(string s)
		{
			return new EventPack(s);
		}

		public override string ToString()
		{
			if (Cell.IsValid)
			{
				return Tag + "-" + Cell;
			}
			return Tag;
		}
	}
}
