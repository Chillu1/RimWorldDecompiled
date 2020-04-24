using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Verse
{
	public static class GenTemperature
	{
		public static readonly Color ColorSpotHot = new Color(1f, 0f, 0f, 0.6f);

		public static readonly Color ColorSpotCold = new Color(0f, 0f, 1f, 0.6f);

		public static readonly Color ColorRoomHot = new Color(1f, 0f, 0f, 0.3f);

		public static readonly Color ColorRoomCold = new Color(0f, 0f, 1f, 0.3f);

		private static List<RoomGroup> neighRoomGroups = new List<RoomGroup>();

		private static RoomGroup[] beqRoomGroups = new RoomGroup[4];

		public static float AverageTemperatureAtTileForTwelfth(int tile, Twelfth twelfth)
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

		public static float MinTemperatureAtTile(int tile)
		{
			float num = float.MaxValue;
			for (int i = 0; i < 3600000; i += 27000)
			{
				num = Mathf.Min(num, GetTemperatureFromSeasonAtTile(i, tile));
			}
			return num;
		}

		public static float MaxTemperatureAtTile(int tile)
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
			return new FloatRange(p.GetStatValue(StatDefOf.ComfyTemperatureMin), p.GetStatValue(StatDefOf.ComfyTemperatureMax));
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

		public static FloatRange SafeTemperatureRange(ThingDef raceDef, List<ThingStuffPair> apparel = null)
		{
			FloatRange result = ComfortableTemperatureRange(raceDef, apparel);
			result.min -= 10f;
			result.max += 10f;
			return result;
		}

		public static float GetTemperatureForCell(IntVec3 c, Map map)
		{
			TryGetTemperatureForCell(c, map, out float tempResult);
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
			RoomGroup roomGroup = c.GetRoomGroup(map);
			if (roomGroup == null)
			{
				temperature = 21f;
				return false;
			}
			temperature = roomGroup.Temperature;
			return true;
		}

		public static bool TryGetAirTemperatureAroundThing(Thing t, out float temperature)
		{
			float num = 0f;
			int num2 = 0;
			List<IntVec3> list = GenAdjFast.AdjacentCells8Way(t);
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i].InBounds(t.Map) && TryGetDirectAirTemperatureForCell(list[i], t.Map, out float temperature2))
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

		public static float OffsetFromSunCycle(int absTick, int tile)
		{
			float num = GenDate.DayPercent(absTick, Find.WorldGrid.LongLatOf(tile).x);
			return Mathf.Cos((float)Math.PI * 2f * (num + 0.32f)) * 7f;
		}

		public static float OffsetFromSeasonCycle(int absTick, int tile)
		{
			float num = (float)(absTick / 60000 % 60) / 60f;
			return Mathf.Cos((float)Math.PI * 2f * (num - Season.Winter.GetMiddleTwelfth(0f).GetBeginningYearPct())) * (0f - SeasonalShiftAmplitudeAt(tile));
		}

		public static float GetTemperatureFromSeasonAtTile(int absTick, int tile)
		{
			if (absTick == 0)
			{
				absTick = 1;
			}
			return Find.WorldGrid[tile].temperature + OffsetFromSeasonCycle(absTick, tile);
		}

		public static float GetTemperatureAtTile(int tile)
		{
			return Current.Game.FindMap(tile)?.mapTemperature.OutdoorTemp ?? GetTemperatureFromSeasonAtTile(GenTicks.TicksAbs, tile);
		}

		public static float SeasonalShiftAmplitudeAt(int tile)
		{
			if (Find.WorldGrid.LongLatOf(tile).y >= 0f)
			{
				return TemperatureTuning.SeasonalTempVariationCurve.Evaluate(Find.WorldGrid.DistanceFromEquatorNormalized(tile));
			}
			return 0f - TemperatureTuning.SeasonalTempVariationCurve.Evaluate(Find.WorldGrid.DistanceFromEquatorNormalized(tile));
		}

		public static List<Twelfth> TwelfthsInAverageTemperatureRange(int tile, float minTemp, float maxTemp)
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
				for (int j = (int)num2; j < 12 && twelfths.Contains((Twelfth)j); j++)
				{
					list.Add((Twelfth)j);
				}
				for (int k = 0; k < 12 && twelfths.Contains((Twelfth)k); k++)
				{
					list.Add((Twelfth)k);
				}
			}
			return twelfths;
		}

		public static Twelfth EarliestTwelfthInAverageTemperatureRange(int tile, float minTemp, float maxTemp)
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
			RoomGroup roomGroup = c.GetRoomGroup(map);
			if (roomGroup != null)
			{
				return roomGroup.PushHeat(energy);
			}
			neighRoomGroups.Clear();
			for (int i = 0; i < 8; i++)
			{
				IntVec3 intVec = c + GenAdj.AdjacentCells[i];
				if (intVec.InBounds(map))
				{
					roomGroup = intVec.GetRoomGroup(map);
					if (roomGroup != null)
					{
						neighRoomGroups.Add(roomGroup);
					}
				}
			}
			float energy2 = energy / (float)neighRoomGroups.Count;
			for (int j = 0; j < neighRoomGroups.Count; j++)
			{
				neighRoomGroups[j].PushHeat(energy2);
			}
			bool result = neighRoomGroups.Count > 0;
			neighRoomGroups.Clear();
			return result;
		}

		public static void PushHeat(Thing t, float energy)
		{
			IntVec3 result;
			if (t.GetRoomGroup() != null)
			{
				PushHeat(t.Position, t.Map, energy);
			}
			else if (GenAdj.TryFindRandomAdjacentCell8WayWithRoomGroup(t, out result))
			{
				PushHeat(result, t.Map, energy);
			}
		}

		public static float ControlTemperatureTempChange(IntVec3 cell, Map map, float energyLimit, float targetTemperature)
		{
			RoomGroup roomGroup = cell.GetRoomGroup(map);
			if (roomGroup == null || roomGroup.UsesOutdoorTemperature)
			{
				return 0f;
			}
			float b = energyLimit / (float)roomGroup.CellCount;
			float a = targetTemperature - roomGroup.Temperature;
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
			int num = 0;
			float num2 = 0f;
			if (twoWay)
			{
				for (int i = 0; i < 2; i++)
				{
					IntVec3 intVec = (i == 0) ? (b.Position + b.Rotation.FacingCell) : (b.Position - b.Rotation.FacingCell);
					if (intVec.InBounds(b.Map))
					{
						RoomGroup roomGroup = intVec.GetRoomGroup(b.Map);
						if (roomGroup != null)
						{
							num2 += roomGroup.Temperature;
							beqRoomGroups[num] = roomGroup;
							num++;
						}
					}
				}
			}
			else
			{
				for (int j = 0; j < 4; j++)
				{
					IntVec3 intVec2 = b.Position + GenAdj.CardinalDirections[j];
					if (intVec2.InBounds(b.Map))
					{
						RoomGroup roomGroup2 = intVec2.GetRoomGroup(b.Map);
						if (roomGroup2 != null)
						{
							num2 += roomGroup2.Temperature;
							beqRoomGroups[num] = roomGroup2;
							num++;
						}
					}
				}
			}
			if (num == 0)
			{
				return;
			}
			float num3 = num2 / (float)num;
			RoomGroup roomGroup3 = b.GetRoomGroup();
			if (roomGroup3 != null)
			{
				roomGroup3.Temperature = num3;
			}
			if (num == 1)
			{
				return;
			}
			float num4 = 1f;
			for (int k = 0; k < num; k++)
			{
				if (!beqRoomGroups[k].UsesOutdoorTemperature)
				{
					float temperature = beqRoomGroups[k].Temperature;
					float num5 = (num3 - temperature) * rate;
					float num6 = num5 / (float)beqRoomGroups[k].CellCount;
					float num7 = beqRoomGroups[k].Temperature + num6;
					if (num5 > 0f && num7 > num3)
					{
						num7 = num3;
					}
					else if (num5 < 0f && num7 < num3)
					{
						num7 = num3;
					}
					float num8 = Mathf.Abs((num7 - temperature) * (float)beqRoomGroups[k].CellCount / num5);
					if (num8 < num4)
					{
						num4 = num8;
					}
				}
			}
			for (int l = 0; l < num; l++)
			{
				if (!beqRoomGroups[l].UsesOutdoorTemperature)
				{
					float temperature2 = beqRoomGroups[l].Temperature;
					float num9 = (num3 - temperature2) * rate * num4 / (float)beqRoomGroups[l].CellCount;
					beqRoomGroups[l].Temperature += num9;
				}
			}
			for (int m = 0; m < beqRoomGroups.Length; m++)
			{
				beqRoomGroups[m] = null;
			}
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
				List<Room> allRooms = map.regionGrid.allRooms;
				for (int i = 0; i < allRooms.Count; i++)
				{
					Room room = allRooms[i];
					if (room.RegionType.Passable() && !room.Fogged && tempRange.Includes(room.Temperature))
					{
						return true;
					}
				}
				return false;
			}
			return false;
		}

		public static string GetAverageTemperatureLabel(int tile)
		{
			return Find.WorldGrid[tile].temperature.ToStringTemperature() + " " + string.Format("({0} {1} {2})", MinTemperatureAtTile(tile).ToStringTemperature("F0"), "RangeTo".Translate(), MaxTemperatureAtTile(tile).ToStringTemperature("F0"));
		}

		public static float CelsiusTo(float temp, TemperatureDisplayMode oldMode)
		{
			switch (oldMode)
			{
			case TemperatureDisplayMode.Celsius:
				return temp;
			case TemperatureDisplayMode.Fahrenheit:
				return temp * 1.8f + 32f;
			case TemperatureDisplayMode.Kelvin:
				return temp + 273.15f;
			default:
				throw new InvalidOperationException();
			}
		}

		public static float CelsiusToOffset(float temp, TemperatureDisplayMode oldMode)
		{
			switch (oldMode)
			{
			case TemperatureDisplayMode.Celsius:
				return temp;
			case TemperatureDisplayMode.Fahrenheit:
				return temp * 1.8f;
			case TemperatureDisplayMode.Kelvin:
				return temp;
			default:
				throw new InvalidOperationException();
			}
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
}
