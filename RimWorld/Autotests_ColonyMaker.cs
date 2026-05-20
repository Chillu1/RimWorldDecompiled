using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld;

public static class Autotests_ColonyMaker
{
	private static CellRect overRect;

	private static BoolGrid usedCells;

	private const int OverRectSize = 100;

	private static Map Map => Find.CurrentMap;

	public static void MakeColony_Full()
	{
		MakeColony(ColonyMakerFlag.ConduitGrid, ColonyMakerFlag.PowerPlants, ColonyMakerFlag.Batteries, ColonyMakerFlag.WorkTables, ColonyMakerFlag.AllBuildings, ColonyMakerFlag.AllItems, ColonyMakerFlag.Filth, ColonyMakerFlag.ColonistsMany, ColonyMakerFlag.ColonistsHungry, ColonyMakerFlag.ColonistsTired, ColonyMakerFlag.ColonistsInjured, ColonyMakerFlag.ColonistsDiseased, ColonyMakerFlag.Beds, ColonyMakerFlag.Stockpiles, ColonyMakerFlag.GrowingZones);
	}

	public static void MakeColony_Animals()
	{
		MakeColony(default(ColonyMakerFlag));
	}

	public static void MakeColony_AncientJunk()
	{
		MakeColony(ColonyMakerFlag.AllAncientJunk);
	}

