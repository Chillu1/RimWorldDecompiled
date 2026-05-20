using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld;

public class Alert_InsufficientContainmentStrength : Alert_Critical
{
	private List<Pawn> targets = new List<Pawn>();

	public Alert_InsufficientContainmentStrength()
	{
		defaultLabel = "Alert_InsufficientContainment".Translate();
		requireAnomaly = true;
	}

	public override AlertReport GetReport()
	{
		if (Find.CurrentMap == null)
		{
			return false;
		}
		GetTargets();
		return AlertReport.CulpritsAre(targets);
	}

	public override TaggedString GetExplanation()
	{
		string arg = targets.Select((Pawn x) => x.NameShortColored.Resolve()).ToLineList("  - ", capitalizeItems: true);
		return string.Format("{0}:\n{1}\n\n{2}", "Alert_InsufficientContainmentDesc".Translate(), arg, "Alert_InsufficientContainmentDescAppended".Translate());
	}

	private void GetTargets()
	{
		targets.Clear();
		List<Thing> list = Find.CurrentMap.listerThings.ThingsInGroup(ThingRequestGroup.EntityHolder);
		for (int i = 0; i < list.Count; i++)
		{
			if (list[i] is Building_HoldingPlatform { HeldPawn: not null } building_HoldingPlatform && !building_HoldingPlatform.SafelyContains(building_HoldingPlatform.HeldPawn) && ContainmentUtility.InitiateEscapeMtbDays(building_HoldingPlatform.HeldPawn) > 0f)
			{
				targets.Add(building_HoldingPlatform.HeldPawn);
			}
		}
	}
}
