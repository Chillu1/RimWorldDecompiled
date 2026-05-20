using System.Collections.Generic;
using RimWorld.Planet;
using Verse;

namespace RimWorld;

public class Alert_WarqueenHasLowResources : Alert
{
	private List<GlobalTargetInfo> targets = new List<GlobalTargetInfo>();

	public Alert_WarqueenHasLowResources()
	{
		requireBiotech = true;
	}

	private void CalculateTargets()
	{
		targets.Clear();
		foreach (Pawn item in PawnsFinder.AllMaps_SpawnedPawnsInFaction(Faction.OfPlayer))
		{
			if (item.IsColonyMech && item.TryGetComp<CompMechCarrier>(out var comp) && comp.LowIngredientCount)
			{
				targets.Add(item);
			}
		}
	}

	public override string GetLabel()
	{
		return defaultLabel ?? (defaultLabel = "AlertWarqueenHasLowResources".Translate(PawnKindDefOf.Mech_Warqueen.LabelCap));
	}

	public override TaggedString GetExplanation()
	{
		return defaultExplanation ?? (defaultExplanation = "AlertWarqueenHasLowResourcesDesc".Translate(PawnKindDefOf.Mech_Warqueen.labelPlural));
	}

	public override AlertReport GetReport()
	{
		CalculateTargets();
		return AlertReport.CulpritsAre(targets);
	}
}
