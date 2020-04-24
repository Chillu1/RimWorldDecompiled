using RimWorld;
using System.Collections.Generic;
using UnityEngine;

namespace Verse
{
	internal class SectionLayer_Terrain : SectionLayer
	{
		private static readonly Color32 ColorWhite = new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);

		private static readonly Color32 ColorClear = new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, 0);

		public override bool Visible => DebugViewSettings.drawTerrain;

		public SectionLayer_Terrain(Section section)
			: base(section)
		{
			relevantChangeTypes = MapMeshFlag.Terrain;
		}

		public virtual Material GetMaterialFor(TerrainDef terrain)
		{
			return terrain.DrawMatSingle;
		}

		public bool AllowRenderingFor(TerrainDef terrain)
		{
			if (!DebugViewSettings.drawTerrainWater)
			{
				return !terrain.HasTag("Water");
			}
			return true;
		}

		public override void Regenerate()
		{
			ClearSubMeshes(MeshParts.All);
			TerrainGrid terrainGrid = base.Map.terrainGrid;
			CellRect cellRect = section.CellRect;
			TerrainDef[] array = new TerrainDef[8];
			HashSet<TerrainDef> hashSet = new HashSet<TerrainDef>();
			bool[] array2 = new bool[8];
			foreach (IntVec3 item in cellRect)
			{
				hashSet.Clear();
				TerrainDef terrainDef = terrainGrid.TerrainAt(item);
				LayerSubMesh subMesh = GetSubMesh(GetMaterialFor(terrainDef));
				if (subMesh != null && AllowRenderingFor(terrainDef))
				{
					int count = subMesh.verts.Count;
					subMesh.verts.Add(new Vector3(item.x, 0f, item.z));
					subMesh.verts.Add(new Vector3(item.x, 0f, item.z + 1));
					subMesh.verts.Add(new Vector3(item.x + 1, 0f, item.z + 1));
					subMesh.verts.Add(new Vector3(item.x + 1, 0f, item.z));
					subMesh.colors.Add(ColorWhite);
					subMesh.colors.Add(ColorWhite);
					subMesh.colors.Add(ColorWhite);
					subMesh.colors.Add(ColorWhite);
					subMesh.tris.Add(count);
					subMesh.tris.Add(count + 1);
					subMesh.tris.Add(count + 2);
					subMesh.tris.Add(count);
					subMesh.tris.Add(count + 2);
					subMesh.tris.Add(count + 3);
				}
				for (int i = 0; i < 8; i++)
				{
					IntVec3 c = item + GenAdj.AdjacentCellsAroundBottom[i];
					if (!c.InBounds(base.Map))
					{
						array[i] = terrainDef;
					}
					else
					{
						TerrainDef terrainDef2 = terrainGrid.TerrainAt(c);
						Thing edifice = c.GetEdifice(base.Map);
						if (edifice != null && edifice.def.coversFloor)
						{
							terrainDef2 = TerrainDefOf.Underwall;
						}
						array[i] = terrainDef2;
						if (terrainDef2 != terrainDef && terrainDef2.edgeType != 0 && terrainDef2.renderPrecedence >= terrainDef.renderPrecedence && !hashSet.Contains(terrainDef2))
						{
							hashSet.Add(terrainDef2);
						}
					}
				}
				foreach (TerrainDef item2 in hashSet)
				{
					LayerSubMesh subMesh2 = GetSubMesh(GetMaterialFor(item2));
					if (subMesh2 != null && AllowRenderingFor(item2))
					{
						int count = subMesh2.verts.Count;
						subMesh2.verts.Add(new Vector3((float)item.x + 0.5f, 0f, item.z));
						subMesh2.verts.Add(new Vector3(item.x, 0f, item.z));
						subMesh2.verts.Add(new Vector3(item.x, 0f, (float)item.z + 0.5f));
						subMesh2.verts.Add(new Vector3(item.x, 0f, item.z + 1));
						subMesh2.verts.Add(new Vector3((float)item.x + 0.5f, 0f, item.z + 1));
						subMesh2.verts.Add(new Vector3(item.x + 1, 0f, item.z + 1));
						subMesh2.verts.Add(new Vector3(item.x + 1, 0f, (float)item.z + 0.5f));
						subMesh2.verts.Add(new Vector3(item.x + 1, 0f, item.z));
						subMesh2.verts.Add(new Vector3((float)item.x + 0.5f, 0f, (float)item.z + 0.5f));
						for (int j = 0; j < 8; j++)
						{
							array2[j] = false;
						}
						for (int k = 0; k < 8; k++)
						{
							if (k % 2 == 0)
							{
								if (array[k] == item2)
								{
									array2[(k - 1 + 8) % 8] = true;
									array2[k] = true;
									array2[(k + 1) % 8] = true;
								}
							}
							else if (array[k] == item2)
							{
								array2[k] = true;
							}
						}
						for (int l = 0; l < 8; l++)
						{
							if (array2[l])
							{
								subMesh2.colors.Add(ColorWhite);
							}
							else
							{
								subMesh2.colors.Add(ColorClear);
							}
						}
						subMesh2.colors.Add(ColorClear);
						for (int m = 0; m < 8; m++)
						{
							subMesh2.tris.Add(count + m);
							subMesh2.tris.Add(count + (m + 1) % 8);
							subMesh2.tris.Add(count + 8);
						}
					}
				}
			}
			FinalizeMesh(MeshParts.All);
		}
	}
}
