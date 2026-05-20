using System.Collections.Generic;
using System.Linq;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace RimWorld;

public static class PlayerItemAccessibilityUtility
{
	private static List<Thing> cachedAccessibleThings = new List<Thing>();

	private static List<ThingDefCount> cachedPossiblyAccessibleThings = new List<ThingDefCount>();

	private static HashSet<ThingDef> cachedMakeableItemDefs = new HashSet<ThingDef>();

	private static PlanetTile cachedAccessibleThingsForTile = PlanetTile.Invalid;

	private static int cachedAccessibleThingsForFrame = -1;

	private static List<Thing> tmpThings = new List<Thing>();

	private static HashSet<ThingDef> tmpWorkTables = new HashSet<ThingDef>();

	private const float MaxDistanceInTilesToConsiderAccessible = 5f;

	public static bool Accessible(ThingDef thing, int count, Map map)
	{
		CacheAccessibleThings(map.Tile);
		int num = 0;
		for (int i = 0; i < cachedAccessibleThings.Count; i++)
		{
			if (cachedAccessibleThings[i].def == thing)
			{
				num += cachedAccessibleThings[i].stackCount;
			}
		}
		return num >= count;
	}

	public static bool PossiblyAccessible(ThingDef thing, int count, Map map)
	{
		if (Accessible(thing, count, map))
		{
			return true;
		}
		CacheAccessibleThings(map.Tile);
		int num = 0;
		for (int i = 0; i < cachedPossiblyAccessibleThings.Count; i++)
		{
			if (cachedPossiblyAccessibleThings[i].ThingDef == thing)
			{
				num += cachedPossiblyAccessibleThings[i].Count;
			}
		}
		return num >= count;
	}

	public static bool PlayerCanMake(ThingDef thing, Map map)
	{
		CacheAccessibleThings(map.Tile);
		return cachedMakeableItemDefs.Contains(thing);
	}

