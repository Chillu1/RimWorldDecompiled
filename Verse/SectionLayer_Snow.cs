using System;
using System.Collections.Generic;
using RimWorld;
using Unity.Collections;
using UnityEngine;

namespace Verse;

[StaticConstructorOnStartup]
public class SectionLayer_Snow : SectionLayer
{
	private float[] adjValuesTmp = new float[9];

	private static readonly List<float> opacityListTmp = new List<float>();

	private static readonly CachedTexture PollutedSnowTex = new CachedTexture("Other/SnowPolluted");

	public static readonly List<List<int>> vertexWeights = new List<List<int>>
	{
		new List<int> { 0, 1, 2, 8 },
		new List<int> { 2, 8 },
		new List<int> { 2, 3, 4, 8 },
		new List<int> { 4, 8 },
		new List<int> { 4, 5, 6, 8 },
		new List<int> { 6, 8 },
		new List<int> { 6, 7, 0, 8 },
		new List<int> { 0, 8 },
		new List<int> { 8 }
	};

	public override bool Visible => DebugViewSettings.drawSnow;

	public SectionLayer_Snow(Section section)
		: base(section)
	{
		relevantChangeTypes = MapMeshFlagDefOf.Snow;
	}

	private bool Filled(int index)
	{
		Building building = base.Map.edificeGrid[index];
		if (building != null)
		{
			return building.def.Fillage == FillCategory.Full;
		}
		return false;
	}

	public override void Regenerate()
	{
		LayerSubMesh subMesh = GetSubMesh(MatBases.Snow);
		if (ModsConfig.BiotechActive)
		{
			subMesh.material.SetTexture(ShaderPropertyIDs.PollutedTex, PollutedSnowTex.Texture);
		}
		if (subMesh.mesh.vertexCount == 0)
		{
			SectionLayerGeometryMaker_Solid.MakeBaseGeometry(section, subMesh, AltitudeLayer.Terrain);
		}
		opacityListTmp.Clear();
		subMesh.Clear(MeshParts.Colors);
		NativeArray<float> depthGrid_Unsafe = base.Map.snowGrid.DepthGrid_Unsafe;
		CellRect cellRect = section.CellRect;
		bool flag = false;
		CellIndices cellIndices = base.Map.cellIndices;
		for (int i = cellRect.minX; i <= cellRect.maxX; i++)
		{
			for (int j = cellRect.minZ; j <= cellRect.maxZ; j++)
			{
				opacityListTmp.Clear();
				float num = depthGrid_Unsafe[cellIndices.CellToIndex(i, j)];
				for (int k = 0; k < 9; k++)
				{
					IntVec3 c = new IntVec3(i, 0, j) + GenAdj.AdjacentCellsAndInsideForUV[k];
					adjValuesTmp[k] = (c.InBounds(base.Map) ? depthGrid_Unsafe[cellIndices.CellToIndex(c)] : num);
				}
				for (int l = 0; l < 9; l++)
				{
					float num2 = 0f;
					for (int m = 0; m < vertexWeights[l].Count; m++)
					{
						num2 += adjValuesTmp[vertexWeights[l][m]];
					}
					num2 /= (float)vertexWeights[l].Count;
					if (num2 > 0.01f)
					{
						flag = true;
					}
					opacityListTmp.Add(num2);
				}
				for (int n = 0; n < 9; n++)
				{
					adjValuesTmp[n] = (base.Map.pollutionGrid.IsPolluted(new IntVec3(i, 0, j) + GenAdj.AdjacentCellsAndInsideForUV[n]) ? 1f : 0f);
				}
				for (int num3 = 0; num3 < 9; num3++)
				{
					float num4 = 0f;
					for (int num5 = 0; num5 < vertexWeights[num3].Count; num5++)
					{
						num4 += adjValuesTmp[vertexWeights[num3][num5]];
					}
					num4 /= (float)vertexWeights[num3].Count;
					float num6 = opacityListTmp[num3];
					subMesh.colors.Add(new Color32(Convert.ToByte(num4 * 255f), byte.MaxValue, byte.MaxValue, Convert.ToByte(num6 * 255f)));
				}
			}
		}
		if (flag)
		{
			subMesh.disabled = false;
			subMesh.FinalizeMesh(MeshParts.Colors);
		}
		else
		{
			subMesh.disabled = true;
		}
	}
}
