using System;
using System.Collections.Generic;
using System.Text;
using RimWorld;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using UnityEngine;
using Verse.Glow;

namespace Verse;

[BurstCompile(CompileSynchronously = true)]
public sealed class GlowGrid : IDisposable
{
	private static class FloodFillerPool
	{
		public static NativeArray<GlowUniqueState> pool;

		private static bool allocated;

		public static void EnsureAllocated()
		{
			if (!allocated)
			{
				allocated = true;
				UnityData.DisposeStatic += Dispose;
				pool = new NativeArray<GlowUniqueState>(UnityData.MaxJobWorkerThreadCount, Allocator.Persistent);
				for (int i = 0; i < pool.Length; i++)
				{
					pool[i] = GlowUniqueState.AllocateNew();
				}
			}
		}

		private static void Dispose()
		{
			if (allocated)
			{
				for (int i = 0; i < pool.Length; i++)
				{
					pool[i].Dispose();
				}
				pool.Dispose();
				allocated = false;
			}
		}
	}

	private class GlowPool : IDisposable
	{
		public NativeList<LocalGlowArea> pool = new NativeList<LocalGlowArea>(4096, Allocator.Persistent);

		public int Take()
		{
			for (int i = 0; i < pool.Length; i++)
			{
				LocalGlowArea value = pool[i];
				if (!value.inUse)
				{
					value.Take();
					pool[i] = value;
					return i;
				}
			}
			LocalGlowArea value2 = LocalGlowArea.AllocateNew();
			value2.Take();
			pool.Add(in value2);
			return pool.Length - 1;
		}

		public void ReturnSet(int index)
		{
			LocalGlowArea value = pool[index];
			value.Return();
			pool[index] = value;
		}

		public void Dispose()
		{
			for (int i = 0; i < pool.Length; i++)
			{
				pool[i].Dispose();
			}
			pool.Dispose();
		}
	}

	[BurstCompile]
	private struct CombineColorsJob : IJobParallelFor
	{
		[ReadOnly]
		public NativeList<GlowLight> lights;

		public UnsafeBitArray dirtyCells;

		public CellIndices indices;

		public bool allDirty;

		[NativeDisableParallelForRestriction]
		public NativeArray<Color32> glow;

		[NativeDisableParallelForRestriction]
		public NativeArray<Color32> glowNoCavePlants;

		[NativeDisableParallelForRestriction]
		public NativeList<LocalGlowArea> glowPool;

		[BurstCompile]
		public void Execute(int i)
		{
			if (!dirtyCells.IsSet(i) && !allDirty)
			{
				return;
			}
			dirtyCells.Set(i, value: false);
			IntVec3 world = indices.IndexToCell(i);
			Color32 existingSum = default(Color32);
			Color32 existingSum2 = default(Color32);
			for (int j = 0; j < lights.Length; j++)
			{
				GlowLight glowLight = lights[j];
				if (glowLight.AffectedRect.Contains(world))
				{
					int index = glowLight.WorldToLocalIndex(in world);
					Color32 toAdd = glowPool[glowLight.localGlowPoolIndex].colors[index];
					AddColors(ref existingSum, in toAdd, glowLight.overlightRadius);
					if (!glowLight.isCavePlant)
					{
						AddColors(ref existingSum2, in toAdd, glowLight.overlightRadius);
					}
				}
			}
			glow[i] = existingSum;
			glowNoCavePlants[i] = existingSum2;
		}

		[BurstCompile]
		private void AddColors(ref Color32 existingSum, in Color32 toAdd, float overlightRadius)
		{
			float num = (int)toAdd.a;
			ColorInt colorInt = toAdd.AsColorInt();
			colorInt.ClampToNonNegative();
			colorInt.a = 0;
			if (colorInt.r > 0 || colorInt.g > 0 || colorInt.b > 0)
			{
				ColorInt colorInt2 = existingSum.AsColorInt();
				colorInt2 += colorInt;
				if (num < overlightRadius)
				{
					colorInt2.a = 1;
				}
				colorInt2.ProjectToColor32Fast(out var outColor);
				existingSum = outColor;
			}
		}
	}

	private readonly Map map;

	private readonly CellIndices indices;

	private readonly GlowPool glowPool;

	private NativeList<GlowLight> lights;

	private NativeBitArray lightBlockers;

	private UnsafeBitArray dirtyCells;

	private NativeArray<Color32> accumulatedGlow;

	private NativeArray<Color32> accumulatedGlowNoCavePlants;

	private readonly HashSet<CompGlower> litGlowers = new HashSet<CompGlower>();

	private readonly HashSet<IntVec3> litTerrain = new HashSet<IntVec3>();

	private bool anyDirtyCell = true;

	private bool anyDirtyLight;

	private bool hasRunInitially;

	private const int AlphaOfOverlit = 1;

	private const float GameGlowLitThreshold = 0.3f;

