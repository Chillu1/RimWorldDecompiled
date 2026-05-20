using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;

namespace Verse;

public static class GenTemperature
{
	public static readonly Color ColorSpotHot = new Color(1f, 0f, 0f, 0.6f);

	public static readonly Color ColorSpotCold = new Color(0f, 0f, 1f, 0.6f);

	public static readonly Color ColorRoomHot = new Color(1f, 0f, 0f, 0.3f);

	public static readonly Color ColorRoomCold = new Color(0f, 0f, 1f, 0.3f);

	private static readonly List<Room> neighRooms = new List<Room>();

	private static readonly HashSet<Room> beqRooms = new HashSet<Room>();

	private const float VacuumTemperatureEqualizationFactor = 0.1f;

	public static float AverageTemperatureAtTileForTwelfth(PlanetTile tile, Twelfth twelfth)
	{
		int num = 30000;
		int num2 = 300000 * (int)twelfth;
		float num3 = 0f;
		for (int i = 0; i < 120; i++)
		{
			int absTick = num2 + num + Mathf.RoundToInt((float)i / 120f * 300000f);
			num3 += GetTemperatureFromSeasonAtTile(absTick, tile);
		}
		return num3 / 120f;
	}

	public static float MinTemperatureAtTile(PlanetTile tile)
	{
		float num = float.MaxValue;
		for (int i = 0; i < 3600000; i += 27000)
		{
			num = Mathf.Min(num, GetTemperatureFromSeasonAtTile(i, tile));
		}
		return num;
	}

	public static float MaxTemperatureAtTile(PlanetTile tile)
	{
		float num = float.MinValue;
		for (int i = 0; i < 3600000; i += 27000)
		{
			num = Mathf.Max(num, GetTemperatureFromSeasonAtTile(i, tile));
		}
		return num;
	}

	public static FloatRange ComfortableTemperatureRange(this Pawn p)
	{
		return new FloatRange(p.GetStatValue(StatDefOf.ComfyTemperatureMin, applyPostProcess: true, 1), p.GetStatValue(StatDefOf.ComfyTemperatureMax, applyPostProcess: true, 1));
	}

	public static FloatRange ComfortableTemperatureRange(this Pawn p, List<ThingStuffPair> apparel)
	{
		float num = p.GetStatValue(StatDefOf.ComfyTemperatureMin, applyPostProcess: true, 1);
		float num2 = p.GetStatValue(StatDefOf.ComfyTemperatureMax, applyPostProcess: true, 1);
		if (apparel != null)
		{
			num -= apparel.Sum((ThingStuffPair x) => x.InsulationCold);
			num2 += apparel.Sum((ThingStuffPair x) => x.InsulationHeat);
		}
		return new FloatRange(num, num2);
	}

	public static FloatRange ComfortableTemperatureRange(ThingDef raceDef, List<ThingStuffPair> apparel = null)
	{
		FloatRange result = new FloatRange(raceDef.GetStatValueAbstract(StatDefOf.ComfyTemperatureMin), raceDef.GetStatValueAbstract(StatDefOf.ComfyTemperatureMax));
		if (apparel != null)
		{
			result.min -= apparel.Sum((ThingStuffPair x) => x.InsulationCold);
			result.max += apparel.Sum((ThingStuffPair x) => x.InsulationHeat);
		}
		return result;
	}

	public static FloatRange SafeTemperatureRange(this Pawn p)
	{
		FloatRange result = p.ComfortableTemperatureRange();
		result.min -= 10f;
		result.max += 10f;
		return result;
	}

	public static FloatRange SafeTemperatureRange(this Pawn p, List<ThingStuffPair> apparel)
	{
		FloatRange result = p.ComfortableTemperatureRange(apparel);
		result.min -= 10f;
		result.max += 10f;
		return result;
	}

	public static bool SafeTemperatureAtCell(this Pawn p, IntVec3 cell, Map map)
	{
		return p.SafeTemperatureRange().Includes(GetTemperatureForCell(cell, map));
	}

	public static bool ComfortableTemperatureAtCell(this Pawn p, IntVec3 cell, Map map)
	{
		return p.ComfortableTemperatureRange().Includes(GetTemperatureForCell(cell, map));
	}

