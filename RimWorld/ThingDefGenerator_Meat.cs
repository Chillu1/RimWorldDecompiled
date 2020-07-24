using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld
{
	public static class ThingDefGenerator_Meat
	{
		public static IEnumerable<ThingDef> ImpliedMeatDefs()
		{
			foreach (ThingDef item in DefDatabase<ThingDef>.AllDefs.ToList())
			{
				if (item.category != ThingCategory.Pawn || item.race.useMeatFrom != null)
				{
					continue;
				}
				if (!item.race.IsFlesh)
				{
					DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(item.race, "meatDef", "Steel");
					continue;
				}
				ThingDef thingDef = new ThingDef();
				thingDef.resourceReadoutPriority = ResourceCountPriority.Middle;
				thingDef.category = ThingCategory.Item;
				thingDef.thingClass = typeof(ThingWithComps);
				thingDef.graphicData = new GraphicData();
				thingDef.graphicData.graphicClass = typeof(Graphic_StackCount);
				thingDef.useHitPoints = true;
				thingDef.selectable = true;
				thingDef.SetStatBaseValue(StatDefOf.MaxHitPoints, 100f);
				thingDef.altitudeLayer = AltitudeLayer.Item;
				thingDef.stackLimit = 75;
				thingDef.comps.Add(new CompProperties_Forbiddable());
				CompProperties_Rottable compProperties_Rottable = new CompProperties_Rottable();
				compProperties_Rottable.daysToRotStart = 2f;
				compProperties_Rottable.rotDestroys = true;
				thingDef.comps.Add(compProperties_Rottable);
				thingDef.tickerType = TickerType.Rare;
				thingDef.SetStatBaseValue(StatDefOf.Beauty, -4f);
				thingDef.alwaysHaulable = true;
				thingDef.rotatable = false;
				thingDef.pathCost = 15;
				thingDef.drawGUIOverlay = true;
				thingDef.socialPropernessMatters = true;
				thingDef.modContentPack = item.modContentPack;
				thingDef.category = ThingCategory.Item;
				if (item.race.Humanlike)
				{
					thingDef.description = "MeatHumanDesc".Translate(item.label);
				}
				else if (item.race.FleshType == FleshTypeDefOf.Insectoid)
				{
					thingDef.description = "MeatInsectDesc".Translate(item.label);
				}
				else
				{
					thingDef.description = "MeatDesc".Translate(item.label);
				}
				thingDef.useHitPoints = true;
				thingDef.healthAffectsPrice = false;
				thingDef.SetStatBaseValue(StatDefOf.MaxHitPoints, 60f);
				thingDef.SetStatBaseValue(StatDefOf.DeteriorationRate, 6f);
				thingDef.SetStatBaseValue(StatDefOf.Mass, 0.03f);
				thingDef.SetStatBaseValue(StatDefOf.Flammability, 0.5f);
				thingDef.SetStatBaseValue(StatDefOf.Nutrition, 0.05f);
				thingDef.SetStatBaseValue(StatDefOf.FoodPoisonChanceFixedHuman, 0.02f);
				thingDef.BaseMarketValue = item.race.meatMarketValue;
				if (thingDef.thingCategories == null)
				{
					thingDef.thingCategories = new List<ThingCategoryDef>();
				}
				DirectXmlCrossRefLoader.RegisterListWantsCrossRef(thingDef.thingCategories, "MeatRaw", thingDef);
				thingDef.ingestible = new IngestibleProperties();
				thingDef.ingestible.parent = thingDef;
				thingDef.ingestible.foodType = FoodTypeFlags.Meat;
				thingDef.ingestible.preferability = FoodPreferability.RawBad;
				DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(thingDef.ingestible, "tasteThought", ThoughtDefOf.AteRawFood.defName);
				thingDef.ingestible.ingestEffect = EffecterDefOf.EatMeat;
				thingDef.ingestible.ingestSound = SoundDefOf.RawMeat_Eat;
				thingDef.ingestible.specialThoughtDirect = item.race.FleshType.ateDirect;
				thingDef.ingestible.specialThoughtAsIngredient = item.race.FleshType.ateAsIngredient;
				thingDef.graphicData.color = item.race.meatColor;
				if (item.race.Humanlike)
				{
					thingDef.graphicData.texPath = "Things/Item/Resource/MeatFoodRaw/Meat_Human";
				}
				else if (item.race.FleshType == FleshTypeDefOf.Insectoid)
				{
					thingDef.graphicData.texPath = "Things/Item/Resource/MeatFoodRaw/Meat_Insect";
				}
				else if (item.race.baseBodySize < 0.7f)
				{
					thingDef.graphicData.texPath = "Things/Item/Resource/MeatFoodRaw/Meat_Small";
				}
				else
				{
					thingDef.graphicData.texPath = "Things/Item/Resource/MeatFoodRaw/Meat_Big";
				}
				thingDef.defName = "Meat_" + item.defName;
				if (item.race.meatLabel.NullOrEmpty())
				{
					thingDef.label = "MeatLabel".Translate(item.label);
				}
				else
				{
					thingDef.label = item.race.meatLabel;
				}
				thingDef.ingestible.sourceDef = item;
				item.race.meatDef = thingDef;
				yield return thingDef;
			}
		}
	}
}
