using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld
{
	public class Alert_RoyalNoAcceptableFood : Alert
	{
		private List<Pawn> targetsResult = new List<Pawn>();

		public List<Pawn> Targets
		{
			get
			{
				targetsResult.Clear();
				List<Map> maps = Find.Maps;
				for (int i = 0; i < maps.Count; i++)
				{
					foreach (Pawn freeColonist in maps[i].mapPawns.FreeColonists)
					{
						if (freeColonist.Spawned && (freeColonist.story == null || !freeColonist.story.traits.HasTrait(TraitDefOf.Ascetic)))
						{
							RoyalTitle royalTitle = freeColonist.royalty?.MostSeniorTitle;
							if (royalTitle != null && royalTitle.conceited && royalTitle.def.foodRequirement.Defined && !FoodUtility.TryFindBestFoodSourceFor(freeColonist, freeColonist, desperate: false, out Thing _, out ThingDef _, canRefillDispenser: true, canUseInventory: true, allowForbidden: false, allowCorpse: false, allowSociallyImproper: false, allowHarvest: false, forceScanWholeMap: false, ignoreReservations: true, FoodPreferability.DesperateOnly))
							{
								targetsResult.Add(freeColonist);
							}
						}
					}
				}
				return targetsResult;
			}
		}

		public Alert_RoyalNoAcceptableFood()
		{
			defaultLabel = "RoyalNoAcceptableFood".Translate();
			defaultExplanation = "RoyalNoAcceptableFoodDesc".Translate();
		}

		public override AlertReport GetReport()
		{
			return AlertReport.CulpritsAre(Targets);
		}

		public override TaggedString GetExplanation()
		{
			return defaultExplanation + "\n" + Targets.Select(delegate(Pawn t)
			{
				RoyalTitle mostSeniorTitle = t.royalty.MostSeniorTitle;
				return t.LabelShort + " (" + mostSeniorTitle.def.GetLabelFor(t.gender) + "):\n" + mostSeniorTitle.def.SatisfyingMeals(includeDrugs: false).Select((Func<ThingDef, string>)((ThingDef m) => m.LabelCap)).ToLineList("- ");
			}).ToLineList("\n");
		}
	}
}