	public static void MakeColony(params ColonyMakerFlag[] flags)
	{
		bool godMode = DebugSettings.godMode;
		DebugSettings.godMode = true;
		Thing.allowDestroyNonDestroyable = true;
		if (usedCells == null)
		{
			usedCells = new BoolGrid(Map);
		}
		else
		{
			usedCells.ClearAndResizeTo(Map);
		}
		overRect = new CellRect(Map.Center.x - 50, Map.Center.z - 50, 100, 100);
		DeleteAllSpawnedPawns();
		GenDebug.ClearArea(overRect, Find.CurrentMap);
		if (flags.Contains(ColonyMakerFlag.Animals))
		{
			foreach (PawnKindDef item in DefDatabase<PawnKindDef>.AllDefs.Where((PawnKindDef k) => k.RaceProps.Animal))
			{
				if (!TryGetFreeRect(6, 3, out var result))
				{
					return;
				}
				result = result.ContractedBy(1);
				foreach (IntVec3 item2 in result)
				{
					Map.terrainGrid.SetTerrain(item2, TerrainDefOf.Concrete);
				}
				GenSpawn.Spawn(PawnGenerator.GeneratePawn(item), result.Cells.ElementAt(0), Map);
				IntVec3 intVec = result.Cells.ElementAt(1);
				HealthUtility.DamageUntilDead((Pawn)GenSpawn.Spawn(PawnGenerator.GeneratePawn(item), intVec, Map));
				CompRottable compRottable = ((Corpse)intVec.GetThingList(Find.CurrentMap).First((Thing t) => t is Corpse)).TryGetComp<CompRottable>();
				if (compRottable != null)
				{
					compRottable.RotProgress += 1200000f;
				}
				if (item.RaceProps.leatherDef != null)
				{
					GenSpawn.Spawn(item.RaceProps.leatherDef, result.Cells.ElementAt(2), Map);
				}
				if (item.RaceProps.meatDef != null)
				{
					GenSpawn.Spawn(item.RaceProps.meatDef, result.Cells.ElementAt(3), Map);
				}
			}
		}
		if (flags.Contains(ColonyMakerFlag.ConduitGrid))
		{
			Designator_Build designator_Build = new Designator_Build(ThingDefOf.PowerConduit);
			for (int num = overRect.minX; num < overRect.maxX; num++)
			{
				for (int num2 = overRect.minZ; num2 < overRect.maxZ; num2 += 7)
				{
					designator_Build.DesignateSingleCell(new IntVec3(num, 0, num2));
				}
			}
			for (int num3 = overRect.minZ; num3 < overRect.maxZ; num3++)
			{
				for (int num4 = overRect.minX; num4 < overRect.maxX; num4 += 7)
				{
					designator_Build.DesignateSingleCell(new IntVec3(num4, 0, num3));
				}
			}
		}
		if (flags.Contains(ColonyMakerFlag.PowerPlants))
		{
			List<ThingDef> list = new List<ThingDef>
			{
				ThingDefOf.SolarGenerator,
				ThingDefOf.WindTurbine
			};
			for (int num5 = 0; num5 < 8; num5++)
			{
				if (TryMakeBuilding(list[num5 % list.Count]) == null)
				{
					Log.Message("Could not make solar generator.");
					break;
				}
			}
		}
		if (flags.Contains(ColonyMakerFlag.Batteries))
		{
			for (int num6 = 0; num6 < 6; num6++)
			{
				Thing thing = TryMakeBuilding(ThingDefOf.Battery);
				if (thing == null)
				{
					Log.Message("Could not make battery.");
					break;
				}
				((Building_Battery)thing).GetComp<CompPowerBattery>().AddEnergy(999999f);
			}
		}
		if (flags.Contains(ColonyMakerFlag.WorkTables))
		{
			foreach (ThingDef item3 in DefDatabase<ThingDef>.AllDefs.Where((ThingDef def) => typeof(Building_WorkTable).IsAssignableFrom(def.thingClass)))
			{
				Thing thing2 = TryMakeBuilding(item3);
				if (thing2 == null)
				{
					Log.Message("Could not make worktable: " + item3.defName);
					break;
				}
				if (!(thing2 is Building_WorkTable building_WorkTable))
				{
					continue;
				}
				foreach (RecipeDef allRecipe in building_WorkTable.def.AllRecipes)
				{
					building_WorkTable.billStack.AddBill(allRecipe.MakeNewBill());
				}
			}
		}
		if (flags.Contains(ColonyMakerFlag.AllBuildings))
		{
			foreach (ThingDef item4 in DefDatabase<ThingDef>.AllDefs.Where((ThingDef def) => def.category == ThingCategory.Building && def.BuildableByPlayer))
			{
				if (item4 != ThingDefOf.PowerConduit && TryMakeBuilding(item4) == null)
				{
					Log.Message("Could not make building: " + item4.defName);
				}
			}
		}
		if (flags.Contains(ColonyMakerFlag.AllAncientJunk))
		{
			foreach (ThingDef item5 in DefDatabase<ThingDef>.AllDefs.Where((ThingDef def) => def.defName.StartsWith("Ancient")))
			{
				if (TryMakeBuilding(item5) == null)
				{
					Log.Message("Could not make building: " + item5.defName);
				}
			}
		}
		if (!TryGetFreeRect(33, 33, out var result2))
		{
			Log.Error("Could not get wallable rect");
		}
		result2 = result2.ContractedBy(1);
		if (flags.Contains(ColonyMakerFlag.AllItems))
		{
			List<ThingDef> itemDefs = DefDatabase<ThingDef>.AllDefs.Where((ThingDef def) => DebugThingPlaceHelper.IsDebugSpawnable(def) && def.category == ThingCategory.Item).ToList();
			FillWithItems(result2, itemDefs);
		}
		else if (flags.Contains(ColonyMakerFlag.ItemsRawFood))
		{
			List<ThingDef> list2 = new List<ThingDef>();
			list2.Add(ThingDefOf.RawPotatoes);
			FillWithItems(result2, list2);
		}
		if (flags.Contains(ColonyMakerFlag.Filth))
		{
			foreach (IntVec3 item6 in result2)
			{
				GenSpawn.Spawn(ThingDefOf.Filth_Dirt, item6, Map);
			}
		}
		if (flags.Contains(ColonyMakerFlag.ItemsWall))
		{
			CellRect cellRect = result2.ExpandedBy(1);
			Designator_Build designator_Build2 = new Designator_Build(ThingDefOf.Wall);
			designator_Build2.SetStuffDef(ThingDefOf.WoodLog);
			foreach (IntVec3 edgeCell in cellRect.EdgeCells)
			{
				designator_Build2.DesignateSingleCell(edgeCell);
			}
		}
		if (flags.Contains(ColonyMakerFlag.ColonistsMany))
		{
			MakeColonists(15, overRect.CenterCell);
		}
		else if (flags.Contains(ColonyMakerFlag.ColonistOne))
		{
			MakeColonists(1, overRect.CenterCell);
		}
		if (flags.Contains(ColonyMakerFlag.Fire))
		{
			if (!TryGetFreeRect(30, 30, out var result3))
			{
				Log.Error("Could not get free rect for fire.");
			}
			ThingDef plant_TreeOak = ThingDefOf.Plant_TreeOak;
			foreach (IntVec3 item7 in result3)
			{
				GenSpawn.Spawn(plant_TreeOak, item7, Map);
			}
			foreach (IntVec3 item8 in result3)
			{
				if (item8.x % 7 == 0 && item8.z % 7 == 0)
				{
					GenExplosion.DoExplosion(item8, Find.CurrentMap, 3.9f, DamageDefOf.Flame, null);
				}
			}
		}
		if (flags.Contains(ColonyMakerFlag.ColonistsHungry))
		{
			DoToColonists(0.4f, delegate(Pawn col)
			{
				col.needs.food.CurLevel = Mathf.Max(0f, Rand.Range(-0.05f, 0.05f));
			});
		}
		if (flags.Contains(ColonyMakerFlag.ColonistsTired))
		{
			DoToColonists(0.4f, delegate(Pawn col)
			{
				col.needs.rest.CurLevel = Mathf.Max(0f, Rand.Range(-0.05f, 0.05f));
			});
		}
		if (flags.Contains(ColonyMakerFlag.ColonistsInjured))
		{
			DoToColonists(0.4f, delegate(Pawn col)
			{
				DamageDef def = DefDatabase<DamageDef>.AllDefs.Where((DamageDef d) => d.ExternalViolenceFor(null)).RandomElement();
				col.TakeDamage(new DamageInfo(def, 10f));
			});
		}
		if (flags.Contains(ColonyMakerFlag.ColonistsDiseased))
		{
			foreach (HediffDef item9 in DefDatabase<HediffDef>.AllDefs.Where((HediffDef d) => d.hediffClass != typeof(Hediff_AddedPart) && (d.HasComp(typeof(HediffComp_Immunizable)) || d.HasComp(typeof(HediffComp_GrowthMode)))))
			{
				Pawn pawn = PawnGenerator.GeneratePawn(Faction.OfPlayer.def.basicMemberKind, Faction.OfPlayer);
				TryGetFreeRect(1, 1, out var result4);
				GenSpawn.Spawn(pawn, result4.CenterCell, Map);
				pawn.health.AddHediff(item9);
			}
		}
		if (flags.Contains(ColonyMakerFlag.Beds))
		{
			IEnumerable<ThingDef> source = DefDatabase<ThingDef>.AllDefs.Where((ThingDef def) => def.thingClass == typeof(Building_Bed));
			int freeColonistsCount = Map.mapPawns.FreeColonistsCount;
			for (int num7 = 0; num7 < freeColonistsCount; num7++)
			{
				if (TryMakeBuilding(source.RandomElement()) == null)
				{
					Log.Message("Could not make beds.");
					break;
				}
			}
		}
		if (flags.Contains(ColonyMakerFlag.Stockpiles))
		{
			Designator_ZoneAddStockpile_Resources designator_ZoneAddStockpile_Resources = new Designator_ZoneAddStockpile_Resources();
			foreach (StoragePriority value in Enum.GetValues(typeof(StoragePriority)))
			{
				TryGetFreeRect(7, 7, out var result5);
				result5 = result5.ContractedBy(1);
				designator_ZoneAddStockpile_Resources.DesignateMultiCell(result5.Cells);
				((Zone_Stockpile)Map.zoneManager.ZoneAt(result5.CenterCell)).settings.Priority = value;
			}
		}
		if (flags.Contains(ColonyMakerFlag.GrowingZones))
		{
			Zone_Growing dummyZone = new Zone_Growing(Map.zoneManager);
			Map.zoneManager.RegisterZone(dummyZone);
			foreach (ThingDef item10 in DefDatabase<ThingDef>.AllDefs.Where((ThingDef d) => d.plant != null && PlantUtility.CanSowOnGrower(d, dummyZone)))
			{
				if (!TryGetFreeRect(6, 6, out var result6))
				{
					Log.Error("Could not get growing zone rect.");
				}
				result6 = result6.ContractedBy(1);
				foreach (IntVec3 item11 in result6)
				{
					Map.terrainGrid.SetTerrain(item11, TerrainDefOf.Soil);
				}
				new Designator_ZoneAdd_Growing().DesignateMultiCell(result6.Cells);
				if (Map.zoneManager.ZoneAt(result6.CenterCell) is Zone_Growing zone_Growing)
				{
					zone_Growing.SetPlantDefToGrow(item10);
				}
			}
			dummyZone.Delete();
		}
		ClearAllHomeArea();
		FillWithHomeArea(overRect);
		DebugSettings.godMode = godMode;
		Thing.allowDestroyNonDestroyable = false;
	}