	private const float GameGlowOverlitThreshold = 0.9f;

	private const float GroundGameGlowFactor = 3.6f;

	private const float MaxGameGlowFromNonOverlitGroundLights = 0.5f;

	public const int MaxLightRadius = 40;

	public const int MaxLightDiameter = 81;

	public const int MaxLightCells = 6561;

	public GlowGrid(Map map)
	{
		this.map = map;
		indices = map.cellIndices;
		lights = new NativeList<GlowLight>(4096, Allocator.Persistent);
		lightBlockers = new NativeBitArray(indices.NumGridCells, Allocator.Persistent);
		dirtyCells = new UnsafeBitArray(indices.NumGridCells, Allocator.Persistent);
		accumulatedGlow = new NativeArray<Color32>(indices.NumGridCells, Allocator.Persistent);
		accumulatedGlowNoCavePlants = new NativeArray<Color32>(indices.NumGridCells, Allocator.Persistent);
		glowPool = new GlowPool();
		FloodFillerPool.EnsureAllocated();
	}

	private Color32 GetAccumulatedGlowAt(IntVec3 c, bool ignoreCavePlants = false)
	{
		return GetAccumulatedGlowAt(map.cellIndices.CellToIndex(c), ignoreCavePlants);
	}

	private Color32 GetAccumulatedGlowAt(int index, bool ignoreCavePlants = false)
	{
		if (!accumulatedGlow.IsCreated)
		{
			return default(Color32);
		}
		if (map.terrainGrid.TerrainAt(index).exposesToVacuum)
		{
			return default(Color32);
		}
		return (ignoreCavePlants ? accumulatedGlowNoCavePlants : accumulatedGlow)[index];
	}

	public Color32 VisualGlowAt(int index)
	{
		return GetAccumulatedGlowAt(index);
	}

	public Color32 VisualGlowAt(IntVec3 c)
	{
		return GetAccumulatedGlowAt(c);
	}

	public float GroundGlowAt(IntVec3 c, bool ignoreCavePlants = false, bool ignoreSky = false)
	{
		float num = 0f;
		if (!ignoreSky && !map.roofGrid.Roofed(c))
		{
			num = map.skyManager.CurSkyGlow;
			if (num >= 1f)
			{
				return num;
			}
		}
		Color32 accumulatedGlowAt = GetAccumulatedGlowAt(c, ignoreCavePlants);
		if (accumulatedGlowAt.a == 1)
		{
			return 1f;
		}
		float b = (float)Mathf.Max(Mathf.Max(accumulatedGlowAt.r, accumulatedGlowAt.g), accumulatedGlowAt.b) / 255f * 3.6f;
		b = Mathf.Min(0.5f, b);
		return Mathf.Max(num, b);
	}

	public PsychGlow PsychGlowAt(IntVec3 c)
	{
		return PsychGlowAtGlow(GroundGlowAt(c));
	}

	public static PsychGlow PsychGlowAtGlow(float glow)
	{
		if (glow > 0.9f)
		{
			return PsychGlow.Overlit;
		}
		if (glow > 0.3f)
		{
			return PsychGlow.Lit;
		}
		return PsychGlow.Dark;
	}

	public void RegisterGlower(CompGlower newGlow)
	{
		if (!litGlowers.Add(newGlow))
		{
			return;
		}
		lights.Add(new GlowLight(newGlow, glowPool.Take()));
		anyDirtyLight = true;
		ref NativeList<GlowLight> reference = ref lights;
		foreach (IntVec3 item in reference[reference.Length - 1].AffectedRect.ClipInsideMap(map))
		{
			DirtyCell(item);
		}
	}

	public void DeRegisterGlower(CompGlower oldGlow)
	{
		if (!litGlowers.Remove(oldGlow) || !lights.IsCreated)
		{
			return;
		}
		int glowerIndex = GetGlowerIndex(oldGlow);
		GlowLight glowLight = lights[glowerIndex];
		foreach (IntVec3 item in glowLight.AffectedRect.ClipInsideMap(map))
		{
			DirtyCell(item);
		}
		glowPool.ReturnSet(glowLight.localGlowPoolIndex);
		lights.RemoveAt(glowerIndex);
	}

	[BurstCompile]
	private int GetGlowerIndex(CompGlower glower)
	{
		for (int i = 0; i < lights.Length; i++)
		{
			GlowLight glowLight = lights[i];
			if (!glowLight.isTerrain && glowLight.id == glower.parent.thingIDNumber)
			{
				return i;
			}
		}
		return -1;
	}

	public void RegisterTerrain(IntVec3 cell)
	{
		if (!litTerrain.Add(cell))
		{
			return;
		}
		lights.Add(new GlowLight(cell, map, glowPool.Take()));
		anyDirtyLight = true;
		ref NativeList<GlowLight> reference = ref lights;
		foreach (IntVec3 item in reference[reference.Length - 1].AffectedRect.ClipInsideMap(map))
		{
			DirtyCell(item);
		}
	}

