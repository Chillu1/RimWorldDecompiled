using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public static class ThingDefGenerator_Corpses
	{
		private const float DaysToStartRot = 2.5f;

		private const float DaysToDessicate = 5f;

		private const float RotDamagePerDay = 2f;

		private const float DessicatedDamagePerDay = 0.7f;

		private const float ButcherProductsMarketValueFactor = 0.6f;

		public static IEnumerable<ThingDef> ImpliedCorpseDefs()
		{
			foreach (ThingDef item in DefDatabase<ThingDef>.AllDefs.ToList())
			{
				if (item.category == ThingCategory.Pawn)
				{
					ThingDef thingDef = new ThingDef();
					thingDef.category = ThingCategory.Item;
					thingDef.thingClass = typeof(Corpse);
					thingDef.selectable = true;
					thingDef.tickerType = TickerType.Rare;
					thingDef.altitudeLayer = AltitudeLayer.ItemImportant;
					thingDef.scatterableOnMapGen = false;
					thingDef.SetStatBaseValue(StatDefOf.Beauty, -50f);
					thingDef.SetStatBaseValue(StatDefOf.DeteriorationRate, 1f);
					thingDef.SetStatBaseValue(StatDefOf.FoodPoisonChanceFixedHuman, 0.05f);
					thingDef.alwaysHaulable = true;
					thingDef.soundDrop = SoundDefOf.Corpse_Drop;
					thingDef.pathCost = DefGenerator.StandardItemPathCost;
					thingDef.socialPropernessMatters = false;
					thingDef.tradeability = Tradeability.None;
					thingDef.messageOnDeteriorateInStorage = false;
					thingDef.inspectorTabs = new List<Type>();
					thingDef.inspectorTabs.Add(typeof(ITab_Pawn_Health));
					thingDef.inspectorTabs.Add(typeof(ITab_Pawn_Character));
					thingDef.inspectorTabs.Add(typeof(ITab_Pawn_Gear));
					thingDef.inspectorTabs.Add(typeof(ITab_Pawn_Social));
					thingDef.inspectorTabs.Add(typeof(ITab_Pawn_Log));
					thingDef.comps.Add(new CompProperties_Forbiddable());
					thingDef.recipes = new List<RecipeDef>();
					if (!item.race.IsMechanoid)
					{
						thingDef.recipes.Add(RecipeDefOf.RemoveBodyPart);
					}
					thingDef.defName = "Corpse_" + item.defName;
					thingDef.label = "CorpseLabel".Translate(item.label);
					thingDef.description = "CorpseDesc".Translate(item.label);
					thingDef.soundImpactDefault = item.soundImpactDefault;
					thingDef.SetStatBaseValue(StatDefOf.MarketValue, CalculateMarketValue(item));
					thingDef.SetStatBaseValue(StatDefOf.Flammability, item.GetStatValueAbstract(StatDefOf.Flammability));
					thingDef.SetStatBaseValue(StatDefOf.MaxHitPoints, item.BaseMaxHitPoints);
					thingDef.SetStatBaseValue(StatDefOf.Mass, item.statBases.GetStatOffsetFromList(StatDefOf.Mass));
					thingDef.SetStatBaseValue(StatDefOf.Nutrition, 5.2f);
					thingDef.modContentPack = item.modContentPack;
					thingDef.ingestible = new IngestibleProperties();
					thingDef.ingestible.parent = thingDef;
					IngestibleProperties ingestible = thingDef.ingestible;
					ingestible.foodType = FoodTypeFlags.Corpse;
					ingestible.sourceDef = item;
					ingestible.preferability = ((!item.race.IsFlesh) ? FoodPreferability.NeverForNutrition : FoodPreferability.DesperateOnly);
					DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(ingestible, "tasteThought", ThoughtDefOf.AteCorpse.defName);
					ingestible.maxNumToIngestAtOnce = 1;
					ingestible.ingestEffect = EffecterDefOf.EatMeat;
					ingestible.ingestSound = SoundDefOf.RawMeat_Eat;
					ingestible.specialThoughtDirect = item.race.FleshType.ateDirect;
					if (item.race.IsFlesh)
					{
						CompProperties_Rottable compProperties_Rottable = new CompProperties_Rottable();
						compProperties_Rottable.daysToRotStart = 2.5f;
						compProperties_Rottable.daysToDessicated = 5f;
						compProperties_Rottable.rotDamagePerDay = 2f;
						compProperties_Rottable.dessicatedDamagePerDay = 0.7f;
						thingDef.comps.Add(compProperties_Rottable);
						CompProperties_SpawnerFilth compProperties_SpawnerFilth = new CompProperties_SpawnerFilth();
						compProperties_SpawnerFilth.filthDef = ThingDefOf.Filth_CorpseBile;
						compProperties_SpawnerFilth.spawnCountOnSpawn = 0;
						compProperties_SpawnerFilth.spawnMtbHours = 0f;
						compProperties_SpawnerFilth.spawnRadius = 0.1f;
						compProperties_SpawnerFilth.spawnEveryDays = 1f;
						compProperties_SpawnerFilth.requiredRotStage = RotStage.Rotting;
						thingDef.comps.Add(compProperties_SpawnerFilth);
					}
					if (thingDef.thingCategories == null)
					{
						thingDef.thingCategories = new List<ThingCategoryDef>();
					}
					if (item.race.Humanlike)
					{
						DirectXmlCrossRefLoader.RegisterListWantsCrossRef(thingDef.thingCategories, ThingCategoryDefOf.CorpsesHumanlike.defName, thingDef);
					}
					else
					{
						DirectXmlCrossRefLoader.RegisterListWantsCrossRef(thingDef.thingCategories, item.race.FleshType.corpseCategory.defName, thingDef);
					}
					item.race.corpseDef = thingDef;
					yield return thingDef;
				}
			}
		}

		private static float CalculateMarketValue(ThingDef raceDef)
		{
			float num = 0f;
			if (raceDef.race.meatDef != null)
			{
				int num2 = Mathf.RoundToInt(raceDef.GetStatValueAbstract(StatDefOf.MeatAmount));
				num += (float)num2 * raceDef.race.meatDef.GetStatValueAbstract(StatDefOf.MarketValue);
			}
			if (raceDef.race.leatherDef != null)
			{
				int num3 = Mathf.RoundToInt(raceDef.GetStatValueAbstract(StatDefOf.LeatherAmount));
				num += (float)num3 * raceDef.race.leatherDef.GetStatValueAbstract(StatDefOf.MarketValue);
			}
			if (raceDef.butcherProducts != null)
			{
				for (int i = 0; i < raceDef.butcherProducts.Count; i++)
				{
					num += raceDef.butcherProducts[i].thingDef.BaseMarketValue * (float)raceDef.butcherProducts[i].count;
				}
			}
			return num * 0.6f;
		}
	}
}
