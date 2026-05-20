using System.Collections.Generic;
using RimWorld.Planet;
using Verse;

namespace RimWorld;

public class Alert_LowDeathrest : Alert
{
	private List<GlobalTargetInfo> targets = new List<GlobalTargetInfo>();

	private List<string> targetLabels = new List<string>();

	public Alert_LowDeathrest()
	{
		requireBiotech = true;
	}

	public override string GetLabel()
	{
		if (targets.Count == 1)
		{
			return "AlertLowDeathrestPawn".Translate(targetLabels[0].Named("PAWN"));
		}
		return "AlertLowDeathrestPawns".Translate(targetLabels.Count.ToStringCached().Named("NUMCULPRITS"));
	}

	private void CalculateTargets()
	{
		targets.Clear();
		targetLabels.Clear();
		foreach (Pawn item in PawnsFinder.AllMapsCaravansAndTravellingTransporters_AliveSpawned)
		{
			if (item.RaceProps.Humanlike && item.Faction == Faction.OfPlayer && item.needs != null && item.needs.TryGetNeed(out Need_Deathrest need) && need.CurLevel <= 0.1f && !item.Deathresting)
			{
				targets.Add(item);
				targetLabels.Add(item.NameShortColored.Resolve());
			}
		}
	}

	public override TaggedString GetExplanation()
	{
		return "AlertLowDeathrestDesc".Translate(targetLabels.ToLineList("  - ").Named("CULPRITS"));
	}

	public override AlertReport GetReport()
	{
		CalculateTargets();
		return AlertReport.CulpritsAre(targets);
	}
}
