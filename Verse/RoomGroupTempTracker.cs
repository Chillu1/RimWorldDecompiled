using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Verse
{
	public sealed class RoomGroupTempTracker
	{
		private RoomGroup roomGroup;

		private float temperatureInt;

		private List<IntVec3> equalizeCells = new List<IntVec3>();

		private float noRoofCoverage;

		private float thickRoofCoverage;

		public const float FractionWallEqualizeCells = 0.2f;

		public const float WallEqualizeFactor = 0.00017f;

		public const float EqualizationPowerOfFilledCells = 0.5f;

		private int cycleIndex;

		private const float ThinRoofEqualizeRate = 5E-05f;

		private const float NoRoofEqualizeRate = 0.0007f;

		private const float DeepEqualizeFractionPerTick = 5E-05f;

		private static int debugGetFrame = -999;

		private static float debugWallEq;

		private Map Map => roomGroup.Map;

		private float ThinRoofCoverage => 1f - (thickRoofCoverage + noRoofCoverage);

		public List<IntVec3> EqualizeCellsForReading => equalizeCells;

		public float Temperature
		{
			get
			{
				return temperatureInt;
			}
			set
			{
				temperatureInt = Mathf.Clamp(value, -273.15f, 1000f);
			}
		}

		public RoomGroupTempTracker(RoomGroup roomGroup, Map map)
		{
			this.roomGroup = roomGroup;
			Temperature = map.mapTemperature.OutdoorTemp;
		}

		public void RoofChanged()
		{
			RegenerateEqualizationData();
		}

		public void RoomChanged()
		{
			if (Map != null)
			{
				Map.autoBuildRoofAreaSetter.ResolveQueuedGenerateRoofs();
			}
			RegenerateEqualizationData();
		}

		private void RegenerateEqualizationData()
		{
			thickRoofCoverage = 0f;
			noRoofCoverage = 0f;
			equalizeCells.Clear();
			if (roomGroup.RoomCount == 0)
			{
				return;
			}
			Map map = Map;
			if (roomGroup.UsesOutdoorTemperature)
			{
				return;
			}
			int num = 0;
			foreach (IntVec3 cell in roomGroup.Cells)
			{
				RoofDef roof = cell.GetRoof(map);
				if (roof == null)
				{
					noRoofCoverage += 1f;
				}
				else if (roof.isThickRoof)
				{
					thickRoofCoverage += 1f;
				}
				num++;
			}
			thickRoofCoverage /= num;
			noRoofCoverage /= num;
			foreach (IntVec3 cell2 in roomGroup.Cells)
			{
				for (int i = 0; i < 4; i++)
				{
					IntVec3 intVec = cell2 + GenAdj.CardinalDirections[i];
					IntVec3 intVec2 = cell2 + GenAdj.CardinalDirections[i] * 2;
					if (intVec.InBounds(map))
					{
						Region region = intVec.GetRegion(map);
						if (region != null)
						{
							if (region.type != RegionType.Portal)
							{
								continue;
							}
							bool flag = false;
							for (int j = 0; j < region.links.Count; j++)
							{
								Region regionA = region.links[j].RegionA;
								Region regionB = region.links[j].RegionB;
								if (regionA.Room.Group != roomGroup && !regionA.IsDoorway)
								{
									flag = true;
									break;
								}
								if (regionB.Room.Group != roomGroup && !regionB.IsDoorway)
								{
									flag = true;
									break;
								}
							}
							if (flag)
							{
								continue;
							}
						}
					}
					if (!intVec2.InBounds(map) || intVec2.GetRoomGroup(map) == roomGroup)
					{
						continue;
					}
					bool flag2 = false;
					for (int k = 0; k < 4; k++)
					{
						if ((intVec2 + GenAdj.CardinalDirections[k]).GetRoomGroup(map) == roomGroup)
						{
							flag2 = true;
							break;
						}
					}
					if (!flag2)
					{
						equalizeCells.Add(intVec2);
					}
				}
			}
			equalizeCells.Shuffle();
		}

		public void EqualizeTemperature()
		{
			if (this.roomGroup.UsesOutdoorTemperature)
			{
				Temperature = Map.mapTemperature.OutdoorTemp;
			}
			else if (this.roomGroup.RoomCount != 0 && this.roomGroup.Rooms[0].RegionType == RegionType.Portal)
			{
				bool flag = true;
				IntVec3 a = this.roomGroup.Rooms[0].Cells.First();
				for (int i = 0; i < 4; i++)
				{
					IntVec3 intVec = a + GenAdj.CardinalDirections[i];
					if (intVec.InBounds(Map))
					{
						RoomGroup roomGroup = intVec.GetRoomGroup(Map);
						if (roomGroup != null && (roomGroup.RoomCount != 1 || roomGroup.Rooms[0].RegionType != RegionType.Portal))
						{
							flag = false;
							break;
						}
					}
				}
				if (flag)
				{
					this.roomGroup.Temperature += WallEqualizationTempChangePerInterval();
				}
			}
			else
			{
				float num = ThinRoofEqualizationTempChangePerInterval();
				float num2 = NoRoofEqualizationTempChangePerInterval();
				float num3 = WallEqualizationTempChangePerInterval();
				float num4 = DeepEqualizationTempChangePerInterval();
				Temperature += num + num2 + num3 + num4;
			}
		}

		private float WallEqualizationTempChangePerInterval()
		{
			if (equalizeCells.Count == 0)
			{
				return 0f;
			}
			float num = 0f;
			int num2 = Mathf.CeilToInt((float)equalizeCells.Count * 0.2f);
			for (int i = 0; i < num2; i++)
			{
				cycleIndex++;
				int index = cycleIndex % equalizeCells.Count;
				num = ((!GenTemperature.TryGetDirectAirTemperatureForCell(equalizeCells[index], Map, out var temperature)) ? (num + (Mathf.Lerp(Temperature, Map.mapTemperature.OutdoorTemp, 0.5f) - Temperature)) : (num + (temperature - Temperature)));
			}
			return num / (float)num2 * (float)equalizeCells.Count * 120f * 0.00017f / (float)roomGroup.CellCount;
		}

		private float TempDiffFromOutdoorsAdjusted()
		{
			float num = Map.mapTemperature.OutdoorTemp - temperatureInt;
			if (Mathf.Abs(num) < 100f)
			{
				return num;
			}
			return Mathf.Sign(num) * 100f + 5f * (num - Mathf.Sign(num) * 100f);
		}

		private float ThinRoofEqualizationTempChangePerInterval()
		{
			if (ThinRoofCoverage < 0.001f)
			{
				return 0f;
			}
			return TempDiffFromOutdoorsAdjusted() * ThinRoofCoverage * 5E-05f * 120f;
		}

		private float NoRoofEqualizationTempChangePerInterval()
		{
			if (noRoofCoverage < 0.001f)
			{
				return 0f;
			}
			return TempDiffFromOutdoorsAdjusted() * noRoofCoverage * 0.0007f * 120f;
		}

		private float DeepEqualizationTempChangePerInterval()
		{
			if (thickRoofCoverage < 0.001f)
			{
				return 0f;
			}
			float num = 15f - temperatureInt;
			if (num > 0f)
			{
				return 0f;
			}
			return num * thickRoofCoverage * 5E-05f * 120f;
		}

		public void DebugDraw()
		{
			foreach (IntVec3 equalizeCell in equalizeCells)
			{
				CellRenderer.RenderCell(equalizeCell);
			}
		}

		internal string DebugString()
		{
			if (roomGroup.UsesOutdoorTemperature)
			{
				return "uses outdoor temperature";
			}
			if (Time.frameCount > debugGetFrame + 120)
			{
				debugWallEq = 0f;
				for (int i = 0; i < 40; i++)
				{
					debugWallEq += WallEqualizationTempChangePerInterval();
				}
				debugWallEq /= 40f;
				debugGetFrame = Time.frameCount;
			}
			return "  thick roof coverage: " + thickRoofCoverage.ToStringPercent("F0") + "\n  thin roof coverage: " + ThinRoofCoverage.ToStringPercent("F0") + "\n  no roof coverage: " + noRoofCoverage.ToStringPercent("F0") + "\n\n  wall equalization: " + debugWallEq.ToStringTemperatureOffset("F3") + "\n  thin roof equalization: " + ThinRoofEqualizationTempChangePerInterval().ToStringTemperatureOffset("F3") + "\n  no roof equalization: " + NoRoofEqualizationTempChangePerInterval().ToStringTemperatureOffset("F3") + "\n  deep equalization: " + DeepEqualizationTempChangePerInterval().ToStringTemperatureOffset("F3") + "\n\n  temp diff from outdoors, adjusted: " + TempDiffFromOutdoorsAdjusted().ToStringTemperatureOffset("F3") + "\n  tempChange e=20 targ= 200C: " + GenTemperature.ControlTemperatureTempChange(roomGroup.Cells.First(), roomGroup.Map, 20f, 200f) + "\n  tempChange e=20 targ=-200C: " + GenTemperature.ControlTemperatureTempChange(roomGroup.Cells.First(), roomGroup.Map, 20f, -200f) + "\n  equalize interval ticks: " + 120 + "\n  equalize cells count:" + equalizeCells.Count;
		}
	}
}