	private static void FillWithItems(CellRect rect, List<ThingDef> itemDefs)
	{
		int num = 0;
		foreach (IntVec3 item in rect)
		{
			if (item.x % 6 != 0 && item.z % 6 != 0)
			{
				DebugThingPlaceHelper.DebugSpawn(itemDefs[num], item, -1, direct: true);
				num++;
				if (num >= itemDefs.Count)
				{
					num = 0;
				}
			}
		}
	}

	private static Thing TryMakeBuilding(ThingDef def)
	{
		if (!TryGetFreeRect(def.size.x + 2, def.size.z + 2, out var result))
		{
			return null;
		}
		foreach (IntVec3 item in result)
		{
			Map.terrainGrid.SetTerrain(item, TerrainDefOf.Concrete);
		}
		new Designator_Build(def).DesignateSingleCell(result.CenterCell);
		return result.CenterCell.GetEdifice(Find.CurrentMap);
	}

	private static bool TryGetFreeRect(int width, int height, out CellRect result)
	{
		for (int i = overRect.minZ; i <= overRect.maxZ - height; i++)
		{
			for (int j = overRect.minX; j <= overRect.maxX - width; j++)
			{
				CellRect cellRect = new CellRect(j, i, width, height);
				bool flag = true;
				for (int k = cellRect.minZ; k <= cellRect.maxZ; k++)
				{
					for (int l = cellRect.minX; l <= cellRect.maxX; l++)
					{
						if (usedCells[l, k])
						{
							flag = false;
							break;
						}
					}
					if (!flag)
					{
						break;
					}
				}
				if (!flag)
				{
					continue;
				}
				result = cellRect;
				for (int m = cellRect.minZ; m <= cellRect.maxZ; m++)
				{
					for (int n = cellRect.minX; n <= cellRect.maxX; n++)
					{
						IntVec3 c = new IntVec3(n, 0, m);
						usedCells.Set(c, value: true);
						if (c.GetTerrain(Find.CurrentMap).passability == Traversability.Impassable)
						{
							Map.terrainGrid.SetTerrain(c, TerrainDefOf.Concrete);
						}
					}
				}
				return true;
			}
		}
		result = new CellRect(0, 0, width, height);
		return false;
	}

