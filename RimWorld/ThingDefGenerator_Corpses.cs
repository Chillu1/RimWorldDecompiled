using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld;

public static class ThingDefGenerator_Corpses
{
	private const float DaysToStartRot = 2.5f;

	private const float DaysToDessicate = 5f;

	private const float RotDamagePerDay = 2f;

	private const float DessicatedDamagePerDay = 0.7f;

	private const float ButcherProductsMarketValueFactor = 0.6f;

	private const string DeactivateImagePath = "UI/Commands/DestroyUnnaturalCorpse";

	public static IEnumerable<ThingDef> ImpliedCorpseDefs(bool hotReload = false)
	{
		List<ThingDef> postprocess = new List<ThingDef>();
		foreach (ThingDef pawnDef in DefDatabase<ThingDef>.AllDefs.ToList())
		{
			if (pawnDef.category != ThingCategory.Pawn || !pawnDef.race.hasCorpse)
			{
				continue;
			}
			if (pawnDef.race.linkedCorpseKind != null && pawnDef.race.linkedCorpseKind != pawnDef)
			{
				if (pawnDef.race.linkedCorpseKind.race.corpseDef == null)
				{
					postprocess.Add(pawnDef);
				}
				else
				{
					pawnDef.race.corpseDef = pawnDef.race.linkedCorpseKind.race.corpseDef;
				}
				continue;
			}
			ThingDef thingDef = GenerateCorpseDef(pawnDef, hotReload);
			pawnDef.race.corpseDef = thingDef;
			yield return thingDef;
			if (ModsConfig.AnomalyActive && pawnDef.race.hasUnnaturalCorpse)
			{
				yield return GenerateUnnaturalCorpseDef(hotReload, pawnDef);
			}
		}
		int num = 100;
		while (postprocess.Any() && num > 0)
		{
			for (int num2 = postprocess.Count - 1; num2 >= 0; num2--)
			{
				ThingDef thingDef2 = postprocess[num2];
				if (thingDef2.race.linkedCorpseKind.race.corpseDef != null)
				{
					thingDef2.race.corpseDef = thingDef2.race.linkedCorpseKind.race.corpseDef;
					postprocess.RemoveAt(num2);
				}
			}
			num--;
		}
		if (num == 0)
		{
			Log.ErrorOnce("Failed to load linked corpse defs, check for a circular loop in the defs: " + postprocess.Select((ThingDef x) => x.defName).ToCommaList(), 25364571);
		}
	}

	private static ThingDef GenerateUnnaturalCorpseDef(bool hotReload, ThingDef pawnDef)
	{
		ThingDef thingDef = GenerateCorpseDef(pawnDef, hotReload, "UnnaturalCorpse_" + pawnDef.defName);
		thingDef.label = "UnnaturalCorpse".Translate(pawnDef);
		thingDef.thingClass = typeof(UnnaturalCorpse);
		thingDef.SetStatBaseValue(StatDefOf.DeteriorationRate, 0.25f);
		thingDef.tickerType = TickerType.Normal;
		CompProperties_Studiable item = new CompProperties_Studiable
		{
			frequencyTicks = 120000,
			knowledgeCategory = KnowledgeCategoryDefOf.Advanced,
			anomalyKnowledge = 2f,
			minMonolithLevelForStudy = 1,
			studyEnabledByDefault = false,
			showToggleGizmo = true
		};
		thingDef.comps.Add(item);
		thingDef.inspectorTabs.Insert(1, typeof(ITab_StudyNotesUnnaturalCorpse));
		CompProperties_DisableUnnaturalCorpse item2 = new CompProperties_DisableUnnaturalCorpse
		{
			activeTicks = 1,
			ticksToActivate = 180,
			activateTexPath = "UI/Commands/DestroyUnnaturalCorpse",
			activateLabelString = "UnnaturalCorpseDeactivate".Translate(),
			activateDescString = "UnnaturalCorpseDeactivateDesc".Translate(),
			guiLabelString = "UnnaturalCorpseGuiLabel".Translate(),
			jobString = "UnnaturalCorpseJobString".Translate(),
			activatingString = "UnnaturalCorpseActivating".Translate(),
			activatingStringPending = "UnnaturalCorpseActivatingEnroute".Translate(),
			targetingParameters = new TargetingParameters
			{
				canTargetBuildings = false,
				canTargetAnimals = false,
				canTargetMechs = false,
				onlyTargetControlledPawns = true
			}
		};
		thingDef.comps.Add(item2);
		pawnDef.race.unnaturalCorpseDef = thingDef;
		pawnDef.race.corpseDef.virtualDefs.Add(thingDef);
		thingDef.virtualDefParent = pawnDef.race.corpseDef;
		return thingDef;
	}