	public static FloatRange SafeTemperatureRange(ThingDef raceDef, List<ThingStuffPair> apparel = null)
	{
		FloatRange result = ComfortableTemperatureRange(raceDef, apparel);
		result.min -= 10f;
		result.max += 10f;
		return result;
	}

	public static float GetTemperatureForCell(IntVec3 c, Map map)
	{
		TryGetTemperatureForCell(c, map, out var tempResult);
		return tempResult;
	}

	public static bool TryGetTemperatureForCell(IntVec3 c, Map map, out float tempResult)
	{
		if (map == null)
		{
			Log.Error("Got temperature for null map.");
			tempResult = 21f;
			return true;
		}
		if (!c.InBounds(map))
		{
			tempResult = 21f;
			return false;
		}
		if (TryGetDirectAirTemperatureForCell(c, map, out tempResult))
		{
			return true;
		}
		List<Thing> list = map.thingGrid.ThingsListAtFast(c);
		for (int i = 0; i < list.Count; i++)
		{
			if (list[i].def.passability == Traversability.Impassable)
			{
				return TryGetAirTemperatureAroundThing(list[i], out tempResult);
			}
		}
		return false;
	}

	public static bool TryGetDirectAirTemperatureForCell(IntVec3 c, Map map, out float temperature)
	{
		if (!c.InBounds(map))
		{
			temperature = 21f;
			return false;
		}
		Room room = c.GetRoom(map);
		if (room == null)
		{
			temperature = 21f;
			return false;
		}
		temperature = room.Temperature;
		return true;
	}

	public static bool TryGetAirTemperatureAroundThing(Thing t, out float temperature)
	{
		float num = 0f;
		int num2 = 0;
		List<IntVec3> list = GenAdjFast.AdjacentCells8Way(t);
		for (int i = 0; i < list.Count; i++)
		{
			if (list[i].InBounds(t.Map) && TryGetDirectAirTemperatureForCell(list[i], t.Map, out var temperature2))
			{
				num += temperature2;
				num2++;
			}
		}
		if (num2 > 0)
		{
			temperature = num / (float)num2;
			return true;
		}
		temperature = 21f;
		return false;
	}

	public static float OffsetFromSunCycle(int absTick, PlanetTile tile)
	{
		if (!tile.Valid)
		{
			return 0f;
		}
		float num = GenDate.DayPercent(absTick, Find.WorldGrid.LongLatOf(tile).x);
		return Mathf.Cos(MathF.PI * 2f * (num + 0.32f)) * 7f;
	}

	public static float OffsetFromSeasonCycle(int absTick, PlanetTile tile)
	{
		float num = (float)absTick / 60000f % 60f / 60f;
		return Mathf.Cos(MathF.PI * 2f * (num - Season.Winter.GetMiddleTwelfth(0f).GetBeginningYearPct())) * (0f - SeasonalShiftAmplitudeAt(tile));
	}

	public static float GetTemperatureFromSeasonAtTile(int absTick, PlanetTile tile)
	{
		if (absTick == 0)
		{
			absTick = 1;
		}
		Tile tile2 = Find.WorldGrid[tile];
		if (tile2 == null)
		{
			return 21f;
		}
		return tile2.temperature + OffsetFromSeasonCycle(absTick, tile);
	}

	public static float GetTemperatureAtTile(PlanetTile tile)
	{
		return Current.Game.FindMap(tile)?.mapTemperature.OutdoorTemp ?? GetTemperatureFromSeasonAtTile(GenTicks.TicksAbs, tile);
	}

	public static float SeasonalShiftAmplitudeAt(PlanetTile tile)
	{
		if (!tile.Valid)
		{
			return 0f;
		}
		if (Find.WorldGrid.LongLatOf(tile).y >= 0f)
		{
			return TemperatureTuning.SeasonalTempVariationCurve.Evaluate(Find.WorldGrid.DistanceFromEquatorNormalized(tile));
		}
		return 0f - TemperatureTuning.SeasonalTempVariationCurve.Evaluate(Find.WorldGrid.DistanceFromEquatorNormalized(tile));
	}