	private static void DoToColonists(float fraction, Action<Pawn> funcToDo)
	{
		int num = Rand.RangeInclusive(1, Mathf.RoundToInt((float)Map.mapPawns.FreeColonistsCount * fraction));
		int num2 = 0;
		foreach (Pawn item in Map.mapPawns.FreeColonists.InRandomOrder())
		{
			funcToDo(item);
			num2++;
			if (num2 >= num)
			{
				break;
			}
		}
	}

	private static void MakeColonists(int count, IntVec3 center)
	{
		for (int i = 0; i < count; i++)
		{
			TryGetFreeRect(1, 1, out var result);
			Pawn pawn = PawnGenerator.GeneratePawn(Faction.OfPlayer.def.basicMemberKind, Faction.OfPlayer);
			foreach (WorkTypeDef allDef in DefDatabase<WorkTypeDef>.AllDefs)
			{
				if (!pawn.WorkTypeIsDisabled(allDef))
				{
					pawn.workSettings.SetPriority(allDef, 3);
				}
			}
			GenSpawn.Spawn(pawn, result.CenterCell, Map);
		}
	}

	private static void DeleteAllSpawnedPawns()
	{
		foreach (Pawn item in Map.mapPawns.AllPawnsSpawned.ToList())
		{
			item.Destroy();
			item.relations?.ClearAllRelations();
		}
		Find.GameEnder.gameEnding = false;
	}

	private static void ClearAllHomeArea()
	{
		foreach (IntVec3 allCell in Map.AllCells)
		{
			Map.areaManager.Home[allCell] = false;
		}
	}

	private static void FillWithHomeArea(CellRect r)
	{
		new Designator_AreaHomeExpand().DesignateMultiCell(r.Cells);
	}
}
