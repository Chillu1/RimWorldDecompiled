using System;
using System.Collections.Generic;
using RimWorld;
using UnityEngine;

namespace Verse;

public class GasGrid : IExposable
{
	private uint[] gasDensity;

	private Map map;

	private int cycleIndexDiffusion;

	private int cycleIndexDissipation;

	[Unsaved(false)]
	private List<IntVec3> cardinalDirections;

	[Unsaved(false)]
	private List<IntVec3> cellsInRandomOrder;

	[Unsaved(false)]
	private bool anyGasEverAdded;

	public const int MaxGasPerCell = 255;

	private const int EstimatedMaxGasPerCell = 48;

	private const float CellsToDissipatePerTickFactor = 1f / 64f;

	private const float CellsToDiffusePerTickFactor = 1f / 32f;

	private const float VacuumDissipatePerTickFactor = 25f;

	private const float MaxOverflowFloodfillRadius = 40f;

	private const int DissipationAmount_BlindSmoke = 4;

	private const int DissipationAmount_ToxGas = 3;

	private const int DissipationAmount_RotStink = 4;

	private const int DissipationAmount_DeadlifeDust = 3;

	private const int MinDiffusion = 17;

	private const int AnyGasCheckIntervalTicks = 600;

	private static readonly IntVec3[] beqCells = new IntVec3[4];

	public bool CalculateGasEffects => anyGasEverAdded;

	public GasGrid(Map map)
	{
		this.map = map;
		gasDensity = new uint[map.cellIndices.NumGridCells];
		cardinalDirections = new List<IntVec3>();
		cardinalDirections.AddRange(GenAdj.CardinalDirections);
		cycleIndexDiffusion = Rand.Range(0, map.Area / 2);
	}

	public void RecalculateEverHadGas()
	{
		anyGasEverAdded = false;
		for (int i = 0; i < gasDensity.Length; i++)
		{
			if (gasDensity[i] != 0)
			{
				anyGasEverAdded = true;
				break;
			}
		}
	}

	public void Tick()
	{
		if (!CalculateGasEffects)
		{
			return;
		}
		int area = map.Area;
		int num = Mathf.CeilToInt((float)area * (1f / 64f));
		cellsInRandomOrder = map.cellsInRandomOrder.GetAll();
		using (new ProfilerBlock("Dissipation"))
		{
			for (int i = 0; i < num; i++)
			{
				if (cycleIndexDissipation >= area)
				{
					cycleIndexDissipation = 0;
				}
				using (new ProfilerBlock("TryDissipateGases"))
				{
					TryDissipateGases(CellIndicesUtility.CellToIndex(cellsInRandomOrder[cycleIndexDissipation], map.Size.x));
				}
				cycleIndexDissipation++;
			}
		}
		using (new ProfilerBlock("Diffusion"))
		{
			num = Mathf.CeilToInt((float)area * (1f / 32f));
			for (int j = 0; j < num; j++)
			{
				if (cycleIndexDiffusion >= area)
				{
					cycleIndexDiffusion = 0;
				}
				using (new ProfilerBlock("TryDiffuseGases"))
				{
					TryDiffuseGases(cellsInRandomOrder[cycleIndexDiffusion]);
				}
				cycleIndexDiffusion++;
			}
		}
		if (map.IsHashIntervalTick(600))
		{
			RecalculateEverHadGas();
		}
	}

	public bool AnyGasAt(IntVec3 cell)
	{
		return AnyGasAt(CellIndicesUtility.CellToIndex(cell, map.Size.x));
	}

	private bool AnyGasAt(int idx)
	{
		return gasDensity[idx] != 0;
	}

	public byte DensityAt(IntVec3 cell, GasType gasType)
	{
		return DensityAt(CellIndicesUtility.CellToIndex(cell, map.info.Size.x), gasType);
	}

	private byte DensityAt(int index, GasType gasType)
	{
		return (byte)((gasDensity[index] >> (int)gasType) & 0xFF);
	}

	public float DensityPercentAt(IntVec3 cell, GasType gasType)
	{
		return (float)(int)DensityAt(cell, gasType) / 255f;
	}

