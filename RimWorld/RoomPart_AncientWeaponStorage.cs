using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class RoomPart_AncientWeaponStorage : RoomPartWorker
{
	private static readonly List<string> WeaponTags = new List<string> { "IndustrialGunAdvanced", "SimpleGun", "RangedHeavy" };

	private static readonly IntRange WeaponCountRange = new IntRange(6, 12);

	public RoomPart_AncientWeaponStorage(RoomPartDef def)
		: base(def)
	{
	}

	public override void FillRoom(Map map, LayoutRoom room, Faction faction, float threatPoints)
	{
		RoomGenUtility.GenerateRows(ThingDefOf.Shelf, room, map, ThingDefOf.Steel);
		int num = WeaponCountRange.RandomInRange;
		int num2 = 999;
		while (num > 0 && num2-- > 0)
		{
			if (EquipmentUtility.TryGenerateWeaponByTag(WeaponTags.RandomElement(), out var weapon) && !weapon.HasThingCategory(ThingCategoryDefOf.WeaponsUnique))
			{
				if (weapon.TryGetComp(out CompQuality comp))
				{
					comp.SetQuality(QualityUtility.GenerateQuality(QualityGenerator.BaseGen), ArtGenerationContext.Outsider);
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