	public static List<Twelfth> TwelfthsInAverageTemperatureRange(PlanetTile tile, float minTemp, float maxTemp)
	{
		List<Twelfth> twelfths = new List<Twelfth>();
		for (int i = 0; i < 12; i++)
		{
			float num = AverageTemperatureAtTileForTwelfth(tile, (Twelfth)i);
			if (num >= minTemp && num <= maxTemp)
			{
				twelfths.Add((Twelfth)i);
			}
		}
		if (twelfths.Count <= 1 || twelfths.Count == 12)
		{
			return twelfths;
		}
		if (twelfths.Contains(Twelfth.Twelfth) && twelfths.Contains(Twelfth.First))
		{
			Twelfth num2 = twelfths.First((Twelfth m) => !twelfths.Contains(m - 1));
			List<Twelfth> list = new List<Twelfth>();
			for (int num3 = (int)num2; num3 < 12 && twelfths.Contains((Twelfth)num3); num3++)
			{
				list.Add((Twelfth)num3);
			}
			for (int num4 = 0; num4 < 12 && twelfths.Contains((Twelfth)num4); num4++)
			{
				list.Add((Twelfth)num4);
			}
		}
		return twelfths;
	}

	public static Twelfth EarliestTwelfthInAverageTemperatureRange(PlanetTile tile, float minTemp, float maxTemp)
	{
		for (int i = 0; i < 12; i++)
		{
			float num = AverageTemperatureAtTileForTwelfth(tile, (Twelfth)i);
			if (!(num >= minTemp) || !(num <= maxTemp))
			{
				continue;
			}
			if (i != 0)
			{
				return (Twelfth)i;
			}
			Twelfth twelfth = (Twelfth)i;
			for (int j = 0; j < 12; j++)
			{
				float num2 = AverageTemperatureAtTileForTwelfth(tile, twelfth.PreviousTwelfth());
				if (num2 < minTemp || num2 > maxTemp)
				{
					return twelfth;
				}
				twelfth = twelfth.PreviousTwelfth();
			}
			return (Twelfth)i;
		}
		return Twelfth.Undefined;
	}

	public static bool PushHeat(IntVec3 c, Map map, float energy)
	{
		if (map == null)
		{
			Log.Error("Added heat to null map.");
			return false;
		}
		Room room = c.GetRoom(map);
		if (room != null)
		{
			return room.PushHeat(energy);
		}
		neighRooms.Clear();
		for (int i = 0; i < 8; i++)
		{
			IntVec3 intVec = c + GenAdj.AdjacentCells[i];
			if (intVec.InBounds(map))
			{
				room = intVec.GetRoom(map);
				if (room != null)
				{
					neighRooms.Add(room);
				}
			}
		}
		float energy2 = energy / (float)neighRooms.Count;
		for (int j = 0; j < neighRooms.Count; j++)
		{
			neighRooms[j].PushHeat(energy2);
		}
		bool result = neighRooms.Count > 0;
		neighRooms.Clear();
		return result;
	}

	public static void PushHeat(Thing t, float energy)
	{
		IntVec3 result;
		if (t.GetRoom() != null)
		{
			PushHeat(t.Position, t.Map, energy);
		}
		else if (GenAdj.TryFindRandomAdjacentCell8WayWithRoom(t, out result))
		{
			PushHeat(result, t.Map, energy);
		}
	}

	public static float ControlTemperatureTempChange(IntVec3 cell, Map map, float energyLimit, float targetTemperature)
	{
		Room room = cell.GetRoom(map);
		if (room == null || room.UsesOutdoorTemperature)
		{
			return 0f;
		}
		float b = energyLimit / (float)room.CellCount;
		float a = targetTemperature - room.Temperature;
		float num = 0f;
		if (energyLimit > 0f)
		{
			num = Mathf.Min(a, b);
			return Mathf.Max(num, 0f);
		}
		num = Mathf.Max(a, b);
		return Mathf.Min(num, 0f);
	}

