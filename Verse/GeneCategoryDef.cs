using System.Collections.Generic;
using System.Linq;

namespace Verse
{
	public class GeneCategoryDef : Def
	{
		public float displayPriorityInXenotype;

		public float displayPriorityInGenepack;

		public override IEnumerable<string> ConfigErrors()
		{
			foreach (string item in base.ConfigErrors())
			{
				yield return item;
			}
			if (DefDatabase<GeneCategoryDef>.AllDefs.Any((GeneCategoryDef x) => x != this && x.displayPriorityInXenotype == displayPriorityInXenotype))
			{
				yield return "Multiple gene categories share the same displayPriorityInXenotype. This can cause display order issues.";
			}
		}
	}
}