	private static ThingDef GenerateCorpseDef(ThingDef pawnDef, bool hotReload = false, string customDefName = null)
	{
		string defName = customDefName ?? ("Corpse_" + pawnDef.defName);
		ThingDef thingDef = (hotReload ? (DefDatabase<ThingDef>.GetNamed(defName, errorOnFail: false) ?? new ThingDef()) : new ThingDef());
		thingDef.defName = defName;
		thingDef.label = "CorpseLabel".Translate(pawnDef.label);
		thingDef.description = "CorpseDesc".Translate(pawnDef.label);
		thingDef.soundImpactDefault = pawnDef.soundImpactDefault;
		thingDef.category = ThingCategory.Item;
		thingDef.thingClass = typeof(Corpse);
		thingDef.selectable = true;
		thingDef.tickerType = TickerType.Rare;
		thingDef.altitudeLayer = AltitudeLayer.ItemImportant;
		thingDef.scatterableOnMapGen = false;
		thingDef.alwaysHaulable = true;
		thingDef.soundDrop = SoundDefOf.Corpse_Drop;
		thingDef.pathCost = 14;
		thingDef.socialPropernessMatters = false;
		thingDef.tradeability = Tradeability.None;
		thingDef.messageOnDeteriorateInStorage = false;
		thingDef.modContentPack = pawnDef.modContentPack;
		thingDef.hideStats = pawnDef.hideStats;
		thingDef.hiddenWhileUndiscovered = pawnDef.race.corpseHiddenWhileUndiscovered;
		thingDef.inspectorTabs = new List<Type>
		{
			typeof(ITab_Pawn_Health),
			typeof(ITab_Pawn_Character),
			typeof(ITab_Pawn_Gear),
			typeof(ITab_Pawn_Social),
			typeof(ITab_Pawn_Log)
		};
		thingDef.comps.Add(new CompProperties_Forbiddable());
		if (ModsConfig.BiotechActive)
		{
			thingDef.inspectorTabs.Add(typeof(ITab_Genes));
		}
		thingDef.SetStatBaseValue(StatDefOf.Beauty, -50f);
		thingDef.SetStatBaseValue(StatDefOf.DeteriorationRate, 1f);
		thingDef.SetStatBaseValue(StatDefOf.FoodPoisonChanceFixedHuman, 0.05f);
		thingDef.recipes = new List<RecipeDef>();
		if (!pawnDef.race.IsMechanoid)
		{
			thingDef.recipes.Add(RecipeDefOf.RemoveBodyPart);
		}
		thingDef.SetStatBaseValue(StatDefOf.MarketValue, CalculateMarketValue(pawnDef));
		thingDef.SetStatBaseValue(StatDefOf.Flammability, pawnDef.GetStatValueAbstract(StatDefOf.Flammability));
		thingDef.SetStatBaseValue(StatDefOf.MaxHitPoints, pawnDef.BaseMaxHitPoints);
		thingDef.SetStatBaseValue(StatDefOf.Mass, pawnDef.statBases.GetStatOffsetFromList(StatDefOf.Mass));
		thingDef.SetStatBaseValue(StatDefOf.Nutrition, 5.2f);
		thingDef.ingestible = new IngestibleProperties();
		thingDef.ingestible.parent = thingDef;
		IngestibleProperties ingestible = thingDef.ingestible;
		ingestible.foodType = FoodTypeFlags.Corpse;
		ingestible.sourceDef = pawnDef;
		ingestible.preferability = ((!pawnDef.race.IsFlesh) ? FoodPreferability.NeverForNutrition : FoodPreferability.DesperateOnly);
		DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(ingestible, "tasteThought", ThoughtDefOf.AteCorpse.defName);
		ingestible.maxNumToIngestAtOnce = 1;
		ingestible.defaultNumToIngestAtOnce = 1;
		ingestible.ingestEffect = EffecterDefOf.EatMeat;
		ingestible.ingestSound = SoundDefOf.RawMeat_Eat;
		ingestible.specialThoughtDirect = pawnDef.race.FleshType.ateDirect;
		if (pawnDef.race.IsFlesh)
		{
			thingDef.comps.Add(new CompProperties_Rottable
			{
				daysToRotStart = 2.5f,
				daysToDessicated = 5f,
				rotDamagePerDay = 2f,
				dessicatedDamagePerDay = 0.7f
			});
			thingDef.comps.Add(new CompProperties_SpawnerFilth
			{
				filthDef = ThingDefOf.Filth_CorpseBile,
				spawnCountOnSpawn = 0,
				spawnMtbHours = 0f,
				spawnRadius = 0.1f,
				spawnEveryDays = 1f,
				requiredRotStage = RotStage.Rotting
			});
		}
		if (ModsConfig.AnomalyActive && pawnDef.race.IsFlesh)
		{
			thingDef.comps.Add(new CompProperties
			{
				compClass = typeof(CompHarbingerTreeConsumable)
			});
		}
		thingDef.race = pawnDef.race;
		if (thingDef.thingCategories == null)
		{
			thingDef.thingCategories = new List<ThingCategoryDef>();
		}
		DirectXmlCrossRefLoader.RegisterListWantsCrossRef(thingDef.thingCategories, pawnDef.race.Humanlike ? ThingCategoryDefOf.CorpsesHumanlike.defName : pawnDef.race.FleshType.corpseCategory.defName, thingDef);
		return thingDef;
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
