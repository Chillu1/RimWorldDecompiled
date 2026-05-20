using System.Collections.Generic;
using RimWorld.Planet;
using Verse;

namespace RimWorld;

public class Alert_AnimalRoaming : Alert
{
	private List<GlobalTargetInfo> targets = new List<GlobalTargetInfo>();

	private List<string> pawnNames = new List<string>();

	public Alert_AnimalRoaming()
	{
		defaultLabel = "AlertAnimalIsRoaming".Translate();
	}

	private void CalculateTargets()
	{
		targets.Clear();
		pawnNames.Clear();
		foreach (Pawn item in PawnsFinder.AllMaps_SpawnedPawnsInFaction(Faction.OfPlayer))
		{
			if (item.MentalStateDef == MentalStateDefOf.Roaming)
			{
				targets.Add(item);
				pawnNames.Add(item.NameShortColored.Resolve());
			}
		}
	}

	public override TaggedString GetExplanation()
	{
		return "AlertAnimalIsRoamingDesc".Translate(pawnNames.ToLineList("  - "));
	}

	public override AlertReport GetReport()
	{
		CalculateTargets();
		return AlertReport.CulpritsAre(targets);
	}
}
