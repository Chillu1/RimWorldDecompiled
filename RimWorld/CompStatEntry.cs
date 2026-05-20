using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class CompStatEntry : ThingComp
	{
		private CompProperties_StatEntry Props => (CompProperties_StatEntry)props;

		public override IEnumerable<StatDrawEntry> SpecialDisplayStats()
		{
			yield return new StatDrawEntry(Props.statCategoryDef, Props.statLabel, Props.valueFunc?.Invoke(this) ?? Props.valueString, Props.reportText, Props.displayPriorityInCategory);
		}
	}
}
