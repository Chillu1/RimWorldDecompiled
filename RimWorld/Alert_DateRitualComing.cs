using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld;

public class Alert_DateRitualComing : Alert
{
	private const float TicksBeforeWarning = 180000f;

	private List<string> ritualEntries = new List<string>();

	public Alert_DateRitualComing()
	{
		defaultLabel = "AlertRitualComing".Translate();
		requireIdeology = true;
	}

	private void UpcomingRituals()
	{
		ritualEntries.Clear();
		foreach (Ideo allIdeo in Faction.OfPlayer.ideos.AllIdeos)
		{
			foreach (Precept_Ritual item in allIdeo.PreceptsListForReading.OfType<Precept_Ritual>())
			{
				if (item.isAnytime)
				{
					continue;
				}
				RitualObligationTrigger ritualObligationTrigger = item.obligationTriggers.FirstOrDefault((RitualObligationTrigger o) => o is RitualObligationTrigger_Date);
				if (ritualObligationTrigger != null)
				{
					RitualObligationTrigger_Date ritualObligationTrigger_Date = (RitualObligationTrigger_Date)ritualObligationTrigger;
					int num = ritualObligationTrigger_Date.OccursOnTick();
					int num2 = ritualObligationTrigger_Date.CurrentTickRelative();
					if ((float)(num - num2) < 180000f && num2 < num)
					{
						ritualEntries.Add("AlertRitualComingEntry".Translate(item.LabelCap, ritualObligationTrigger_Date.DateString, (num - num2).ToStringTicksToPeriodVerbose()));
					}
				}
			}
		}
	}

	public override TaggedString GetExplanation()
	{
		return "AlertRitualComingDesc".Translate() + ":\n" + ritualEntries.ToLineList("  - ") + "\n\n" + "AlertRitualComingExtra".Translate(RitualObligation.DaysToExpire).Resolve();
	}

	public override AlertReport GetReport()
	{
		UpcomingRituals();
		return ritualEntries.Count > 0;
	}
}