	private static void CacheAccessibleThings(PlanetTile nearTile)
	{
		if (nearTile == cachedAccessibleThingsForTile && RealTime.frameCount == cachedAccessibleThingsForFrame)
		{
			return;
		}
		cachedAccessibleThings.Clear();
		cachedPossiblyAccessibleThings.Clear();
		cachedMakeableItemDefs.Clear();
		WorldGrid worldGrid = Find.WorldGrid;
		List<Map> maps = Find.Maps;
		for (int i = 0; i < maps.Count; i++)
		{
			if (maps[i].Tile.Valid && !(worldGrid.ApproxDistanceInTiles(nearTile, maps[i].Tile) > 5f))
			{
				ThingOwnerUtility.GetAllThingsRecursively(maps[i], tmpThings, allowUnreal: false);
				cachedAccessibleThings.AddRange(tmpThings);
			}
		}
		List<Caravan> caravans = Find.WorldObjects.Caravans;
		for (int j = 0; j < caravans.Count; j++)
		{
			if (caravans[j].IsPlayerControlled && !(worldGrid.ApproxDistanceInTiles(nearTile, caravans[j].Tile) > 5f))
			{
				ThingOwnerUtility.GetAllThingsRecursively(caravans[j], tmpThings, allowUnreal: false);
				cachedAccessibleThings.AddRange(tmpThings);
			}
		}
		for (int k = 0; k < cachedAccessibleThings.Count; k++)
		{
			Thing thing = cachedAccessibleThings[k];
			cachedPossiblyAccessibleThings.Add(new ThingDefCount(thing.def, thing.stackCount));
			if (GenLeaving.CanBuildingLeaveResources(thing, DestroyMode.Deconstruct))
			{
				List<ThingDefCountClass> list = thing.CostListAdjusted();
				for (int l = 0; l < list.Count; l++)
				{
					int num = Mathf.RoundToInt((float)list[l].count * thing.def.resourcesFractionWhenDeconstructed);
					if (num > 0)
					{
						cachedPossiblyAccessibleThings.Add(new ThingDefCount(list[l].thingDef, num));
						cachedMakeableItemDefs.Add(list[l].thingDef);
					}
				}
			}
			if (thing is Plant plant && (plant.HarvestableNow || plant.HarvestableSoon))
			{
				int num2 = Mathf.RoundToInt(plant.def.plant.harvestYield * Find.Storyteller.difficulty.cropYieldFactor);
				if (num2 > 0)
				{
					cachedPossiblyAccessibleThings.Add(new ThingDefCount(plant.def.plant.harvestedThingDef, num2));
					cachedMakeableItemDefs.Add(plant.def.plant.harvestedThingDef);
				}
			}
			if (!thing.def.butcherProducts.NullOrEmpty())
			{
				for (int m = 0; m < thing.def.butcherProducts.Count; m++)
				{
					cachedPossiblyAccessibleThings.Add(thing.def.butcherProducts[m]);
					cachedMakeableItemDefs.Add(thing.def.butcherProducts[m].thingDef);
				}
			}
			if (thing is Pawn pawn)
			{
				if (pawn.RaceProps.meatDef != null)
				{
					int num3 = Mathf.RoundToInt(pawn.GetStatValue(StatDefOf.MeatAmount));
					if (num3 > 0)
					{
						cachedPossiblyAccessibleThings.Add(new ThingDefCount(pawn.RaceProps.meatDef, num3));
						cachedMakeableItemDefs.Add(pawn.RaceProps.meatDef);
					}
				}
				if (pawn.RaceProps.leatherDef != null)
				{
					int num4 = GenMath.RoundRandom(pawn.GetStatValue(StatDefOf.LeatherAmount));
					if (num4 > 0)
					{
						cachedPossiblyAccessibleThings.Add(new ThingDefCount(pawn.RaceProps.leatherDef, num4));
						cachedMakeableItemDefs.Add(pawn.RaceProps.leatherDef);
					}
				}
				if (!pawn.RaceProps.Humanlike)
				{
					PawnKindLifeStage curKindLifeStage = pawn.ageTracker.CurKindLifeStage;
					if (curKindLifeStage.butcherBodyPart != null)
					{
						cachedPossiblyAccessibleThings.Add(new ThingDefCount(curKindLifeStage.butcherBodyPart.thing, 1));
						cachedMakeableItemDefs.Add(curKindLifeStage.butcherBodyPart.thing);
					}
				}
			}
			if (thing.def.smeltable)
			{
				List<ThingDefCountClass> list2 = thing.CostListAdjusted();
				for (int n = 0; n < list2.Count; n++)
				{
					if (!list2[n].thingDef.intricate)
					{
						int num5 = Mathf.RoundToInt((float)list2[n].count * 0.25f);
						if (num5 > 0)
						{
							cachedPossiblyAccessibleThings.Add(new ThingDefCount(list2[n].thingDef, num5));
							cachedMakeableItemDefs.Add(list2[n].thingDef);
						}
					}
				}
			}
			if (thing.def.smeltable && !thing.def.smeltProducts.NullOrEmpty())
			{
				for (int num6 = 0; num6 < thing.def.smeltProducts.Count; num6++)
				{
					cachedPossiblyAccessibleThings.Add(thing.def.smeltProducts[num6]);
					cachedMakeableItemDefs.Add(thing.def.smeltProducts[num6].thingDef);
				}
			}
		}
		int num7 = 0;
		for (int num8 = 0; num8 < cachedAccessibleThings.Count; num8++)
		{
			if (cachedAccessibleThings[num8] is Pawn { IsFreeColonist: not false, Dead: false, Downed: false } pawn2 && pawn2.workSettings.WorkIsActive(WorkTypeDefOf.Crafting))
			{
				num7++;
			}
		}
		if (num7 > 0)
		{
			tmpWorkTables.Clear();
			for (int num9 = 0; num9 < cachedAccessibleThings.Count; num9++)
			{
				if (!(cachedAccessibleThings[num9] is Building_WorkTable { Spawned: not false } building_WorkTable) || !tmpWorkTables.Add(building_WorkTable.def))
				{
					continue;
				}
				List<RecipeDef> allRecipes = building_WorkTable.def.AllRecipes;
				for (int num10 = 0; num10 < allRecipes.Count; num10++)
				{
					if (!allRecipes[num10].AvailableNow || !allRecipes[num10].AvailableOnNow(building_WorkTable) || !allRecipes[num10].products.Any() || allRecipes[num10].PotentiallyMissingIngredients(null, building_WorkTable.Map).Any())
					{
						continue;
					}
					ThingDef stuff = (allRecipes[num10].products[0].thingDef.MadeFromStuff ? GenStuff.DefaultStuffFor(allRecipes[num10].products[0].thingDef) : null);
					float num11 = allRecipes[num10].WorkAmountForStuff(stuff);
					if (num11 <= 0f)
					{
						continue;
					}
					int num12 = Mathf.FloorToInt((float)(num7 * 60000 * 5) * 0.09f / num11);
					if (num12 > 0)
					{
						for (int num13 = 0; num13 < allRecipes[num10].products.Count; num13++)
						{
							cachedPossiblyAccessibleThings.Add(new ThingDefCount(allRecipes[num10].products[num13].thingDef, allRecipes[num10].products[num13].count * num12));
							cachedMakeableItemDefs.Add(allRecipes[num10].products[num13].thingDef);
						}
					}
				}
			}
		}
		cachedAccessibleThingsForTile = nearTile;
		cachedAccessibleThingsForFrame = RealTime.frameCount;
	}

