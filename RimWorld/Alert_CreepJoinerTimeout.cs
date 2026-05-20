using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Verse;

namespace RimWorld;

public class Alert_CreepJoinerTimeout : Alert
{
	private const int WarningTicks = 30000;

	private readonly List<Pawn> creepjoiners = new List<Pawn>();

	private StringBuilder sb = new StringBuilder();

	private List<Pawn> Creepjoiners
	{
		get
		{
			creepjoiners.Clear();
			foreach (Map map in Find.Maps)
			{
				foreach (Pawn item in map.mapPawns.AllHumanlikeSpawned)
				{
					if (item.IsCreepJoiner && item.creepjoiner.IsOnEntryLord && GenTicks.TicksAbs >= item.creepjoiner.timeoutAt - 30000)
					{
						creepjoiners.Add(item);
					}
				}
			}
			return creepjoiners;
		}
	}

	public Alert_CreepJoinerTimeout()
	{
		defaultPriority = AlertPriority.High;
		requireAnomaly = true;
	}

	public override string GetLabel()
	{
		if (creepjoiners.NullOrEmpty())
		{
			return string.Empty;
		}
		return "CreepJoinerTimeout".Translate();
	}

	public override TaggedString GetExplanation()
	{
		if (creepjoiners.NullOrEmpty())
		{
			return string.Empty;
		}
		sb.Length = 0;
		foreach (Pawn creepjoiner in creepjoiners)
		{
			int numTicks = Mathf.Max(creepjoiner.creepjoiner.timeoutAt - GenTicks.TicksAbs, 0);
			sb.AppendLineTagged("  - " + creepjoiner.LabelShortCap.Colorize(ColoredText.NameColor) + ", " + creepjoiner.creepjoiner.form.label.Colorize(Color.gray) + ": " + numTicks.ToStringTicksToPeriodVerbose());
		}
		return string.Format("{0}:\n\n{1}\n{2}", "CreepJoinerTimeoutDesc".Translate(), sb, "CreepJoinerTimeoutDescAppended".Translate());
	}

	public override AlertReport GetReport()
	{
		return AlertReport.CulpritsAre(Creepjoiners);
	}
}
