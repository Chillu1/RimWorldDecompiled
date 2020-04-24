using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld
{
	public static class PawnInventoryGenerator
	{
		public static void GenerateInventoryFor(Pawn p, PawnGenerationRequest request)
		{
			p.inventory.DestroyAll();
			for (int i = 0; i < p.kindDef.fixedInventory.Count; i++)
			{
				ThingDefCountClass thingDefCountClass = p.kindDef.fixedInventory[i];
				Thing thing = ThingMaker.MakeThing(thingDefCountClass.thingDef);
				thing.stackCount = thingDefCountClass.count;
				p.inventory.innerContainer.TryAdd(thing);
			}
			if (p.kindDef.inventoryOptions != null)
			{
				foreach (Thing item in p.kindDef.inventoryOptions.GenerateThings())
				{
					p.inventory.innerContainer.TryAdd(item);
				}
			}
			if (request.AllowFood)
			{
				GiveRandomFood(p);
			}
			GiveDrugsIfAddicted(p);
			GiveCombatEnhancingDrugs(p);
		}

		public static void GiveRandomFood(Pawn p)
		{
			if (p.kindDef.invNutrition > 0.001f)
			{
				ThingDef def;
				if (p.kindDef.invFoodDef != null)
				{
					def = p.kindDef.invFoodDef;
				}
				else
				{
					float value = Rand.Value;
					def = ((value < 0.5f) ? ThingDefOf.MealSimple : ((!((double)value < 0.75)) ? ThingDefOf.MealSurvivalPack : ThingDefOf.MealFine));
				}
				Thing thing = ThingMaker.MakeThing(def);
				thing.stackCount = GenMath.RoundRandom(p.kindDef.invNutrition / thing.GetStatValue(StatDefOf.Nutrition));
				p.inventory.TryAddItemNotForSale(thing);
			}
		}

		private static void GiveDrugsIfAddicted(Pawn p)
		{
			if (p.RaceProps.Humanlike)
			{
				foreach (Hediff_Addiction addiction in p.health.hediffSet.GetHediffs<Hediff_Addiction>())
				{
					if (DefDatabase<ThingDef>.AllDefsListForReading.Where(delegate(ThingDef x)
					{
						if (x.category != ThingCategory.Item)
						{
							return false;
						}
						if (p.Faction != null && (int)x.techLevel > (int)p.Faction.def.techLevel)
						{
							return false;
						}
						CompProperties_Drug compProperties = x.GetCompProperties<CompProperties_Drug>();
						return compProperties != null && compProperties.chemical != null && compProperties.chemical.addictionHediff == addiction.def;
					}).TryRandomElement(out ThingDef result))
					{
						int stackCount = Rand.RangeInclusive(2, 5);
						Thing thing = ThingMaker.MakeThing(result);
						thing.stackCount = stackCount;
						p.inventory.TryAddItemNotForSale(thing);
					}
				}
			}
		}

		private static void GiveCombatEnhancingDrugs(Pawn pawn)
		{
			if (Rand.Value >= pawn.kindDef.combatEnhancingDrugsChance || pawn.IsTeetotaler())
			{
				return;
			}
			for (int i = 0; i < pawn.inventory.innerContainer.Count; i++)
			{
				CompDrug compDrug = pawn.inventory.innerContainer[i].TryGetComp<CompDrug>();
				if (compDrug != null && compDrug.Props.isCombatEnhancingDrug)
				{
					return;
				}
			}
			int randomInRange = pawn.kindDef.combatEnhancingDrugsCount.RandomInRange;
			if (randomInRange <= 0)
			{
				return;
			}
			IEnumerable<ThingDef> source = DefDatabase<ThingDef>.AllDefsListForReading.Where(delegate(ThingDef x)
			{
				if (x.category != ThingCategory.Item)
				{
					return false;
				}
				if (pawn.Faction != null && (int)x.techLevel > (int)pawn.Faction.def.techLevel)
				{
					return false;
				}
				CompProperties_Drug compProperties = x.GetCompProperties<CompProperties_Drug>();
				return (compProperties != null && compProperties.isCombatEnhancingDrug) ? true : false;
			});
			for (int j = 0; j < randomInRange; j++)
			{
				if (!source.TryRandomElement(out ThingDef result))
				{
					break;
				}
				pawn.inventory.innerContainer.TryAdd(ThingMaker.MakeThing(result));
			}
		}
	}
}
