using System.Collections.Generic;
using RimWorld.Planet;
using Verse;

namespace RimWorld;

public class Alert_UnusableMeditationFocus : Alert
{
	public class Alert_PermitAvailable : Alert
	{
		private List<GlobalTargetInfo> targets = new List<GlobalTargetInfo>();

		private Pawn target;

		public Alert_PermitAvailable()
		{
			defaultLabel = "PermitChoiceReadyAlert".Translate();
			requireRoyalty = true;
		}

		private void CalculateTargets()
		{
			targets.Clear();
			target = null;
			foreach (Pawn item in PawnsFinder.HomeMaps_FreeColonistsSpawned)
			{
				if (item.royalty != null && item.royalty.PermitPointsAvailable)
				{
					if (target == null)
					{
						target = item;
					}
					targets.Add(item);
				}
			}
		}

		public override AlertReport GetReport()
		{
			CalculateTargets();
			return AlertReport.CulpritsAre(targets);
		}

		public override TaggedString GetExplanation()
		{
			return "PermitChoiceReadyAlertDesc".Translate(target.Named("PAWN"));
		}

		protected override void OnClick()
		{
			base.OnClick();
			target?.royalty?.OpenPermitWindow();
		}
	}

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
				if (item.timetable == null || item.timetable.CurrentAssignment != TimeAssignmentDefOf.Meditate || !item.psychicEntropy.IsCurrentlyMeditating || MeditationFocusDefOf.Natural.CanPawnUse(item) || !(item.jobs.curDriver is JobDriver_Meditate jobDriver_Meditate) || jobDriver_Meditate.Focus != null || jobDriver_Meditate is JobDriver_Reign)
				{
					continue;
				}
				foreach (Thing item2 in GenRadial.RadialDistinctThingsAround(item.Position, item.Map, MeditationUtility.FocusObjectSearchRadius, useCenter: false))
				{
					if (item2.def == ThingDefOf.Plant_TreeAnima || item2.def == ThingDefOf.AnimusStone || item2.def == ThingDefOf.NatureShrine_Small || item2.def == ThingDefOf.NatureShrine_Large)
					{
						targets.Add(item);
						pawnEntries.Add(item.NameShortColored.Resolve() + " (" + item2.LabelShort + ")");
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
		requireRoyalty = true;
	}

	public override TaggedString GetExplanation()
	{
		return "UnusableMeditationFocusAlertDesc".Translate(pawnEntries.ToLineList("  - "));
	}

	public override AlertReport GetReport()
	{
		return AlertReport.CulpritsAre(Targets);
	}
}
