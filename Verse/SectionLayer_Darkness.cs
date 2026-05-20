using System.Collections.Generic;
using RimWorld;
using UnityEngine;

namespace Verse;

public class SectionLayer_Darkness : SectionLayer
{
	private List<float>[] vertLight = new List<float>[9];

	private MaterialPropertyBlock propertyBlock;

	private float[] cachedLight;

	private const float MaxDarkness = 1f;

	public override bool Visible => base.Map.gameConditionManager.DarknessVisible;

	public SectionLayer_Darkness(Section section)
		: base(section)
	{
		relevantChangeTypes = MapMeshFlagDefOf.GroundGlow;
		propertyBlock = new MaterialPropertyBlock();
		cachedLight = new float[section.CellRect.ExpandedBy(1).Area];
		for (int i = 0; i < 9; i++)
		{
			vertLight[i] = new List<float>(4);
		}
	}

	public override void DrawLayer()
	{
		if (!Visible)
		{
			return;
		}
		float a = Mathf.Clamp01(1f - Mathf.Max(base.Map.gameConditionManager.MapBrightness, base.Map.skyManager.CurSkyGlow));
		propertyBlock.SetColor(ShaderPropertyIDs.ColorTwo, new Color(1f, 1f, 1f, a));
		propertyBlock.SetFloat(ShaderPropertyIDs.AgeSecsPausable, RealTime.UnpausedRealTime);
		int count = subMeshes.Count;
		for (int i = 0; i < count; i++)
		{
			LayerSubMesh layerSubMesh = subMeshes[i];
			if (layerSubMesh.finalized && !layerSubMesh.disabled)
			{
				Graphics.DrawMesh(layerSubMesh.mesh, Vector3.zero, Quaternion.identity, layerSubMesh.material, 0, null, 0, propertyBlock);
			}
		}
	}

	public override void Regenerate()
	{
		if (!ModsConfig.AnomalyActive || !Visible)
		{
			return;
		}
		LayerSubMesh subMesh = GetSubMesh(MatBases.Darkness);
		if (subMesh.mesh.vertexCount == 0)
		{
			SectionLayerGeometryMaker_Solid.MakeBaseGeometry(section, subMesh, AltitudeLayer.Darkness);
		}
		subMesh.Clear(MeshParts.Colors);
		CellRect cellRect = section.CellRect;
		int num = cellRect.Width + 2;
		int num2 = base.Map.Size.z - 1;
		int num3 = base.Map.Size.x - 1;
		for (int i = cellRect.minX - 1; i <= cellRect.maxX + 1; i++)
		{
			for (int j = cellRect.minZ - 1; j <= cellRect.maxZ + 1; j++)
			{
				int num4 = i - cellRect.minX + 1;
				int num5 = j - cellRect.minZ + 1;
				cachedLight[num5 * num + num4] = LightAt(i, j);
			}
		}
		for (int k = cellRect.minX; k <= cellRect.maxX; k++)
		{
			for (int l = cellRect.minZ; l <= cellRect.maxZ; l++)
			{
				int num6 = k - cellRect.minX + 1;
				int num7 = l - cellRect.minZ + 1;
				float item = cachedLight[num7 * num + num6];
				for (int m = 0; m < 9; m++)
				{
					vertLight[m].Clear();
					vertLight[m].Add(item);
				}
				if (l < num2)
				{
					item = cachedLight[(num7 + 1) * num + num6];
					vertLight[2].Add(item);
					vertLight[3].Add(item);
					vertLight[4].Add(item);
				}
				if (l > 0)
				{
					item = cachedLight[(num7 - 1) * num + num6];
					vertLight[6].Add(item);
					vertLight[7].Add(item);
					vertLight[0].Add(item);
				}
				if (k < num3)
				{
					item = cachedLight[num7 * num + (num6 + 1)];
					vertLight[4].Add(item);
					vertLight[5].Add(item);
					vertLight[6].Add(item);
				}
				if (k > 0)
				{
					item = cachedLight[num7 * num + (num6 - 1)];
					vertLight[0].Add(item);
					vertLight[1].Add(item);
					vertLight[2].Add(item);
				}
				if (l > 0 && k > 0)
				{
					vertLight[0].Add(cachedLight[(num7 - 1) * num + (num6 - 1)]);
				}
				if (l < num2 && k > 0)
				{
					vertLight[2].Add(cachedLight[(num7 + 1) * num + (num6 - 1)]);
				}
				if (l < num2 && k < num3)
				{
					vertLight[4].Add(cachedLight[(num7 + 1) * num + (num6 + 1)]);
				}
				if (l > 0 && k < num3)
				{
					vertLight[6].Add(cachedLight[(num7 - 1) * num + (num6 + 1)]);
				}
				for (int n = 0; n < 9; n++)
				{
					float num8 = 0f;
					if (vertLight[n].Count > 0)
					{
						for (int num9 = 0; num9 < vertLight[n].Count; num9++)
						{
							num8 += vertLight[n][num9];
						}
						num8 /= (float)vertLight[n].Count;
						num8 = 1f - num8;
					}
					subMesh.colors.Add(new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, (byte)(Mathf.Min(num8, 1f) * 255f)));
				}
			}
		}
		subMesh.FinalizeMesh(MeshParts.Colors);
	}

	private float LightAt(int x, int z)
	{
		IntVec3 intVec = new IntVec3(x, 0, z);
		if (!intVec.InBounds(base.Map) || base.Map.fogGrid.IsFogged(intVec))
		{
			return 0f;
		}
		if (LightBlockingEdificeAt(intVec))
		{
			float num = base.Map.glowGrid.GroundGlowAt(intVec, ignoreCavePlants: false, ignoreSky: true);
			for (int i = 0; i < 8; i++)
			{
				IntVec3 c = intVec + GenAdj.AdjacentCells[i];
				if (c.InBounds(base.Map) && !base.Map.fogGrid.IsFogged(c) && !LightBlockingEdificeAt(c))
				{
					num = Mathf.Max(num, base.Map.glowGrid.GroundGlowAt(c, ignoreCavePlants: false, ignoreSky: true));
				}
			}
			return num;
		}
		return base.Map.glowGrid.GroundGlowAt(intVec, ignoreCavePlants: false, ignoreSky: true);
	}

	private bool LightBlockingEdificeAt(IntVec3 c)
	{
		return base.Map.edificeGrid[c]?.def.blockLight ?? false;
	}
}
