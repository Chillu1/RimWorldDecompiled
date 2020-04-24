using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld.BaseGen
{
	public static class BaseGenUtility
	{
		private static List<IntVec3> bridgeCells = new List<IntVec3>();

		public static ThingDef RandomCheapWallStuff(Faction faction, bool notVeryFlammable = false)
		{
			return RandomCheapWallStuff(faction?.def.techLevel ?? TechLevel.Spacer, notVeryFlammable);
		}

		public static ThingDef RandomCheapWallStuff(TechLevel techLevel, bool notVeryFlammable = false)
		{
			if (techLevel.IsNeolithicOrWorse())
			{
				return ThingDefOf.WoodLog;
			}
			return DefDatabase<ThingDef>.AllDefsListForReading.Where((ThingDef d) => IsCheapWallStuff(d) && (!notVeryFlammable || d.BaseFlammability < 0.5f)).RandomElement();
		}

		public static bool IsCheapWallStuff(ThingDef d)
		{
			if (d.IsStuff && d.stuffProps.CanMake(ThingDefOf.Wall))
			{
				return d.BaseMarketValue / d.VolumePerUnit < 5f;
			}
			return false;
		}

		public static ThingDef RandomHightechWallStuff()
		{
			if (Rand.Value < 0.15f)
			{
				return ThingDefOf.Plasteel;
			}
			return ThingDefOf.Steel;
		}

		public static TerrainDef RandomHightechFloorDef()
		{
			return Rand.Element(TerrainDefOf.Concrete, TerrainDefOf.Concrete, TerrainDefOf.PavedTile, TerrainDefOf.PavedTile, TerrainDefOf.MetalTile);
		}

		public static TerrainDef RandomBasicFloorDef(Faction faction, bool allowCarpet = false)
		{
			if (allowCarpet && (faction == null || !faction.def.techLevel.IsNeolithicOrWorse()) && Rand.Chance(0.1f))
			{
				return DefDatabase<TerrainDef>.AllDefsListForReading.Where((TerrainDef x) => x.IsCarpet).RandomElement();
			}
			return Rand.Element(TerrainDefOf.MetalTile, TerrainDefOf.PavedTile, TerrainDefOf.WoodPlankFloor, TerrainDefOf.TileSandstone);
		}

		public static bool TryRandomInexpensiveFloor(out TerrainDef floor, Predicate<TerrainDef> validator = null)
		{
			Func<TerrainDef, float> costCalculator = delegate(TerrainDef x)
			{
				List<ThingDefCountClass> list = x.CostListAdjusted(null);
				float num2 = 0f;
				for (int i = 0; i < list.Count; i++)
				{
					num2 += (float)list[i].count * list[i].thingDef.BaseMarketValue;
				}
				return num2;
			};
			IEnumerable<TerrainDef> enumerable = DefDatabase<TerrainDef>.AllDefs.Where((TerrainDef x) => x.BuildableByPlayer && x.terrainAffordanceNeeded != TerrainAffordanceDefOf.Bridgeable && !x.IsCarpet && (validator == null || validator(x)) && costCalculator(x) > 0f);
			float cheapest = -1f;
			foreach (TerrainDef item in enumerable)
			{
				float num = costCalculator(item);
				if (cheapest == -1f || num < cheapest)
				{
					cheapest = num;
				}
			}
			return enumerable.Where((TerrainDef x) => costCalculator(x) <= cheapest * 4f).TryRandomElement(out floor);
		}

		public static TerrainDef CorrespondingTerrainDef(ThingDef stuffDef, bool beautiful)
		{
			TerrainDef terrainDef = null;
			List<TerrainDef> allDefsListForReading = DefDatabase<TerrainDef>.AllDefsListForReading;
			for (int i = 0; i < allDefsListForReading.Count; i++)
			{
				if (allDefsListForReading[i].costList == null)
				{
					continue;
				}
				for (int j = 0; j < allDefsListForReading[i].costList.Count; j++)
				{
					if (allDefsListForReading[i].costList[j].thingDef == stuffDef && (terrainDef == null || (beautiful ? (terrainDef.statBases.GetStatOffsetFromList(StatDefOf.Beauty) < allDefsListForReading[i].statBases.GetStatOffsetFromList(StatDefOf.Beauty)) : (terrainDef.statBases.GetStatOffsetFromList(StatDefOf.Beauty) > allDefsListForReading[i].statBases.GetStatOffsetFromList(StatDefOf.Beauty)))))
					{
						terrainDef = allDefsListForReading[i];
					}
				}
			}
			if (terrainDef == null)
			{
				terrainDef = TerrainDefOf.Concrete;
			}
			return terrainDef;
		}

		public static TerrainDef RegionalRockTerrainDef(int tile, bool beautiful)
		{
			ThingDef thingDef = Find.World.NaturalRockTypesIn(tile).RandomElementWithFallback()?.building.mineableThing;
			return CorrespondingTerrainDef((thingDef != null && thingDef.butcherProducts != null && thingDef.butcherProducts.Count > 0) ? thingDef.butcherProducts[0].thingDef : null, beautiful);
		}

		public static bool AnyDoorAdjacentCardinalTo(IntVec3 cell, Map map)
		{
			for (int i = 0; i < 4; i++)
			{
				IntVec3 c = cell + GenAdj.CardinalDirections[i];
				if (c.InBounds(map) && c.GetDoor(map) != null)
				{
					return true;
				}
			}
			return false;
		}

		public static bool AnyDoorAdjacentCardinalTo(CellRect rect, Map map)
		{
			foreach (IntVec3 item in rect.AdjacentCellsCardinal)
			{
				if (item.InBounds(map) && item.GetDoor(map) != null)
				{
					return true;
				}
			}
			return false;
		}

		public static ThingDef WallStuffAt(IntVec3 c, Map map)
		{
			Building edifice = c.GetEdifice(map);
			if (edifice != null && edifice.def == ThingDefOf.Wall)
			{
				return edifice.Stuff;
			}
			return null;
		}

		public static void CheckSpawnBridgeUnder(ThingDef thingDef, IntVec3 c, Rot4 rot)
		{
			if (thingDef.category != ThingCategory.Building)
			{
				return;
			}
			Map map = BaseGen.globalSettings.map;
			CellRect cellRect = GenAdj.OccupiedRect(c, rot, thingDef.size);
			bridgeCells.Clear();
			foreach (IntVec3 item in cellRect)
			{
				if (!item.SupportsStructureType(map, thingDef.terrainAffordanceNeeded) && GenConstruct.CanBuildOnTerrain(TerrainDefOf.Bridge, item, map, Rot4.North))
				{
					bridgeCells.Add(item);
				}
			}
			if (!bridgeCells.Any())
			{
				return;
			}
			if (thingDef.size.x != 1 || thingDef.size.z != 1)
			{
				for (int num = bridgeCells.Count - 1; num >= 0; num--)
				{
					for (int i = 0; i < 8; i++)
					{
						IntVec3 intVec = bridgeCells[num] + GenAdj.AdjacentCells[i];
						if (!bridgeCells.Contains(intVec) && intVec.InBounds(map) && !intVec.SupportsStructureType(map, thingDef.terrainAffordanceNeeded) && GenConstruct.CanBuildOnTerrain(TerrainDefOf.Bridge, intVec, map, Rot4.North))
						{
							bridgeCells.Add(intVec);
						}
					}
				}
			}
			for (int j = 0; j < bridgeCells.Count; j++)
			{
				map.terrainGrid.SetTerrain(bridgeCells[j], TerrainDefOf.Bridge);
			}
		}

		[DebugOutput]
		private static void WallStuffs()
		{
			DebugTables.MakeTablesDialog(GenStuff.AllowedStuffsFor(ThingDefOf.Wall), new TableDataGetter<ThingDef>("defName", (ThingDef d) => d.defName), new TableDataGetter<ThingDef>("cheap", (ThingDef d) => IsCheapWallStuff(d).ToStringCheckBlank()), new TableDataGetter<ThingDef>("floor", (ThingDef d) => CorrespondingTerrainDef(d, beautiful: false).defName), new TableDataGetter<ThingDef>("floor (beautiful)", (ThingDef d) => CorrespondingTerrainDef(d, beautiful: true).defName));
		}
	}
}
