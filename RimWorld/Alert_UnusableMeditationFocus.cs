using RimWorld.Planet;
using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class Alert_UnusableMeditationFocus : Alert
	{
		private List<GlobalTargetInfo> targets = new List<GlobalTargetInfo>();

		private List<string> pawnEntries = new List<string>();

		private List<GlobalTargetInfo> Targets
		{
			get
			{
				targets.Clear();
				pawnEntries.Clear();
				foreach (Pawn item in PawnsFinder.HomeMaps_FreeColonistsSpawned)
				{
					if (item.timetable == null || item.timetable.CurrentAssignment != TimeAssignmentDefOf.Meditate || !item.psychicEntropy.IsCurrentlyMeditating || MeditationFocusDefOf.Natural.CanPawnUse(item))
					{
						continue;
					}
					JobDriver_Meditate jobDriver_Meditate = item.jobs.curDriver as JobDriver_Meditate;
					if (jobDriver_Meditate == null || jobDriver_Meditate.Focus != null || jobDriver_Meditate is JobDriver_Reign)
					{
						continue;
					}
					foreach (Thing item2 in GenRadial.RadialDistinctThingsAround(item.Position, item.Map, MeditationUtility.FocusObjectSearchRadius, useCenter: false))
					{
						if (item2.def == ThingDefOf.Plant_TreeAnima || item2.def == ThingDefOf.AnimusStone || item2.def == ThingDefOf.NatureShrine_Small || item2.def == ThingDefOf.NatureShrine_Large)
						{
							targets.Add(item);
							pawnEntries.Add(item.LabelShort + " (" + item2.LabelShort + ")");
							break;
						}
					}
				}
				return targets;
			}
		}

		public Alert_UnusableMeditationFocus()
		{
			defaultLabel = "UnusableMeditationFocusAlert".Translate();
		}

		public override TaggedString GetExplanation()
		{
			return "UnusableMeditationFocusAlertDesc".Translate(pawnEntries.ToLineList("  - "));
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
