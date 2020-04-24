using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class PawnTableDef : Def
	{
		public List<PawnColumnDef> columns;

		public Type workerClass = typeof(PawnTable);

		public int minWidth = 998;
	}
}