	public void AddGas(IntVec3 cell, GasType gasType, int amount, bool canOverflow = true)
	{
		if (amount <= 0 || !GasCanMoveTo(cell))
		{
			return;
		}
		anyGasEverAdded = true;
		int index = CellIndicesUtility.CellToIndex(cell, map.Size.x);
		byte b = DensityAt(index, GasType.BlindSmoke);
		byte b2 = DensityAt(index, GasType.ToxGas);
		byte b3 = DensityAt(index, GasType.RotStink);
		byte b4 = DensityAt(index, GasType.DeadlifeDust);
		int overflow = 0;
		switch (gasType)
		{
		case GasType.BlindSmoke:
			b = AdjustedDensity(b + amount, out overflow);
			break;
		case GasType.ToxGas:
			if (!ModLister.CheckBiotech("Tox gas"))
			{
				return;
			}
			b2 = AdjustedDensity(b2 + amount, out overflow);
			break;
		case GasType.RotStink:
			b3 = AdjustedDensity(b3 + amount, out overflow);
			break;
		case GasType.DeadlifeDust:
			if (!ModLister.CheckAnomaly("Deadlife dust"))
			{
				return;
			}
			b4 = AdjustedDensity(b4 + amount, out overflow);
			break;
		default:
			Log.Error("Trying to add unknown gas type.");
			return;
		}
		SetDirect(index, b, b2, b3, b4);
		map.mapDrawer.MapMeshDirty(cell, MapMeshFlagDefOf.Gas);
		if (canOverflow && overflow > 0)
		{
			Overflow(cell, gasType, overflow);
		}
	}

	private byte AdjustedDensity(int newDensity, out int overflow)
	{
		if (newDensity > 255)
		{
			overflow = newDensity - 255;
			return byte.MaxValue;
		}
		overflow = 0;
		if (newDensity < 0)
		{
			return 0;
		}
		return (byte)newDensity;
	}

	public Vector4 DensitiesAt(IntVec3 cell)
	{
		int index = CellIndicesUtility.CellToIndex(cell, map.Size.x);
		float x = (int)DensityAt(index, GasType.BlindSmoke);
		float y = (int)DensityAt(index, GasType.ToxGas);
		float z = (int)DensityAt(index, GasType.RotStink);
		float w = (int)DensityAt(index, GasType.DeadlifeDust);
		return new Vector4(x, y, z, w);
	}

	public void Notify_ThingSpawned(Thing thing)
	{
		if (!thing.Spawned || thing.def.Fillage != FillCategory.Full)
		{
			return;
		}
		foreach (IntVec3 item in thing.OccupiedRect())
		{
			if (AnyGasAt(item))
			{
				gasDensity[CellIndicesUtility.CellToIndex(item, map.Size.x)] = 0u;
				map.mapDrawer.MapMeshDirty(item, MapMeshFlagDefOf.Gas);
			}
		}
	}

	public uint GetDirect(IntVec3 c)
	{
		return gasDensity[CellIndicesUtility.CellToIndex(c, map.Size.x)];
	}

	public uint GetDirect(int index)
	{
		return gasDensity[index];
	}

	public void SetDirect(IntVec3 c, byte smoke, byte toxic, byte rotStink, byte deadlife)
	{
		SetDirect(CellIndicesUtility.CellToIndex(c, map.Size.x), smoke, toxic, rotStink, deadlife);
	}

	public void SetDirect(int index, byte smoke, byte toxic, byte rotStink, byte deadlife)
	{
		if (!ModsConfig.BiotechActive)
		{
			toxic = 0;
		}
		if (!ModsConfig.AnomalyActive)
		{
			deadlife = 0;
		}
		gasDensity[index] = (uint)((rotStink << 16) | (toxic << 8) | smoke | (deadlife << 24));
	}

	public void SetDirect(IntVec3 c, uint packedDensities)
	{
		SetDirect(CellIndicesUtility.CellToIndex(c, map.Size.x), packedDensities);
	}

	public void SetDirect(int index, uint packedDensities)
	{
		uint num = uint.MaxValue;
		if (!ModsConfig.BiotechActive)
		{
			num &= 0xFFFF00FFu;
		}
		if (!ModsConfig.AnomalyActive)
		{
			num &= 0xFFFFFF;
		}
		gasDensity[index] = packedDensities & num;
		anyGasEverAdded = true;
	}

	private void Overflow(IntVec3 cell, GasType gasType, int amount)
	{
		if (amount <= 0)
		{
			return;
		}
		int remainingAmount = amount;
		map.floodFiller.FloodFill(cell, GasCanMoveTo, delegate(IntVec3 c)
		{
			int num = Mathf.Min(remainingAmount, 255 - DensityAt(c, gasType));
			if (num > 0)
			{
				AddGas(c, gasType, num, canOverflow: false);
				remainingAmount -= num;
			}
			return remainingAmount <= 0;
		}, GenRadial.NumCellsInRadius(40f), rememberParents: true);
	}

