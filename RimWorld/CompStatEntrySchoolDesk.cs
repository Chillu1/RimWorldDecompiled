using System.Collections.Generic;
using System.Text;
using Verse;

namespace RimWorld
{
	public class CompStatEntrySchoolDesk : CompStatEntry
	{
		private CompProperties_StatEntry Props => (CompProperties_StatEntry)props;

		public override IEnumerable<StatDrawEntry> SpecialDisplayStats()
		{
			float f = LearningUtility.SchoolDeskLearningRate(parent);
			int num = LearningUtility.ConnectedBlackboards(parent);
			StringBuilder stringBuilder = new StringBuilder(Props.reportText);
			stringBuilder.AppendLine();
			stringBuilder.AppendLine();
			stringBuilder.AppendLine("StatsReport_BaseValue".Translate() + ": " + 1f.ToStringPercent());
			stringBuilder.AppendLine();
			if (num > 0)
			{
				TaggedString taggedString = "StatsReport_Connected".Translate(Find.ActiveLanguageWorker.Pluralize(ThingDefOf.Blackboard.label));
				taggedString += " " + num + " / " + 3 + ": x" + (1f + (float)num * 0.2f).ToStringPercent();
				stringBuilder.AppendLine(taggedString);
			}
			stringBuilder.AppendLine("StatsReport_FinalValue".Translate() + ": " + f.ToStringPercent());
			yield return new StatDrawEntry(Props.statCategoryDef, Props.statLabel, f.ToStringPercent(), stringBuilder.ToString(), Props.displayPriorityInCategory);
		}
	}
}
