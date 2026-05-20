using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;

namespace Verse;

public sealed class RoomTempTracker
{
	private Room room;

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

	private const float UndergroundEqualizeFractionPerTick = 0.002f;

	private const float VacuumEqualizeFractionPerTick = 5E-05f;

	private static int debugGetFrame = -999;

	private static float debugWallEq;

	private Map Map => room.Map;

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

	public RoomTempTracker(Room room, Map map)
	{
		this.room = room;
		Temperature = map.mapTemperature.OutdoorTemp;
	}

	public void RoofChanged()
	{
		RegenerateEqualizationData();
	}

	public void RoomChanged()
	{
		if (Map != null && !GravshipPlacementUtility.placingGravship)
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
		if (room.DistrictCount != 0)
		{
			Map map = Map;
			if (!room.UsesOutdoorTemperature)
			{
				CalculateRoofCovereage(map);
				RegenerateEqualizeCells(map);
			}
		}
	}

	private void CalculateRoofCovereage(Map map)
	{
		int num = 0;
		foreach (IntVec3 cell in room.Cells)
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
	}

	private void RegenerateEqualizeCells(Map map)
	{
		foreach (IntVec3 cell in room.Cells)
		{
			for (int i = 0; i < 4; i++)
			{
				IntVec3 intVec = cell + GenAdj.CardinalDirections[i];
				IntVec3 intVec2 = cell + GenAdj.CardinalDirections[i] * 2;
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
							if (regionA.Room != room && !regionA.IsDoorway)
							{
								flag = true;
								break;
							}
							if (regionB.Room != room && !regionB.IsDoorway)
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
				if (!intVec2.InBounds(map) || intVec2.GetRoom(map) == room)
				{
					continue;
				}
				bool flag2 = false;
				for (int k = 0; k < 4; k++)
				{
					if ((intVec2 + GenAdj.CardinalDirections[k]).GetRoom(map) == room)
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
		if (this.room.UsesOutdoorTemperature)
		{
			Temperature = Map.mapTemperature.OutdoorTemp;
		}
		else if (this.room.IsDoorway)
		{
			bool flag = true;
			IntVec3 anyCell = this.room.Districts[0].Regions[0].AnyCell;
			for (int i = 0; i < 4; i++)
			{
				IntVec3 intVec = anyCell + GenAdj.CardinalDirections[i];
				if (intVec.InBounds(Map))
				{
					Room room = intVec.GetRoom(Map);
					if (room != null && !room.IsDoorway)
					{
						flag = false;
						break;
					}
				}
			}
			if (flag)
			{
				this.room.Temperature += WallEqualizationTempChangePerInterval();
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
			if (GenTemperature.TryGetDirectAirTemperatureForCell(equalizeCells[index], Map, out var temperature))
			{
				if (Map.Biome.inVacuum)
				{
					float num3 = temperature - Temperature;
					num += num3 * 5E-05f;
				}
				else
				{
					num += temperature - Temperature;
				}
				continue;
			}
			float outdoorTemp = Map.mapTemperature.OutdoorTemp;
			if (Map.Biome.inVacuum)
			{
				float num4 = outdoorTemp - Temperature;
				num += num4 * 5E-05f;
			}
			else
			{
				num += Mathf.Lerp(Temperature, outdoorTemp, 0.5f) - Temperature;
			}
		}
		return num / (float)num2 * (float)equalizeCells.Count * 120f * 0.00017f / (float)room.CellCount;
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
		float num = TempDiffFromOutdoorsAdjusted();
		float num2 = (Map.Biome.inVacuum ? 5E-05f : 5E-05f);
		return num * ThinRoofCoverage * num2 * 120f;
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
		float num2 = ((Map.generatorDef != null && Map.generatorDef.isUnderground) ? 0.002f : 5E-05f);
		return num * thickRoofCoverage * num2 * 120f;
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
		if (room.UsesOutdoorTemperature)
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
		return "  thick roof coverage: " + thickRoofCoverage.ToStringPercent("F0") + "\n  thin roof coverage: " + ThinRoofCoverage.ToStringPercent("F0") + "\n  no roof coverage: " + noRoofCoverage.ToStringPercent("F0") + "\n\n  wall equalization: " + debugWallEq.ToStringTemperatureOffset("F3") + "\n  thin roof equalization: " + ThinRoofEqualizationTempChangePerInterval().ToStringTemperatureOffset("F3") + "\n  no roof equalization: " + NoRoofEqualizationTempChangePerInterval().ToStringTemperatureOffset("F3") + "\n  deep equalization: " + DeepEqualizationTempChangePerInterval().ToStringTemperatureOffset("F3") + "\n\n  temp diff from outdoors, adjusted: " + TempDiffFromOutdoorsAdjusted().ToStringTemperatureOffset("F3") + "\n  tempChange e=20 targ= 200C: " + GenTemperature.ControlTemperatureTempChange(room.Cells.First(), room.Map, 20f, 200f) + "\n  tempChange e=20 targ=-200C: " + GenTemperature.ControlTemperatureTempChange(room.Cells.First(), room.Map, 20f, -200f) + "\n  equalize interval ticks: " + 120 + "\n  equalize cells count:" + equalizeCells.Count;
	}
}