	public void EstimateGasDiffusion(IntVec3 cell, GasType gasType, int amount, Action<IntVec3> processor)
	{
		processor(cell);
		int num = Mathf.Min(amount, 48 - DensityAt(cell, gasType));
		amount -= num;
		map.floodFiller.FloodFill(cell, GasCanMoveTo, delegate(IntVec3 c)
		{
			int num2 = Mathf.Min(amount, 48 - DensityAt(c, gasType));
			if (num2 > 0)
			{
				processor(c);
				amount -= num2;
			}
			return amount <= 0;
		}, GenRadial.NumCellsInRadius(40f), rememberParents: true);
	}

	private void TryDissipateGases(int index)
	{
		if (!AnyGasAt(index))
		{
			return;
		}
		IntVec3 intVec = CellIndicesUtility.IndexToCell(index, map.Size.x);
		float vacuum = intVec.GetVacuum(map);
		float num = (intVec.Roofed(map) ? 0.5f : 1f);
		num += vacuum * 25f;
		bool flag = false;
		int num2 = DensityAt(index, GasType.BlindSmoke);
		if (num2 > 0)
		{
			num2 = Math.Max(num2 - Mathf.RoundToInt(4f * num), 0);
			if (num2 == 0)
			{
				flag = true;
			}
		}
		int num3 = DensityAt(index, GasType.ToxGas);
		if (num3 > 0)
		{
			num3 = Math.Max(num3 - Mathf.RoundToInt(3f * num), 0);
			if (num3 == 0)
			{
				flag = true;
			}
		}
		int num4 = DensityAt(index, GasType.RotStink);
		if (num4 > 0)
		{
			num4 = Math.Max(num4 - Mathf.RoundToInt(4f * num), 0);
			if (num4 == 0)
			{
				flag = true;
			}
		}
		int num5 = DensityAt(index, GasType.DeadlifeDust);
		if (num5 > 0)
		{
			num5 = Math.Max(num5 - Mathf.RoundToInt(3f * num), 0);
			if (num5 == 0)
			{
				flag = true;
			}
		}
		SetDirect(index, (byte)num2, (byte)num3, (byte)num4, (byte)num5);
		if (flag)
		{
			map.mapDrawer.MapMeshDirty(intVec, MapMeshFlagDefOf.Gas);
		}
	}

	private void TryDiffuseGases(IntVec3 cell)
	{
		int index = CellIndicesUtility.CellToIndex(cell, map.info.Size.x);
		int gasA = DensityAt(index, GasType.ToxGas);
		int gasA2 = DensityAt(index, GasType.RotStink);
		int gasA3 = DensityAt(index, GasType.DeadlifeDust);
		if (gasA + gasA2 + gasA3 < 17)
		{
			return;
		}
		bool flag = false;
		cardinalDirections.Shuffle();
		for (int i = 0; i < cardinalDirections.Count; i++)
		{
			IntVec3 intVec = cell + cardinalDirections[i];
			if (!GasCanMoveTo(intVec))
			{
				continue;
			}
			int index2 = CellIndicesUtility.CellToIndex(intVec, map.Size.x);
			int gasB = DensityAt(index2, GasType.ToxGas);
			int gasB2 = DensityAt(index2, GasType.RotStink);
			int gasB3 = DensityAt(index2, GasType.DeadlifeDust);
			if ((0u | (TryDiffuseIndividualGas(ref gasA, ref gasB) ? 1u : 0u) | (TryDiffuseIndividualGas(ref gasA2, ref gasB2) ? 1u : 0u) | (TryDiffuseIndividualGas(ref gasA3, ref gasB3) ? 1u : 0u)) != 0)
			{
				SetDirect(index2, DensityAt(index2, GasType.BlindSmoke), (byte)gasB, (byte)gasB2, (byte)gasB3);
				map.mapDrawer.MapMeshDirty(intVec, MapMeshFlagDefOf.Gas);
				flag = true;
				if (gasA + gasA2 < 17)
				{
					break;
				}
			}
		}
		if (flag)
		{
			SetDirect(index, DensityAt(index, GasType.BlindSmoke), (byte)gasA, (byte)gasA2, (byte)gasA3);
			map.mapDrawer.MapMeshDirty(cell, MapMeshFlagDefOf.Gas);
		}
	}

