using LudeonTK;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using UnityEngine;

namespace Verse.Glow;

[BurstCompile]
public struct ComputeGlowGridsJob : IJobParallelFor
{
	private static readonly IntVec3[] Directions = new IntVec3[8]
	{
		new IntVec3(0, 0, -1),
		new IntVec3(1, 0, 0),
		new IntVec3(0, 0, 1),
		new IntVec3(-1, 0, 0),
		new IntVec3(1, 0, -1),
		new IntVec3(1, 0, 1),
		new IntVec3(-1, 0, 1),
		new IntVec3(-1, 0, -1)
	};

	private const int CardinalCost = 100;

	private const int DiagonalCost = 141;

	[NativeSetThreadIndex]
	[ReadOnly]
	private int threadIndex;

	[NativeDisableParallelForRestriction]
	public NativeList<GlowLight> lights;

	[ReadOnly]
	public NativeBitArray lightBlockers;

	[ReadOnly]
	public CellIndices indices;

	[NativeDisableParallelForRestriction]
	public NativeList<LocalGlowArea> glowPool;

	[NativeDisableParallelForRestriction]
	public NativeArray<GlowUniqueState> statePool;

	public IntVec3 mapSize;

	[BurstCompile]
	public void Execute(int index)
	{
		GlowLight light = lights[index];
		if (light.dirty)
		{
			light.dirty = false;
			lights[index] = light;
			GlowUniqueState filler = statePool[threadIndex];
			filler.PrepareComparer(ref light);
			LocalGlowArea localArea = glowPool[light.localGlowPoolIndex];
			PrepareFill(ref light, ref filler, ref localArea);
			Flood(ref light, ref filler, ref localArea);
			filler.Clear();
		}
	}

	[BurstCompile]
	private void Flood(ref GlowLight glowLight, ref GlowUniqueState filler, ref LocalGlowArea localArea)
	{
		int num = Mathf.RoundToInt(glowLight.glowRadius * 100f);
		int num2 = glowLight.diameter * glowLight.diameter;
		IntVec3 t;
		while (filler.queue.TryPop(out t))
		{
			int index = glowLight.DeltaToLocalIndex(in t);
			GlowCell value = filler.area[index];
			value.status = GlowCellStatus.Finalized;
			filler.area[index] = value;
			SetGlowFromDist(ref glowLight, ref filler, ref localArea, in t);
			for (int i = 0; i < Directions.Length; i++)
			{
				IntVec3 delta = t + Directions[i];
				int num3 = glowLight.DeltaToLocalIndex(in delta);
				if (num3 < 0 || num3 >= num2)
				{
					continue;
				}
				GlowCell value2 = filler.area[num3];
				if (value2.status == GlowCellStatus.Finalized)
				{
					continue;
				}
				IntVec3 c = glowLight.position + delta;
				if (c.x >= mapSize.x || c.z >= mapSize.z || c.x < 0 || c.z < 0)
				{
					continue;
				}
				bool flag = lightBlockers.IsSet(indices.CellToIndex(c));
				filler.blockers.Set(i, flag);
				if (flag)
				{
					continue;
				}
				int num4 = ((i < 4) ? 100 : 141);
				int num5 = filler.area[index].intDist + num4;
				if (num5 > num)
				{
					continue;
				}
				switch (i)
				{
				case 4:
					if (filler.blockers.IsSet(0) && filler.blockers.IsSet(1))
					{
						continue;
					}
					break;
				case 5:
					if (filler.blockers.IsSet(1) && filler.blockers.IsSet(2))
					{
						continue;
					}
					break;
				case 6:
					if (filler.blockers.IsSet(2) && filler.blockers.IsSet(3))
					{
						continue;
					}
					break;
				case 7:
					if (filler.blockers.IsSet(0) && filler.blockers.IsSet(3))
					{
						continue;
					}
					break;
				}
				if (value2.status == GlowCellStatus.Unvisited)
				{
					value2.intDist = int.MaxValue;
					value2.status = GlowCellStatus.Open;
					filler.area[num3] = value2;
				}
				if (num5 < value2.intDist)
				{
					value2.intDist = num5;
					value2.status = GlowCellStatus.Open;
					filler.area[num3] = value2;
					filler.queue.Insert(in delta);
				}
			}
		}
	}

	[BurstCompile]
	private void PrepareFill(ref GlowLight glowLight, ref GlowUniqueState filler, ref LocalGlowArea localArea)
	{
		NativeArrayUtility.MemClear(localArea.colors);
		int num = glowLight.DeltaToLocalIndex(new IntVec3(0, 0, 0));
		for (int i = 0; i < glowLight.diameter * glowLight.diameter; i++)
		{
			GlowCell value = default(GlowCell);
			if (i == num)
			{
				value.intDist = 100;
			}
			filler.area[i] = value;
		}
		filler.queue.Insert(in IntVec3.Zero);
	}

	[BurstCompile]
	private void SetGlowFromDist(ref GlowLight glowLight, ref GlowUniqueState filler, ref LocalGlowArea localArea, in IntVec3 delta)
	{
		float num = -1f / glowLight.glowRadius;
		int index = glowLight.DeltaToLocalIndex(in delta);
		float num2 = (float)filler.area[index].intDist / 100f;
		ColorInt colorInt = default(ColorInt);
		if (num2 <= glowLight.glowRadius)
		{
			float b = 1f / (num2 * num2);
			float num3 = Mathf.Lerp(1f + num * num2, b, 0.4f);
			colorInt = glowLight.glowColor * num3;
			colorInt.a = 0;
		}
		if (colorInt.r > 0 || colorInt.g > 0 || colorInt.b > 0)
		{
			colorInt.ClampToNonNegative();
			colorInt.a = (int)num2;
			colorInt.ProjectToColor32Fast(out var outColor);
			localArea.colors[index] = outColor;
		}
	}
}