	public void DeregisterTerrain(IntVec3 cell)
	{
		if (!litTerrain.Remove(cell) || !lights.IsCreated)
		{
			return;
		}
		for (int i = 0; i < lights.Length; i++)
		{
			GlowLight glowLight = lights[i];
			if (!glowLight.isTerrain || glowLight.position != cell)
			{
				continue;
			}
			foreach (IntVec3 item in glowLight.AffectedRect.ClipInsideMap(map))
			{
				DirtyCell(item);
			}
			glowPool.ReturnSet(glowLight.localGlowPoolIndex);
			lights.RemoveAt(i);
			break;
		}
	}

	public void GlowGridUpdate_First()
	{
		if (!accumulatedGlow.IsCreated || !accumulatedGlowNoCavePlants.IsCreated)
		{
			return;
		}
		try
		{
			if (anyDirtyLight)
			{
				using (new ProfilerBlock("ComputeGlowGridsJob()"))
				{
					IJobParallelForExtensions.Schedule(new ComputeGlowGridsJob
					{
						lights = lights,
						indices = indices,
						lightBlockers = lightBlockers,
						statePool = FloodFillerPool.pool,
						glowPool = glowPool.pool,
						mapSize = map.Size
					}, lights.Length, UnityData.GetIdealBatchCount(lights.Length)).Complete();
					anyDirtyLight = false;
				}
			}
			if (anyDirtyCell)
			{
				using (new ProfilerBlock("CombineColorsJob()"))
				{
					IJobParallelForExtensions.Schedule(new CombineColorsJob
					{
						lights = lights,
						dirtyCells = dirtyCells,
						indices = indices,
						glow = accumulatedGlow,
						glowNoCavePlants = accumulatedGlowNoCavePlants,
						glowPool = glowPool.pool,
						allDirty = !hasRunInitially
					}, indices.NumGridCells, UnityData.GetIdealBatchCount(indices.NumGridCells)).Complete();
					anyDirtyCell = false;
				}
			}
		}
		catch (Exception ex)
		{
			Log.Error(ex.ToString());
			throw;
		}
		if (!hasRunInitially)
		{
			map.mapDrawer.WholeMapChanged(MapMeshFlagDefOf.GroundGlow);
		}
		hasRunInitially = true;
	}

	public void DirtyCell(IntVec3 cell)
	{
		if (dirtyCells.IsCreated && hasRunInitially)
		{
			dirtyCells.Set(indices.CellToIndex(cell), value: true);
			map.mapDrawer.MapMeshDirty(cell, MapMeshFlagDefOf.Roofs);
			map.mapDrawer.MapMeshDirty(cell, MapMeshFlagDefOf.GroundGlow);
			anyDirtyCell = true;
			map.events.Notify_GlowChanged(cell);
		}
	}

	public void LightBlockerAdded(IntVec3 cell)
	{
		lightBlockers.Set(indices.CellToIndex(cell), value: true);
		DirtyCell(cell);
		DirtyLightsAround(cell);
	}

	public void LightBlockerRemoved(IntVec3 cell)
	{
		if (lightBlockers.IsCreated)
		{
			lightBlockers.Set(indices.CellToIndex(cell), value: false);
			DirtyCell(cell);
			DirtyLightsAround(cell);
		}
	}

	[BurstCompile]
	private void DirtyLightsAround(IntVec3 cell)
	{
		if (!hasRunInitially)
		{
			return;
		}
		for (int i = 0; i < lights.Length; i++)
		{
			GlowLight value = lights[i];
			if (value.dirty || !value.AffectedRect.Contains(cell))
			{
				continue;
			}
			value.dirty = true;
			lights[i] = value;
			foreach (IntVec3 item in value.AffectedRect)
			{
				if (item.InBounds(map) && indices.Contains(item))
				{
					DirtyCell(item);
				}
			}
			anyDirtyLight = true;
		}
	}

	public void DevPrintLightIdsAffectingCell(IntVec3 cell)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine($"Lights affecting {cell}\n");
		for (int i = 0; i < lights.Length; i++)
		{
			if (lights[i].AffectedRect.Contains(cell))
			{
				stringBuilder.AppendLine($"   [{i}] - {lights[i].ToString()}");
			}
		}
		Log.Message(stringBuilder.ToString());
	}

	public void DevDirtyRect(CellRect rect)
	{
		foreach (IntVec3 cell in rect.Cells)
		{
			dirtyCells.Set(indices.CellToIndex(cell), value: true);
		}
		anyDirtyCell = true;
	}

	public void Dispose()
	{
		glowPool.Dispose();
		lights.Dispose();
		dirtyCells.Dispose();
		lightBlockers.Dispose();
		accumulatedGlow.Dispose();
		accumulatedGlowNoCavePlants.Dispose();
	}
}
