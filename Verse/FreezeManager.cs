using System.Collections.Generic;
using RimWorld;
using UnityEngine;

namespace Verse;

public class FreezeManager : IExposable
{
	public const float WaterFreezeThreshold = -7f;

	private const float IceMeltThreshold = 2f;

	private const float BaseWaterFreezeChance = 0.003f;

	private const float BaseIceMeltChance = 0.001f;

	private const float TemperatureFactor = 5f;

	private const float FreezeRadius = 2.9f;

	private const int LakeFrozenPctExp = 10;

	private Map map;

	[Unsaved(false)]
	private List<IntVec3> tmpCellsToAffect = new List<IntVec3>();

	public FreezeManager()
	{
	}

	public FreezeManager(Map map)
	{
		this.map = map;
	}

	public void ExposeData()
	{
		Scribe_References.Look(ref map, "map");
	}

	public void DoCellSteadyEffects(IntVec3 c)
	{
		if (ModsConfig.OdysseyActive)
		{
			DoWaterFreezing(c);
			DoIceMelting(c);
		}
	}

	private void DoIceMelting(IntVec3 c)
	{
		float temperature = c.GetTemperature(map);
		Room room = c.GetRoom(map);
		if (temperature < 2f || !Rand.Chance(0.001f * ((temperature + 5f) / 5f)))
		{
			return;
		}
		tmpCellsToAffect.Clear();
		foreach (IntVec3 item in GenRadial.RadialCellsAround(c, 2.9f, useCenter: true))
		{
			if (item.InBounds(map) && item.GetRoom(map) == room && CanEverMelt(item) && CanMelt(item))
			{
				tmpCellsToAffect.Add(item);
			}
		}
		foreach (IntVec3 item2 in tmpCellsToAffect)
		{
			map.terrainGrid.RemoveTempTerrain(item2);
		}
	}

	private void DoWaterFreezing(IntVec3 c)
	{
		float temperature = c.GetTemperature(map);
		Room room = c.GetRoom(map);
		if (temperature > -7f || !CanFreeze(c))
		{
			return;
		}
		float num = 0.003f * (temperature / -5f);
		WaterBody waterBody = c.GetWaterBody(map);
		num += (1f - num) * Mathf.Pow((float)waterBody.numCellsFrozen / (float)waterBody.numCellsCanFreeze, 10f);
		if (SurroundedByIce(c))
		{
			num = 1f;
		}
		if (!Rand.Chance(num))
		{
			return;
		}
		tmpCellsToAffect.Clear();
		foreach (IntVec3 item in GenRadial.RadialCellsAround(c, 2.9f, useCenter: true))
		{
			if (item.InBounds(map) && item.GetRoom(map) == room && CanFreeze(item))
			{
				tmpCellsToAffect.Add(item);
			}
		}
		foreach (IntVec3 item2 in tmpCellsToAffect)
		{
			map.terrainGrid.SetTempTerrain(item2, TerrainDefOf.ThinIce);
		}
	}

	public static bool CanEverFreeze(IntVec3 c, Map map)
	{
		if (c.GetTerrain(map).canFreeze)
		{
			return true;
		}
		return false;
	}

	private bool CanFreeze(IntVec3 c)
	{
		if (!CanEverFreeze(c, map))
		{
			return false;
		}
		return !SurroundedByWater(c);
	}

	private bool CanEverMelt(IntVec3 c)
	{
		return c.GetTerrain(map) == TerrainDefOf.ThinIce;
	}

	private bool CanMelt(IntVec3 c)
	{
		if (!CanEverMelt(c))
		{
			return false;
		}
		return !SurroundedByIce(c);
	}

	private bool SurroundedByIce(IntVec3 c)
	{
		bool result = true;
		IntVec3[] cardinalDirections = GenAdj.CardinalDirections;
		foreach (IntVec3 intVec in cardinalDirections)
		{
			IntVec3 c2 = c + intVec;
			if (c2.InBounds(map) && c2.GetTerrain(map) != TerrainDefOf.ThinIce)
			{
				result = false;
			}
		}
		return result;
	}

	private bool SurroundedByWater(IntVec3 c)
	{
		bool result = true;
		IntVec3[] cardinalDirections = GenAdj.CardinalDirections;
		foreach (IntVec3 intVec in cardinalDirections)
		{
			IntVec3 c2 = c + intVec;
			if (c2.InBounds(map) && !c2.GetTerrain(map).IsWater)
			{
				result = false;
			}
		}
		return result;
	}
}