	private bool TryDiffuseIndividualGas(ref int gasA, ref int gasB)
	{
		if (gasA < 17)
		{
			return false;
		}
		int num = Mathf.Abs(gasA - gasB) / 2;
		if (gasA > gasB && num >= 17)
		{
			gasA -= num;
			gasB += num;
			return true;
		}
		return false;
	}

	public bool GasCanMoveTo(IntVec3 cell)
	{
		if (!cell.InBounds(map))
		{
			return false;
		}
		Building edifice = cell.GetEdifice(map);
		if (edifice != null && edifice.def.Fillage == FillCategory.Full)
		{
			if (edifice is Building_Door building_Door)
			{
				return building_Door.Open;
			}
			return false;
		}
		return true;
	}

	public void EqualizeGasThroughBuilding(Building b, bool twoWay)
	{
		if (!CalculateGasEffects)
		{
			return;
		}
		for (int i = 0; i < beqCells.Length; i++)
		{
			beqCells[i] = IntVec3.Invalid;
		}
		int beqCellCount = 0;
		int totalRotStink = 0;
		int totalToxGas = 0;
		int totalDeadlifeDust = 0;
		if (twoWay)
		{
			for (int j = 0; j < 2; j++)
			{
				IntVec3 cell = ((j == 0) ? (b.Position + b.Rotation.FacingCell) : (b.Position - b.Rotation.FacingCell));
				VisitCell(cell);
			}
		}
		else
		{
			for (int k = 0; k < 4; k++)
			{
				IntVec3 cell2 = b.Position + GenAdj.CardinalDirections[k];
				VisitCell(cell2);
			}
		}
		if (beqCellCount <= 1)
		{
			return;
		}
		byte toxic = (byte)Mathf.Min(totalToxGas / beqCellCount, 255);
		byte rotStink = (byte)Mathf.Min(totalRotStink / beqCellCount, 255);
		byte deadlife = (byte)Mathf.Min(totalDeadlifeDust / beqCellCount, 255);
		for (int l = 0; l < beqCellCount; l++)
		{
			if (beqCells[l].IsValid)
			{
				SetDirect(map.cellIndices.CellToIndex(beqCells[l]), DensityAt(beqCells[l], GasType.BlindSmoke), toxic, rotStink, deadlife);
				map.mapDrawer.MapMeshDirty(beqCells[l], MapMeshFlagDefOf.Gas);
			}
		}
		void VisitCell(IntVec3 intVec)
		{
			if (intVec.IsValid && GasCanMoveTo(intVec))
			{
				if (AnyGasAt(intVec))
				{
					totalRotStink += DensityAt(intVec, GasType.RotStink);
					if (ModsConfig.BiotechActive)
					{
						totalToxGas += DensityAt(intVec, GasType.ToxGas);
					}
					if (ModsConfig.AnomalyActive)
					{
						totalDeadlifeDust += DensityAt(intVec, GasType.DeadlifeDust);
					}
				}
				beqCells[beqCellCount] = intVec;
				beqCellCount++;
			}
		}
	}

	public void ClearCellUnsafe(IntVec3 cell)
	{
		gasDensity[CellIndicesUtility.CellToIndex(cell, map.Size.x)] = 0u;
	}

	public void Debug_ClearAll()
	{
		for (int i = 0; i < gasDensity.Length; i++)
		{
			gasDensity[i] = 0u;
		}
		anyGasEverAdded = false;
		map.mapDrawer.WholeMapChanged(MapMeshFlagDefOf.Gas);
	}

	public void Debug_FillAll()
	{
		for (int i = 0; i < gasDensity.Length; i++)
		{
			if (GasCanMoveTo(map.cellIndices.IndexToCell(i)))
			{
				SetDirect(i, byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);
			}
		}
		anyGasEverAdded = true;
		map.mapDrawer.WholeMapChanged(MapMeshFlagDefOf.Gas);
	}

	public void ExposeData()
	{
		MapExposeUtility.ExposeUint(map, (IntVec3 c) => gasDensity[map.cellIndices.CellToIndex(c)], delegate(IntVec3 c, uint val)
		{
			gasDensity[map.cellIndices.CellToIndex(c)] = val;
		}, "gasDensity");
		Scribe_Values.Look(ref cycleIndexDiffusion, "cycleIndexDiffusion", 0);
		Scribe_Values.Look(ref cycleIndexDissipation, "cycleIndexDissipation", 0);
	}
}
