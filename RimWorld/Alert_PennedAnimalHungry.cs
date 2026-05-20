using System.Collections.Generic;
using RimWorld.Planet;
using Verse;

namespace RimWorld;

public class Alert_PennedAnimalHungry : Alert
{
	private List<GlobalTargetInfo> targets = new List<GlobalTargetInfo>();

	private List<string> pawnNames = new List<string>();

	public const int TicksStarvingThresh = 2500;

	public Alert_PennedAnimalHungry()
	{
		defaultLabel = "AlertPennedAnimalHungry".Translate();
	}

	private void CalculateTargets()
	{
		targets.Clear();
		pawnNames.Clear();
		foreach (Pawn item in PawnsFinder.AllMaps_SpawnedPawnsInFaction(Faction.OfPlayer))
		{
			if (AnimalPenUtility.NeedsToBeManagedByRope(item) && item.needs.food != null && item.needs.food.TicksStarving > 2500 && AnimalPenUtility.GetCurrentPenOf(item, allowUnenclosedPens: false) != null)
			{
				targets.Add(item);
				pawnNames.Add(item.NameShortColored.Resolve());
			}
		}
	}

	public override TaggedString GetExplanation()
	{
		return "AlertPennedAnimalHungryDesc".Translate(pawnNames.ToLineList("  - "));
	}

	public override AlertReport GetReport()
	{
		CalculateTargets();
		return AlertReport.CulpritsAre(targets);
	}
}
