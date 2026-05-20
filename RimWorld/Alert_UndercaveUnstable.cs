using System.Collections.Generic;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace RimWorld;

public class Alert_UndercaveUnstable : Alert_Critical
{
	private List<GlobalTargetInfo> targets = new List<GlobalTargetInfo>();

	private PitGate PitGate => targets[0].Thing as PitGate;

	protected override Color BGColor
	{
		get
		{
			PitGate pitGate = PitGate;
			if (pitGate != null && pitGate.CollapseStage == 1)
			{
				return Color.clear;
			}
			return base.BGColor;
		}
	}

	public Alert_UndercaveUnstable()
	{
		defaultLabel = "Alert_UndercaveUnstable".Translate();
		defaultExplanation = "Alert_UndercaveUnstableDesc".Translate();
		requireAnomaly = true;
	}

	private void CalculateTargets()
	{
		targets.Clear();
		List<Map> maps = Find.Maps;
		for (int i = 0; i < maps.Count; i++)
		{
			foreach (PitGate item in maps[i].listerThings.ThingsOfDef(ThingDefOf.PitGate))
			{
				if (item.IsCollapsing)
				{
					targets.Add(item);
				}
			}
		}
	}

	public override string GetLabel()
	{
		return defaultLabel + ": " + PitGate.TicksUntilCollapse.ToStringTicksToPeriodVerbose();
	}

	public override AlertReport GetReport()
	{
		CalculateTargets();
		return AlertReport.CulpritsAre(targets);
	}
}
