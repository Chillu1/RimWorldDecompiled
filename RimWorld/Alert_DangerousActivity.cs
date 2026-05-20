using System.Collections.Generic;
using System.Text;
using Verse;

namespace RimWorld;

public class Alert_DangerousActivity : Alert_Critical
{
	private readonly List<Thing> highActivity = new List<Thing>();

	protected override bool DoMessage => false;

	private List<Thing> HighActivity
	{
		get
		{
			highActivity.Clear();
			foreach (Map map in Find.Maps)
			{
				foreach (ThingWithComps item in map.listerThings.ThingsInGroup(ThingRequestGroup.Suppressable))
				{
					if (DangerousActivity(item))
					{
						highActivity.Add(item);
					}
				}
				foreach (Thing item2 in map.listerThings.ThingsInGroup(ThingRequestGroup.EntityHolder))
				{
					if (item2 is Building_HoldingPlatform { Occupied: not false } building_HoldingPlatform && DangerousActivity(building_HoldingPlatform.HeldPawn))
					{
						highActivity.Add(building_HoldingPlatform.HeldPawn);
					}
				}
			}
			return highActivity;
		}
	}

	public Alert_DangerousActivity()
	{
		defaultLabel = "ActivityMultipleDangerous".Translate();
		requireAnomaly = true;
	}

	private static bool DangerousActivity(ThingWithComps thing)
	{
		CompActivity comp = thing.GetComp<CompActivity>();
		if (comp != null && comp.ActivityLevel > comp.Props.warning && comp.State == ActivityState.Passive)
		{
			return true;
		}
		return false;
	}

	public override string GetLabel()
	{
		if (highActivity.Count == 1)
		{
			return highActivity[0].LabelNoParenthesisCap + ": " + highActivity[0].TryGetComp<CompActivity>().ActivityLevel.ToStringPercent("0");
		}
		return defaultLabel;
	}

	public override TaggedString GetExplanation()
	{
		StringBuilder stringBuilder = new StringBuilder();
		foreach (Thing item in highActivity)
		{
			CompActivity compActivity = item.TryGetComp<CompActivity>();
			stringBuilder.AppendLine("  - " + item.LabelNoParenthesisCap + ": " + compActivity.ActivityLevel.ToStringPercent("0"));
		}
		return string.Format("{0}:\n{1}\n{2}", "ActivityDangerousDesc".Translate(), stringBuilder, "ActivityDangerousDescAppended".Translate());
	}

	public override AlertReport GetReport()
	{
		return AlertReport.CulpritsAre(HighActivity);
	}
}
