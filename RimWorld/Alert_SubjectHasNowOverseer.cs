using System.Collections.Generic;
using System.Linq;
using RimWorld.Planet;
using Verse;

namespace RimWorld;

public class Alert_SubjectHasNowOverseer : Alert
{
	private List<GlobalTargetInfo> targets = new List<GlobalTargetInfo>();

	private List<Pawn> tmpMechsCanGoFeralNow = new List<Pawn>();

	private List<Pawn> tmpMechsCanGoFeralSoon = new List<Pawn>();

	private List<GlobalTargetInfo> Targets
	{
		get
		{
			targets.Clear();
			tmpMechsCanGoFeralNow.Clear();
			tmpMechsCanGoFeralSoon.Clear();
			foreach (Pawn item in PawnsFinder.AllMaps_SpawnedPawnsInFaction(Faction.OfPlayer))
			{
				if (item.IsColonyMechRequiringMechanitor())
				{
					targets.Add(item);
					if (item.OverseerSubject.DelayUntilFeralCheckTicks > 0)
					{
						tmpMechsCanGoFeralSoon.Add(item);
					}
					else
					{
						tmpMechsCanGoFeralNow.Add(item);
					}
				}
			}
			return targets;
		}
	}

	public Alert_SubjectHasNowOverseer()
	{
		defaultLabel = "AlertMechLacksOverseer".Translate();
		requireBiotech = true;
	}

	public override TaggedString GetExplanation()
	{
		TaggedString result = "AlertMechLacksOverseerDesc".Translate(Faction.OfMechanoids);
		if (tmpMechsCanGoFeralSoon.Count > 0)
		{
			result += "\n\n" + "AlertMechLacksOverseerUncontrolled".Translate() + ":\n" + tmpMechsCanGoFeralSoon.Select((Pawn m) => m.LabelCap).ToLineList("  - ");
		}
		if (tmpMechsCanGoFeralNow.Count > 0)
		{
			result += "\n\n" + "AlertMechLacksOverseerMayGoFeral".Translate() + ":\n" + tmpMechsCanGoFeralNow.Select((Pawn m) => m.LabelCap).ToLineList("  - ");
		}
		return result;
	}

	public override AlertReport GetReport()
	{
		return AlertReport.CulpritsAre(Targets);
	}
}