	public static void EqualizeTemperaturesThroughBuilding(Building b, float rate, bool twoWay)
	{
		beqRooms.Clear();
		float num = 0f;
		foreach (IntVec3 item in b.OccupiedRect())
		{
			if (twoWay)
			{
				for (int i = 0; i < 2; i++)
				{
					IntVec3 intVec = ((i == 0) ? (item + b.Rotation.FacingCell) : (item - b.Rotation.FacingCell));
					if (intVec.InBounds(b.Map))
					{
						Room room = intVec.GetRoom(b.Map);
						if (room != null && beqRooms.Add(room))
						{
							num += room.Temperature;
						}
					}
				}
				continue;
			}
			for (int j = 0; j < 4; j++)
			{
				IntVec3 intVec2 = item + GenAdj.CardinalDirections[j];
				if (intVec2.InBounds(b.Map))
				{
					Room room2 = intVec2.GetRoom(b.Map);
					if (room2 != null && beqRooms.Add(room2))
					{
						num += room2.Temperature;
					}
				}
			}
		}
		int count = beqRooms.Count;
		if (count == 0)
		{
			return;
		}
		float num2 = num / (float)count;
		foreach (IntVec3 item2 in b.OccupiedRect())
		{
			Room room3 = item2.GetRoom(b.Map);
			if (room3 != null)
			{
				room3.Temperature = num2;
			}
		}
		if (beqRooms.Count == 1)
		{
			return;
		}
		float num3 = 1f;
		foreach (Room beqRoom in beqRooms)
		{
			if (!beqRoom.UsesOutdoorTemperature)
			{
				float temperature = beqRoom.Temperature;
				float num4 = (num2 - temperature) * rate;
				float num5 = num4 / (float)beqRoom.CellCount;
				float num6 = beqRoom.Temperature + num5;
				if (num4 > 0f && num6 > num2)
				{
					num6 = num2;
				}
				else if (num4 < 0f && num6 < num2)
				{
					num6 = num2;
				}
				float num7 = Mathf.Abs((num6 - temperature) * (float)beqRoom.CellCount / num4);
				if (num7 < num3)
				{
					num3 = num7;
				}
			}
		}
		foreach (Room beqRoom2 in beqRooms)
		{
			if (!beqRoom2.UsesOutdoorTemperature)
			{
				float temperature2 = beqRoom2.Temperature;
				float num8 = num2 - temperature2;
				float num9 = ((b.Map.Biome.inVacuum && num8 < 0f) ? 0.1f : 1f);
				float num10 = num8 * rate * num3 * num9 / (float)beqRoom2.CellCount;
				beqRoom2.Temperature += num10;
			}
		}
		beqRooms.Clear();
	}

	public static float RotRateAtTemperature(float temperature)
	{
		if (temperature < 0f)
		{
			return 0f;
		}
		if (temperature >= 10f)
		{
			return 1f;
		}
		return (temperature - 0f) / 10f;
	}

	public static bool FactionOwnsPassableRoomInTemperatureRange(Faction faction, FloatRange tempRange, Map map)
	{
		if (faction == Faction.OfPlayer)
		{
			IReadOnlyList<Room> allRooms = map.regionGrid.AllRooms;
			for (int i = 0; i < allRooms.Count; i++)
			{
				Room room = allRooms[i];
				if (room.AnyPassable && !room.Fogged && tempRange.Includes(room.Temperature))
				{
					return true;
				}
			}
			return false;
		}
		return false;
	}

	public static string GetAverageTemperatureLabel(PlanetTile tile)
	{
		return ((!tile.Valid) ? 21f : Find.WorldGrid[tile].temperature).ToStringTemperature() + string.Format(" ({0} {1} {2})", MinTemperatureAtTile(tile).ToStringTemperature("F0"), "RangeTo".Translate(), MaxTemperatureAtTile(tile).ToStringTemperature("F0"));
	}

	public static float CelsiusTo(float temp, TemperatureDisplayMode oldMode)
	{
		return oldMode switch
		{
			TemperatureDisplayMode.Celsius => temp, 
			TemperatureDisplayMode.Fahrenheit => temp * 1.8f + 32f, 
			TemperatureDisplayMode.Kelvin => temp + 273.15f, 
			_ => throw new InvalidOperationException(), 
		};
	}

	public static float CelsiusToOffset(float temp, TemperatureDisplayMode oldMode)
	{
		return oldMode switch
		{
			TemperatureDisplayMode.Celsius => temp, 
			TemperatureDisplayMode.Fahrenheit => temp * 1.8f, 
			TemperatureDisplayMode.Kelvin => temp, 
			_ => throw new InvalidOperationException(), 
		};
	}

	public static float ConvertTemperatureOffset(float temp, TemperatureDisplayMode oldMode, TemperatureDisplayMode newMode)
	{
		switch (oldMode)
		{
		case TemperatureDisplayMode.Fahrenheit:
			temp /= 1.8f;
			break;
		}
		switch (newMode)
		{
		case TemperatureDisplayMode.Fahrenheit:
			temp *= 1.8f;
			break;
		}
		return temp;
	}
}
