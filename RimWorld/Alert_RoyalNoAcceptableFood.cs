using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;

namespace RimWorld;

public class Alert_RoyalNoAcceptableFood : Alert
{
	private struct CachedResult
	{
		public RoyalTitleFoodRequirement requirement;

		public IntVec3 cell;

		public CachedResult(RoyalTitleFoodRequirement requirement, IntVec3 cell)
		{
			this.requirement = requirement;
			this.cell = cell;
		}
	}

	private readonly List<Pawn> targetsResult = new List<Pawn>();

	private readonly Dictionary<CachedResult, bool> cachedResults = new Dictionary<CachedResult, bool>();

	public List<Pawn> Targets
	{
		get
		{
			targetsResult.Clear();
			List<Map> maps = Find.Maps;
			for (int i = 0; i < maps.Count; i++)
			{
				foreach (Pawn item in maps[i].mapPawns.FreeColonistsSpawned)
				{
					if (!item.Spawned || (item.story != null && item.story.traits.HasTrait(TraitDefOf.Ascetic)))
					{
						continue;
					}
					RoyalTitle royalTitle = item.royalty?.MostSeniorTitle;
					if (royalTitle == null || !royalTitle.conceited || !royalTitle.def.foodRequirement.Defined)
					{
						continue;
					}
					bool flag = false;
					foreach (var (cachedResult2, flag3) in cachedResults)
					{
						if (cachedResult2.requirement.Equals(royalTitle.def.foodRequirement) && item.CanReach(cachedResult2.cell, PathEndMode.ClosestTouch, Danger.Unspecified))
						{
							flag = true;
							if (!flag3)
							{
								targetsResult.Add(item);
							}
							break;
						}
					}
					if (!flag)
					{
						CachedResult key = new CachedResult(royalTitle.def.foodRequirement, item.Position);
						cachedResults[key] = FoodUtility.TryFindBestFoodSourceFor(item, item, desperate: false, out var _, out var _, canRefillDispenser: true, canUseInventory: true, canUsePackAnimalInventory: false, allowForbidden: false, allowCorpse: false, allowSociallyImproper: false, allowHarvest: false, forceScanWholeMap: false, ignoreReservations: true, calculateWantedStackCount: false, allowVenerated: false, FoodPreferability.DesperateOnly);
						if (!cachedResults[key])
						{
							targetsResult.Add(item);
						}
					}
				}
			}
			cachedResults.Clear();
			return targetsResult;
		}
	}

	public Alert_RoyalNoAcceptableFood()
	{
		defaultLabel = "RoyalNoAcceptableFood".Translate();
		defaultExplanation = "RoyalNoAcceptableFoodDesc".Translate();
		requireRoyalty = true;
	}

	public override AlertReport GetReport()
	{
		return AlertReport.CulpritsAre(Targets);
	}

	public override TaggedString GetExplanation()
	{
		return defaultExplanation + "\n" + targetsResult.Select(delegate(Pawn t)
		{
			RoyalTitle mostSeniorTitle = t.royalty.MostSeniorTitle;
			string text = t.LabelShort + " (" + mostSeniorTitle.def.GetLabelFor(t.gender) + "):\n" + mostSeniorTitle.def.SatisfyingMeals(includeDrugs: false).Select((Func<ThingDef, string>)((ThingDef m) => m.LabelCap)).ToLineList("  - ");
			if (ModsConfig.IdeologyActive && t.Ideo != null && t.Ideo.VeneratedAnimals.Any())
			{
				text = text + "\n\n" + "AlertRoyalTitleNoVeneratedAnimalMeat".Translate(t.Named("PAWN"), t.Ideo.Named("IDEO"), t.Ideo.VeneratedAnimals.Select((ThingDef x) => x.label).ToCommaList().Named("ANIMALS")).Resolve();
			}
			return text;
		}).ToLineList("\n");
	}
}
