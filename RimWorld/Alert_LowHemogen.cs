using System.Collections.Generic;
using RimWorld.Planet;
using Verse;

namespace RimWorld;

public class Alert_LowHemogen : Alert
{
	private List<GlobalTargetInfo> targets = new List<GlobalTargetInfo>();

	private List<string> targetLabels = new List<string>();

	public Alert_LowHemogen()
	{
		requireBiotech = true;
		defaultLabel = "AlertLowHemogen".Translate();
	}

	public override string GetLabel()
	{
		string text = defaultLabel;
		if (targets.Count == 1)
		{
			text = text + ": " + targetLabels[0];
		}
		return text;
	}

	private void CalculateTargets()
	{
		targets.Clear();
		targetLabels.Clear();
		foreach (Pawn item in PawnsFinder.AllMapsCaravansAndTravellingTransporters_AliveSpawned)
		{
			if (item.genes != null && item.RaceProps.Humanlike && item.Faction == Faction.OfPlayer)
			{
				Gene_Hemogen firstGeneOfType = item.genes.GetFirstGeneOfType<Gene_Hemogen>();
				if (firstGeneOfType != null && firstGeneOfType.Value < firstGeneOfType.MinLevelForAlert)
				{
					targets.Add(item);
					targetLabels.Add(item.NameShortColored.Resolve());
				}
			}
		}
	}

	public override TaggedString GetExplanation()
	{
		return "AlertLowHemogenDesc".Translate() + ":\n" + targetLabels.ToLineList("  - ");
	}

	public override AlertReport GetReport()
	{
		CalculateTargets();
		return AlertReport.CulpritsAre(targets);
	}
}