	public static bool PlayerOrQuestRewardHas(ThingFilter thingFilter)
	{
		ThingRequest bestThingRequest = thingFilter.BestThingRequest;
		List<Map> maps = Find.Maps;
		for (int i = 0; i < maps.Count; i++)
		{
			List<Thing> list = maps[i].listerThings.ThingsMatching(bestThingRequest);
			for (int j = 0; j < list.Count; j++)
			{
				if (thingFilter.Allows(list[j]))
				{
					return true;
				}
			}
		}
		List<Caravan> caravans = Find.WorldObjects.Caravans;
		for (int k = 0; k < caravans.Count; k++)
		{
			if (!caravans[k].IsPlayerControlled)
			{
				continue;
			}
			List<Thing> list2 = CaravanInventoryUtility.AllInventoryItems(caravans[k]);
			for (int l = 0; l < list2.Count; l++)
			{
				if (thingFilter.Allows(list2[l]))
				{
					return true;
				}
			}
		}
		List<Site> sites = Find.WorldObjects.Sites;
		for (int m = 0; m < sites.Count; m++)
		{
			for (int n = 0; n < sites[m].parts.Count; n++)
			{
				SitePart sitePart = sites[m].parts[n];
				if (sitePart.things == null)
				{
					continue;
				}
				for (int num = 0; num < sitePart.things.Count; num++)
				{
					if (thingFilter.Allows(sitePart.things[num]))
					{
						return true;
					}
				}
			}
			DefeatAllEnemiesQuestComp component = sites[m].GetComponent<DefeatAllEnemiesQuestComp>();
			if (component == null)
			{
				continue;
			}
			ThingOwner rewards = component.rewards;
			for (int num2 = 0; num2 < rewards.Count; num2++)
			{
				if (thingFilter.Allows(rewards[num2]))
				{
					return true;
				}
			}
		}
		return false;
	}

	public static bool PlayerOrQuestRewardHas(ThingDef thingDef, int count = 1)
	{
		if (count <= 0)
		{
			return true;
		}
		int num = 0;
		List<Map> maps = Find.Maps;
		for (int i = 0; i < maps.Count; i++)
		{
			if (count == 1)
			{
				if (maps[i].listerThings.ThingsOfDef(thingDef).Count > 0)
				{
					return true;
				}
				continue;
			}
			num += Mathf.Max(maps[i].resourceCounter.GetCount(thingDef), maps[i].listerThings.ThingsOfDef(thingDef).Count);
			if (num >= count)
			{
				return true;
			}
		}
		List<Caravan> caravans = Find.WorldObjects.Caravans;
		for (int j = 0; j < caravans.Count; j++)
		{
			if (!caravans[j].IsPlayerControlled)
			{
				continue;
			}
			List<Thing> list = CaravanInventoryUtility.AllInventoryItems(caravans[j]);
			for (int k = 0; k < list.Count; k++)
			{
				if (list[k].def == thingDef)
				{
					num += list[k].stackCount;
					if (num >= count)
					{
						return true;
					}
				}
			}
		}
		List<Site> sites = Find.WorldObjects.Sites;
		for (int l = 0; l < sites.Count; l++)
		{
			for (int m = 0; m < sites[l].parts.Count; m++)
			{
				SitePart sitePart = sites[l].parts[m];
				if (sitePart.things == null)
				{
					continue;
				}
				for (int n = 0; n < sitePart.things.Count; n++)
				{
					if (sitePart.things[n].def == thingDef)
					{
						num += sitePart.things[n].stackCount;
						if (num >= count)
						{
							return true;
						}
					}
				}
			}
			DefeatAllEnemiesQuestComp component = sites[l].GetComponent<DefeatAllEnemiesQuestComp>();
			if (component == null)
			{
				continue;
			}
			ThingOwner rewards = component.rewards;
			for (int num2 = 0; num2 < rewards.Count; num2++)
			{
				if (rewards[num2].def == thingDef)
				{
					num += rewards[num2].stackCount;
					if (num >= count)
					{
						return true;
					}
				}
			}
		}
		return false;
	}

	public static bool ItemStashHas(ThingDef thingDef)
	{
		List<Site> sites = Find.WorldObjects.Sites;
		for (int i = 0; i < sites.Count; i++)
		{
			Site site = sites[i];
			for (int j = 0; j < site.parts.Count; j++)
			{
				SitePart sitePart = site.parts[j];
				if (sitePart.things == null)
				{
					continue;
				}
				for (int k = 0; k < sitePart.things.Count; k++)
				{
					if (sitePart.things[k].def == thingDef)
					{
						return true;
					}
				}
			}
		}
		return false;
	}
}
