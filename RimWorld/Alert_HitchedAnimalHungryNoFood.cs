using System.Collections.Generic;
using RimWorld.Planet;
using Verse;

namespace RimWorld;

public class Alert_HitchedAnimalHungryNoFood : Alert
{
	private List<GlobalTargetInfo> targets = new List<GlobalTargetInfo>();

	private List<string> animalNames = new List<string>();

	public Alert_HitchedAnimalHungryNoFood()
	{
		defaultLabel = "AlertHitchedAnimalHungryNoFood".Translate();
	}

	private void CalculateTargets()
	{
		targets.Clear();
		foreach (Pawn item in PawnsFinder.AllMaps_SpawnedPawnsInFaction(Faction.OfPlayer))
		{
			Pawn_RopeTracker roping = item.roping;
			if (roping != null && roping.IsRopedToHitchingPost && item.needs.food.TicksStarving > 2500)
			{
				targets.Add(item);
			}
		}
	}

	public override TaggedString GetExplanation()
	{
		animalNames.Clear();
		foreach (GlobalTargetInfo target in targets)
		{
			animalNames.Add(((Pawn)target.Thing).NameShortColored.Resolve());
		}
		return "AlertHitchedAnimalHungryNoFoodDesc".Translate(animalNames.ToLineList("  - "));
	}

	public override AlertReport GetReport()
	{
		CalculateTargets();
		return AlertReport.CulpritsAre(targets);
	}
}
