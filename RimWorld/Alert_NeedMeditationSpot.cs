using RimWorld.Planet;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld
{
	public class Alert_NeedMeditationSpot : Alert
	{
		private List<GlobalTargetInfo> targets = new List<GlobalTargetInfo>();

		private List<string> pawnNames = new List<string>();

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
					if ((item.HasPsylink || flag) && !MeditationUtility.AllMeditationSpotCandidates(item, allowFallbackSpots: false).Any())
					{
						targets.Add(item);
						pawnNames.Add(item.LabelShort);
					}
				}
				return targets;
			}
		}

		public Alert_NeedMeditationSpot()
		{
			defaultLabel = "NeedMeditationSpotAlert".Translate();
		}

		public override TaggedString GetExplanation()
		{
			return "NeedMeditationSpotAlertDesc".Translate(pawnNames.ToLineList("  - "));
		}

		public override AlertReport GetReport()
		{
			if (!ModsConfig.RoyaltyActive)
			{
				return false;
			}
			return AlertReport.CulpritsAre(Targets);
		}
	}
}
