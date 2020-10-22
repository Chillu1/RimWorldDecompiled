using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public static class ThingSetMakerUtility
	{
		public static List<ThingDef> allGeneratableItems = new List<ThingDef>();

		public static void Reset()
		{
			allGeneratableItems.Clear();
			foreach (ThingDef allDef in DefDatabase<ThingDef>.AllDefs)
			{
				if (CanGenerate(allDef))
				{
					allGeneratableItems.Add(allDef);
				}
			}
			ThingSetMaker_Meteorite.Reset();
		}

		public static bool CanGenerate(ThingDef thingDef)
		{
			if ((thingDef.category != ThingCategory.Item && !thingDef.Minifiable) || (thingDef.category == ThingCategory.Item && !thingDef.EverHaulable) || thingDef.isUnfinishedThing || thingDef.IsCorpse || !thingDef.PlayerAcquirable || thingDef.graphicData == null || typeof(MinifiedThing).IsAssignableFrom(thingDef.thingClass))
			{
				return false;
			}
			return true;
		}

		public static IEnumerable<ThingDef> GetAllowedThingDefs(ThingSetMakerParams parms)
		{
			TechLevel techLevel = parms.techLevel ?? TechLevel.Undefined;
			IEnumerable<ThingDef> source = parms.filter.AllowedThingDefs;
			if (techLevel != 0)
			{
				source = source.Where((ThingDef x) => (int)x.techLevel <= (int)techLevel);
			}
			RoyalTitleDef highestTitle = null;
			if (parms.makingFaction != null && parms.makingFaction.def.HasRoyalTitles)
			{
				foreach (Pawn allMapsCaravansAndTravelingTransportPods_Alive_Colonist in PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive_Colonists)
				{
					RoyalTitleDef royalTitleDef = ((allMapsCaravansAndTravelingTransportPods_Alive_Colonist.royalty != null) ? allMapsCaravansAndTravelingTransportPods_Alive_Colonist.royalty.GetCurrentTitle(parms.makingFaction) : null);
					if (royalTitleDef != null && (highestTitle == null || royalTitleDef.seniority > highestTitle.seniority))
					{
						highestTitle = royalTitleDef;
					}
				}
			}
			source = source.Where(delegate(ThingDef x)
			{
				CompProperties_Techprint compProperties = x.GetCompProperties<CompProperties_Techprint>();
				if (compProperties != null)
				{
					if (parms.makingFaction != null && !compProperties.project.heldByFactionCategoryTags.Contains(parms.makingFaction.def.categoryTag))
					{
						return false;
					}
					if (compProperties.project.IsFinished || compProperties.project.TechprintRequirementMet)
					{
						return false;
					}
				}
				if (parms.makingFaction != null && parms.makingFaction.def.HasRoyalTitles)
				{
					RoyalTitleDef minTitleToUse = ThingRequiringRoyalPermissionUtility.GetMinTitleToUse(x, parms.makingFaction);
					if (minTitleToUse != null && (highestTitle == null || highestTitle.seniority < minTitleToUse.seniority))
					{
						return false;
					}
				}
				return true;
			});
			return source.Where((ThingDef x) => CanGenerate(x) && (!parms.maxThingMarketValue.HasValue || x.BaseMarketValue <= parms.maxThingMarketValue) && (parms.validator == null || parms.validator(x)));
		}

		public static void AssignQuality(Thing thing, QualityGenerator? qualityGenerator)
		{
			CompQuality compQuality = thing.TryGetComp<CompQuality>();
			if (compQuality != null)
			{
				QualityCategory q = QualityUtility.GenerateQuality(qualityGenerator ?? QualityGenerator.BaseGen);
				compQuality.SetQuality(q, ArtGenerationContext.Outsider);
			}
		}

		public static bool IsDerpAndDisallowed(ThingDef thing, ThingDef stuff, QualityGenerator? qualityGenerator)
		{
			if (qualityGenerator == QualityGenerator.Gift || qualityGenerator == QualityGenerator.Reward || qualityGenerator == QualityGenerator.Super)
			{
				if (!PawnWeaponGenerator.IsDerpWeapon(thing, stuff))
				{
					return PawnApparelGenerator.IsDerpApparel(thing, stuff);
				}
				return true;
			}
			return false;
		}

		public static float AdjustedBigCategoriesSelectionWeight(ThingDef d, int numMeats, int numLeathers)
		{
			float num = 1f;
			if (d.IsMeat)
			{
				num *= Mathf.Min(5f / (float)numMeats, 1f);
			}
			if (d.IsLeather)
			{
				num *= Mathf.Min(5f / (float)numLeathers, 1f);
			}
			return num;
		}

		public static bool PossibleToWeighNoMoreThan(ThingDef t, float maxMass, IEnumerable<ThingDef> allowedStuff)
		{
			if (maxMass == float.MaxValue || t.category == ThingCategory.Pawn)
			{
				return true;
			}
			if (maxMass < 0f)
			{
				return false;
			}
			if (t.MadeFromStuff)
			{
				foreach (ThingDef item in allowedStuff)
				{
					if (t.GetStatValueAbstract(StatDefOf.Mass, item) <= maxMass)
					{
						return true;
					}
				}
				return false;
			}
			return t.GetStatValueAbstract(StatDefOf.Mass) <= maxMass;
		}

		public static bool TryGetRandomThingWhichCanWeighNoMoreThan(IEnumerable<ThingDef> candidates, TechLevel stuffTechLevel, float maxMass, QualityGenerator? qualityGenerator, out ThingStuffPair thingStuffPair)
		{
			if (!candidates.Where((ThingDef x) => PossibleToWeighNoMoreThan(x, maxMass, GenStuff.AllowedStuffsFor(x, stuffTechLevel))).TryRandomElement(out var thingDef))
			{
				thingStuffPair = default(ThingStuffPair);
				return false;
			}
			ThingDef result;
			if (thingDef.MadeFromStuff)
			{
				if (!(from x in GenStuff.AllowedStuffsFor(thingDef, stuffTechLevel)
					where thingDef.GetStatValueAbstract(StatDefOf.Mass, x) <= maxMass && !IsDerpAndDisallowed(thingDef, x, qualityGenerator)
					select x).TryRandomElementByWeight((ThingDef x) => x.stuffProps.commonality, out result))
				{
					thingStuffPair = default(ThingStuffPair);
					return false;
				}
			}
			else
			{
				result = null;
			}
			thingStuffPair = new ThingStuffPair(thingDef, result);
			return true;
		}

		public static bool PossibleToWeighNoMoreThan(IEnumerable<ThingDef> candidates, TechLevel stuffTechLevel, float maxMass, int count)
		{
			if (maxMass == float.MaxValue || count <= 0)
			{
				return true;
			}
			if (maxMass < 0f)
			{
				return false;
			}
			float num = float.MaxValue;
			foreach (ThingDef candidate in candidates)
			{
				num = Mathf.Min(num, GetMinMass(candidate, stuffTechLevel));
			}
			return num <= maxMass * (float)count;
		}

		public static float GetMinMass(ThingDef thingDef, TechLevel stuffTechLevel)
		{
			float num = float.MaxValue;
			if (thingDef.MadeFromStuff)
			{
				foreach (ThingDef item in GenStuff.AllowedStuffsFor(thingDef, stuffTechLevel))
				{
					if (item.stuffProps.commonality > 0f)
					{
						num = Mathf.Min(num, thingDef.GetStatValueAbstract(StatDefOf.Mass, item));
					}
				}
				return num;
			}
			return Mathf.Min(num, thingDef.GetStatValueAbstract(StatDefOf.Mass));
		}

		public static float GetMinMarketValue(ThingDef thingDef, TechLevel stuffTechLevel)
		{
			float num = float.MaxValue;
			if (thingDef.MadeFromStuff)
			{
				foreach (ThingDef item in GenStuff.AllowedStuffsFor(thingDef, stuffTechLevel))
				{
					if (item.stuffProps.commonality > 0f)
					{
						num = Mathf.Min(num, StatDefOf.MarketValue.Worker.GetValue(StatRequest.For(thingDef, item, QualityCategory.Awful)));
					}
				}
				return num;
			}
			return Mathf.Min(num, StatDefOf.MarketValue.Worker.GetValue(StatRequest.For(thingDef, null, QualityCategory.Awful)));
		}
	}
}
