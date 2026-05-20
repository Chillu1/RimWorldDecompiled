using System.Collections.Generic;
using RimWorld.Planet;
using Verse;

namespace RimWorld;

public class Alert_NeedMeditationSpot : Alert
{
	private List<GlobalTargetInfo> targets = new List<GlobalTargetInfo>();

	private List<string> pawnNames = new List<string>();

	private static Dictionary<Pawn, Building> CachedSpots = new Dictionary<Pawn, Building>();

	private List<GlobalTargetInfo> Targets
	{
		get
		{
			targets.Clear();
			pawnNames.Clear();
			foreach (Pawn item in PawnsFinder.HomeMaps_FreeColonistsSpawned)
			{
				bool flag = false;
				for (int i = 0; i < item.timetable.times.Count; i++)
				{
					if (item.timetable.times[i] == TimeAssignmentDefOf.Meditate)
					{
						flag = true;
						break;
					}
				}
				if ((!item.HasPsylink && !flag) || !item.psychicEntropy.IsPsychicallySensitive)
				{
					continue;
				}
				if (CachedSpots.TryGetValue(item, out var value))
				{
					if (MeditationUtility.IsValidMeditationBuildingForPawn(value, item))
					{
						continue;
					}
					CachedSpots.Remove(item);
				}
				LocalTargetInfo localTargetInfo = MeditationUtility.AllMeditationSpotCandidates(item, allowFallbackSpots: false).FirstOrFallback(LocalTargetInfo.Invalid);
				if (!localTargetInfo.IsValid)
				{
					targets.Add(item);
					pawnNames.Add(item.NameShortColored.Resolve());
				}
				else if (localTargetInfo.Thing is Building value2)
				{
					CachedSpots[item] = value2;
				}
			}
			return targets;
		}
	}

	public Alert_NeedMeditationSpot()
	{
		defaultLabel = "NeedMeditationSpotAlert".Translate();
		requireRoyalty = true;
	}

	public override TaggedString GetExplanation()
	{
		return "NeedMeditationSpotAlertDesc".Translate(pawnNames.ToLineList("  - "));
	}

	public override AlertReport GetReport()
	{
		return AlertReport.CulpritsAre(Targets);
	}

	public static void ClearCache()
	{
		CachedSpots.Clear();
	}
}
