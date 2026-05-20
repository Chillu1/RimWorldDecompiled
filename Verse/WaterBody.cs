using System.Collections.Generic;
using RimWorld;
using UnityEngine;

namespace Verse;

public class WaterBody : IExposable
{
	public Map map;

	public IntVec3 rootCell;

	public WaterBodyType waterBodyType;

	[Unsaved(false)]
	public HashSet<IntVec3> cells = new HashSet<IntVec3>();

	[Unsaved(false)]
	public CellRect? cachedBounds;

	private bool shouldHaveFish;

	private List<ThingDef> commonFish = new List<ThingDef>();

	private List<ThingDef> uncommonFish = new List<ThingDef>();

	private int cellCount;

	private float population;

	[Unsaved(false)]
	private float cachedPopulationFactor = -1f;

	public int numCellsFrozen;

	public int numCellsPolluted;

	public int numCellsCanFreeze;

	public bool HasFish
	{
		get
		{
			if (commonFish.Count > 0 || uncommonFish.Count > 0)
			{
				return population > 0f;
			}
			return false;
		}
	}

	public float PollutionPct => (float)numCellsPolluted / (float)cellCount;

	public bool TotallyFrozen => numCellsFrozen == cells.Count;

	public int Size => cellCount;

	public float Population
	{
		get
		{
			return population;
		}
		set
		{
			population = value;
			population = Mathf.Clamp(population, 0f, MaxPopulation);
		}
	}

	public float MaxPopulation => Mathf.Max(map.TileInfo.MaxFishPopulation * PopulationFactor, 20f);

	public float PopulationFactor
	{
		get
		{
			if (cachedPopulationFactor < 0f)
			{
				cachedPopulationFactor = FishingUtility.FishPopulationFactorPerBodySizeCurve.Evaluate(Size);
				if (cachedPopulationFactor < 0f)
				{
					cachedPopulationFactor = 0f;
				}
			}
			return cachedPopulationFactor;
		}
	}

	public int CellCount
	{
		get
		{
			return cellCount;
		}
		set
		{
			cellCount = value;
			cachedPopulationFactor = -1f;
			cachedBounds = null;
		}
	}

	public CellRect Bounds
	{
		get
		{
			CellRect valueOrDefault = cachedBounds.GetValueOrDefault();
			if (!cachedBounds.HasValue)
			{
				valueOrDefault = CellRect.FromCellList(cells);
				cachedBounds = valueOrDefault;
			}
			return cachedBounds.Value;
		}
	}

	public IEnumerable<ThingDef> CommonFish => commonFish;

	public IEnumerable<ThingDef> UncommonFish => uncommonFish;

	public IEnumerable<ThingDef> CommonFishIncludingExtras
	{
		get
		{
			if (commonFish.NullOrEmpty())
			{
				yield break;
			}
			foreach (ThingDef item in commonFish)
			{
				yield return item;
			}
			if (ModsConfig.BiotechActive && FishingUtility.PollutionToxfishChanceCurve.Evaluate(PollutionPct) > 0f)
			{
				yield return ThingDefOf.Fish_Toxfish;
			}
		}
	}

	public WaterBody()
	{
	}

	public WaterBody(Map map, IntVec3 rootCell)
	{
		this.map = map;
		this.rootCell = rootCell;
		cellCount = 1;
		waterBodyType = rootCell.GetWaterBodyType(map);
	}

	public void Initialize()
	{
		InitializePopulation();
		SetFishTypes();
		RecacheState();
	}

	public void InitializePopulation()
	{
		population = MaxPopulation;
	}

