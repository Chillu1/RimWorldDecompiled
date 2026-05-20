using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class Alert_AnimaLinkingReady : Alert
{
	private List<Thing> culprits = new List<Thing>();

	private List<string> targetLabels = new List<string>();

	private List<Pawn> tempPawns = new List<Pawn>();

	public Alert_AnimaLinkingReady()
	{
		defaultLabel = "AnimaLinkingReadyLabel".Translate();
		requireRoyalty = true;
	}

	private void GetTargets()
	{
		culprits.Clear();
		targetLabels.Clear();
		tempPawns.Clear();
		foreach (LordJob_Ritual activeRitual in Find.IdeoManager.GetActiveRituals(Find.CurrentMap))
		{
			if (activeRitual.Ritual != null && activeRitual.Ritual.def == PreceptDefOf.AnimaTreeLinking)
			{
				return;
			}
		}
		foreach (Pawn item in Find.CurrentMap.mapPawns.FreeColonistsSpawned)
		{
			if (item.GetPsylinkLevel() < item.GetMaxPsylinkLevel() && MeditationFocusDefOf.Natural.CanPawnUse(item) && item.psychicEntropy.IsPsychicallySensitive && (!ModsConfig.BiotechActive || !item.DevelopmentalStage.Juvenile()))
			{
				tempPawns.Add(item);
			}
		}
		foreach (Thing item2 in Find.CurrentMap.listerThings.ThingsOfDef(ThingDefOf.Plant_TreeAnima))
		{
			CompPsylinkable compPsylinkable = item2.TryGetComp<CompPsylinkable>();
			int count = compPsylinkable.CompSubplant.SubplantsForReading.Count;
			bool flag = false;
			foreach (Pawn tempPawn in tempPawns)
			{
				if (compPsylinkable.GetRequiredPlantCount(tempPawn) <= count)
				{
					if (!culprits.Contains(tempPawn))
					{
						culprits.Add(tempPawn);
						targetLabels.Add(tempPawn.NameFullColored);
					}
					flag = true;
				}
			}
			if (flag)
			{
				culprits.Add(item2);
			}
		}
	}

	public override TaggedString GetExplanation()
	{
		return "AnimaLinkingReadyDesc".Translate() + ":\n\n" + targetLabels.ToLineList("  - ") + "\n\n" + "AnimaLinkingReadyDescExtra".Translate();
	}

	public override AlertReport GetReport()
	{
		if (Find.CurrentMap == null)
		{
			return AlertReport.Inactive;
		}
		GetTargets();
		return AlertReport.CulpritsAre(culprits);
	}
}
