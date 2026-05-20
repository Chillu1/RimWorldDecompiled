using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld;

public class RoomContents_Stockpile : RoomContentsWorker
{
	private static readonly List<string> WeaponTags = new List<string> { "IndustrialGunAdvanced", "SpacerGun", "RangedHeavy" };

	public override void FillRoom(Map map, LayoutRoom room, Faction faction, float? threatPoints = null)
	{
		RoomGenUtility.GenerateRows(ThingDefOf.Shelf, room, map, ThingDefOf.Steel);
		if (((map.listerThings.ThingsOfDef(ThingDefOf.AncientHatchExit).FirstOrDefault() as PocketMapExit)?.entrance ?? PocketMapUtility.currentlyGeneratingPortal) is AncientHatch ancientHatch)
		{
			switch (ancientHatch.stockpileType)
			{
			case TileMutatorWorker_Stockpile.StockpileType.Medicine:
				GenerateItems(map, room, ThingDefOf.MedicineUltratech, new IntRange(5, 10));
				GenerateItems(map, room, ThingDefOf.MedicineIndustrial, new IntRange(30, 60));
				break;
			case TileMutatorWorker_Stockpile.StockpileType.Chemfuel:
				GenerateItems(map, room, ThingDefOf.Chemfuel, new IntRange(750, 1200));
				break;
			case TileMutatorWorker_Stockpile.StockpileType.Component:
				GenerateItems(map, room, ThingDefOf.ComponentIndustrial, new IntRange(25, 50));
				GenerateItems(map, room, ThingDefOf.ComponentSpacer, new IntRange(2, 3));
				break;
			case TileMutatorWorker_Stockpile.StockpileType.Weapons:
				GenerateWeapons(map, room, new IntRange(2, 3));
				break;
			case TileMutatorWorker_Stockpile.StockpileType.Gravcore:
				GenerateItems(map, room, ThingDefOf.Gravcore, IntRange.One);
				GenerateItems(map, room, ThingDefOf.GravlitePanel, new IntRange(150, 200));
				break;
			case TileMutatorWorker_Stockpile.StockpileType.Drugs:
				GenerateItems(map, room, ThingDefOf.SmokeleafJoint, new IntRange(5, 20));
				GenerateItems(map, room, ThingDefOf.Flake, new IntRange(25, 100));
				GenerateItems(map, room, ThingDefOf.Yayo, new IntRange(25, 60));
				GenerateItems(map, room, ThingDefOf.GoJuice, new IntRange(-10, 10));
				GenerateItems(map, room, ThingDefOf.WakeUp, new IntRange(-10, 10));
				GenerateItems(map, room, ThingDefOf.Luciferium, new IntRange(5, 15));
				break;
			}
		}
		ThingSetMakerDef thingSetMakerDef = room.defs.FirstOrDefault((LayoutRoomDef x) => x.thingSetMakerDef != null)?.thingSetMakerDef;
		if (thingSetMakerDef != null)
		{
			ThingSetMakerParams parms = new ThingSetMakerParams
			{
				totalMarketValueRange = new FloatRange(2200f)
			};
			List<Thing> items = thingSetMakerDef.root.Generate(parms);
			GenerateItems(map, room, items);
		}
		base.FillRoom(map, room, faction, threatPoints);
	}

	private void GenerateItems(Map map, LayoutRoom room, List<Thing> items)
	{
		int num = 999;
		while (items.Count > 0 && num-- > 0)
		{
			Thing itemToSpawn = items.Last();
			items.Remove(itemToSpawn);
			if (room.TryGetRandomCellInRoom(map, out var cell, 0, 0, (IntVec3 c) => ShelfValidator(map, c, itemToSpawn.def), ignoreBuildings: true))
			{
				GenSpawn.Spawn(itemToSpawn, cell, map).SetForbidden(value: true);
				continue;
			}
			break;
		}
	}

	private void GenerateItems(Map map, LayoutRoom room, ThingDef itemDef, IntRange countRange)
	{
		int num = countRange.RandomInRange;
		int num2 = 99;
		IntVec3 cell;
		while (num > 0 && num2-- > 0 && room.TryGetRandomCellInRoom(map, out cell, 0, 0, (IntVec3 c) => ShelfValidator(map, c, itemDef), ignoreBuildings: true))
		{
			int a = Rand.Range(1, Mathf.Min(countRange.max / 4, itemDef.stackLimit));
			a = Mathf.Min(a, num);
			Thing thing = GenSpawn.Spawn(ThingMaker.MakeThing(itemDef), cell, map);
			thing.stackCount = a;
			thing.SetForbidden(value: true);
			num -= a;
		}
	}

	private void GenerateWeapons(Map map, LayoutRoom room, IntRange countRange)
	{
		int num = countRange.RandomInRange;
		int num2 = 999;
		while (num > 0 && num2-- > 0)
		{
			if (EquipmentUtility.TryGenerateWeaponByTag(WeaponTags.RandomElement(), out var weapon) && !weapon.HasThingCategory(ThingCategoryDefOf.WeaponsUnique))
			{
				if (weapon.TryGetComp(out CompQuality comp))
				{
					comp.SetQuality(QualityUtility.GenerateFromGaussian(1f, QualityCategory.Legendary, QualityCategory.Excellent, QualityCategory.Excellent), ArtGenerationContext.Outsider);
				}
				if (room.TryGetRandomCellInRoom(map, out var cell, 0, 0, (IntVec3 c) => ShelfValidator(map, c, weapon.def), ignoreBuildings: true))
				{
					GenSpawn.Spawn(weapon, cell, map).SetForbidden(value: true);
					num--;
				}
			}
		}
	}

	private bool ShelfValidator(Map map, IntVec3 c, ThingDef itemDef)
	{
		if (!(c.GetFirstThing(map, ThingDefOf.Shelf) is Building_Storage building_Storage))
		{
			return false;
		}
		if (building_Storage.SpaceRemainingFor(itemDef) == 0)
		{
			return false;
		}
		return true;
	}
}
