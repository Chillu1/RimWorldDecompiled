using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld;

public static class PawnInventoryGenerator
{
	private static List<Hediff_Addiction> tmpAddictionHediffs = new List<Hediff_Addiction>();

	public static void GenerateInventoryFor(Pawn p, PawnGenerationRequest request)
	{
		p.inventory.DestroyAll();
		for (int i = 0; i < p.kindDef.fixedInventory.Count; i++)
		{
			ThingDefCountClass thingDefCountClass = p.kindDef.fixedInventory[i];
			Thing thing = ThingMaker.MakeThing(thingDefCountClass.thingDef, thingDefCountClass.stuff);
			thing.stackCount = thingDefCountClass.count;
			if (thing.TryGetComp(out CompQuality comp))
			{
				comp.SetQuality(thingDefCountClass.quality, ArtGenerationContext.Outsider);
			}
			if (thingDefCountClass.color.HasValue && thing.TryGetComp(out CompColorable comp2))
			{
				comp2.SetColor(thingDefCountClass.color.Value);
			}
			if (thing.HasComp<CompEquippable>())
			{
				p.equipment.AddEquipment((ThingWithComps)thing);
			}
			else if (thing is Apparel newApparel)
			{
				p.apparel.Wear(newApparel);
			}
			else
			{
				p.inventory.innerContainer.TryAdd(thing);
			}
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
		if (!p.RaceProps.Humanlike)
		{
			return;
		}
		p.health.hediffSet.GetHediffs(ref tmpAddictionHediffs);
		foreach (Hediff_Addiction addiction in tmpAddictionHediffs)
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
			}).TryRandomElement(out var result))
			{
				int stackCount = Rand.RangeInclusive(2, 5);
				Thing thing = ThingMaker.MakeThing(result);
				thing.stackCount = stackCount;
				p.inventory.TryAddItemNotForSale(thing);
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
		for (int num = 0; num < randomInRange; num++)
		{
			if (!source.TryRandomElement(out var result))
			{
				break;
			}
			pawn.inventory.innerContainer.TryAdd(ThingMaker.MakeThing(result));
		}
	}
}