	public void SetFishTypes()
	{
		if (map.Biome.fishTypes == null)
		{
			return;
		}
		commonFish.Clear();
		uncommonFish.Clear();
		shouldHaveFish = Rand.Chance(FishingUtility.ChanceForCommonFishFromWaterBodySizeCurve.Evaluate(Size));
		if (!shouldHaveFish)
		{
			return;
		}
		bool flag = Rand.Chance(FishingUtility.ChanceForUncommonFishFromWaterBodySizeCurve.Evaluate(Size));
		switch (waterBodyType)
		{
		case WaterBodyType.Freshwater:
		{
			if (map.Biome.fishTypes.freshwater_Common.TryRandomElement(out var result3))
			{
				commonFish.Add(result3.fishDef);
			}
			if (flag && map.Biome.fishTypes.freshwater_Uncommon.TryRandomElement(out var result4))
			{
				uncommonFish.Add(result4.fishDef);
			}
			break;
		}
		case WaterBodyType.Saltwater:
		{
			if (map.Biome.fishTypes.saltwater_Common.TryRandomElement(out var result))
			{
				commonFish.Add(result.fishDef);
			}
			if (flag && map.Biome.fishTypes.saltwater_Uncommon.TryRandomElement(out var result2))
			{
				uncommonFish.Add(result2.fishDef);
			}
			break;
		}
		}
	}

	public void RecacheState()
	{
		numCellsPolluted = 0;
		numCellsFrozen = 0;
		numCellsCanFreeze = 0;
		foreach (IntVec3 cell in cells)
		{
			if (cell.GetTerrain(map) == TerrainDefOf.ThinIce)
			{
				numCellsFrozen++;
			}
			if (map.terrainGrid.BaseTerrainAt(cell).canFreeze)
			{
				numCellsCanFreeze++;
			}
			if (cell.IsPolluted(map))
			{
				numCellsPolluted++;
			}
		}
		if (shouldHaveFish && commonFish.NullOrEmpty())
		{
			SetFishTypes();
		}
	}

	public void Notify_TerrainChanged(IntVec3 cell, TerrainDef oldTerr, TerrainDef newTerr)
	{
		if (cells.Contains(cell))
		{
			if (newTerr == TerrainDefOf.ThinIce && oldTerr != TerrainDefOf.ThinIce)
			{
				numCellsFrozen++;
			}
			if (newTerr != TerrainDefOf.ThinIce && oldTerr == TerrainDefOf.ThinIce)
			{
				numCellsFrozen--;
			}
		}
	}

	public void Notify_PollutionChanged(IntVec3 cell, bool isPolluted)
	{
		if (cells.Contains(cell))
		{
			if (isPolluted)
			{
				numCellsPolluted++;
			}
			else
			{
				numCellsPolluted--;
			}
		}
	}

	public override int GetHashCode()
	{
		return Gen.HashCombineInt(Gen.HashCombineInt(Gen.HashCombineInt(base.GetHashCode(), rootCell.GetHashCode()), cellCount), waterBodyType.GetHashCode());
	}

	public void ExposeData()
	{
		Scribe_Values.Look(ref rootCell, "rootCell");
		Scribe_Values.Look(ref cellCount, "cellCount", 0);
		Scribe_Values.Look(ref waterBodyType, "fishType", WaterBodyType.None);
		Scribe_Values.Look(ref population, "population", 0f);
		Scribe_Values.Look(ref shouldHaveFish, "shouldHaveFish", defaultValue: false);
		Scribe_Collections.Look(ref commonFish, "commonFish", LookMode.Def);
		Scribe_Collections.Look(ref uncommonFish, "uncommonFish", LookMode.Def);
		if (Scribe.mode == LoadSaveMode.PostLoadInit)
		{
			if (!commonFish.NullOrEmpty())
			{
				shouldHaveFish = true;
			}
			if (shouldHaveFish && commonFish == null)
			{
				commonFish = new List<ThingDef>();
				uncommonFish = new List<ThingDef>();
				SetFishTypes();
			}
			if (shouldHaveFish && Population > 0f && commonFish.Count == 0)
			{
				SetFishTypes();
			}
			commonFish.RemoveAll((ThingDef x) => x == null);
			uncommonFish.RemoveAll((ThingDef x) => x == null);
		}
	}
}
