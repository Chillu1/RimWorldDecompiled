using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class DrugPolicyDef : Def
	{
		public bool allowPleasureDrugs;

		public List<DrugPolicyEntry> entries;
	}
}
