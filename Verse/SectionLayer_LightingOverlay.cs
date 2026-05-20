using System;
using System.Text;
using RimWorld;
using UnityEngine;

namespace Verse;

[StaticConstructorOnStartup]
public class SectionLayer_LightingOverlay : SectionLayer
{
	private int firstCenterInd;

	private CellRect sectRect;

	private const byte RoofedAreaMinSkyCover = 100;

	public override bool Visible => DebugViewSettings.drawLightingOverlay;

	public SectionLayer_LightingOverlay(Section section)
		: base(section)
	{
		relevantChangeTypes = (ulong)MapMeshFlagDefOf.Roofs | (ulong)MapMeshFlagDefOf.GroundGlow;
	}

	public string GlowReportAt(IntVec3 c)
	{
		Color32[] colors = GetSubMesh(MatBases.LightOverlay).mesh.colors32;
		CalculateVertexIndices(sectRect, c.x, c.z, firstCenterInd, out var botLeft, out var topLeft, out var topRight, out var botRight, out var center);
		StringBuilder stringBuilder = new StringBuilder();
		Color32 color = colors[botLeft];
		stringBuilder.Append("BL=" + color.ToString());
		color = colors[topLeft];
		stringBuilder.Append("\nTL=" + color.ToString());
		color = colors[topRight];
		stringBuilder.Append("\nTR=" + color.ToString());
		color = colors[botRight];
		stringBuilder.Append("\nBR=" + color.ToString());
		color = colors[center];
		stringBuilder.Append("\nCenter=" + color.ToString());
		return stringBuilder.ToString();
	}

	public static LayerSubMesh Bake(Map map, CellRect sectRect, Material mat, Predicate<int> filter)
	{
		LayerSubMesh layerSubMesh = MapDrawLayer.CreateFreeSubMesh(mat ?? MatBases.LightOverlay, map);
		int num = 0;
		GenerateLightingOverlay(map, layerSubMesh, sectRect, ref num, centered: true, filter);
		return layerSubMesh;
	}

	public override void Regenerate()
	{
		LayerSubMesh subMesh = GetSubMesh(MatBases.LightOverlay);
		if (subMesh.verts.Count == 0)
		{
			sectRect = new CellRect(section.botLeft.x, section.botLeft.z, 17, 17);
			sectRect.ClipInsideMap(base.Map);
		}
		GenerateLightingOverlay(base.Map, subMesh, sectRect, ref firstCenterInd);
	}

	private static void GenerateLightingOverlay(Map map, LayerSubMesh subMesh, CellRect rect, ref int firstCenterInd, bool centered = false, Predicate<int> filter = null)
	{
		rect.ClipInsideMap(map);
		if (subMesh.verts.Count == 0)
		{
			MakeBaseGeometry(map, subMesh, rect, out firstCenterInd, centered);
		}
		Color32[] array = new Color32[subMesh.verts.Count];
		int maxX = rect.maxX;
		int maxZ = rect.maxZ;
		int width = rect.Width;
		int x = map.Size.x;
		Thing[] innerArray = map.edificeGrid.InnerArray;
		Thing[] array2 = innerArray;
		int num = array2.Length;
		RoofGrid roofGrid = map.roofGrid;
		CellIndices cellIndices = map.cellIndices;
		CalculateVertexIndices(rect, firstCenterInd, rect.minX, rect.minZ, out var botLeft, out var _, out var topRight, out var botRight, out var center);
		int num2 = cellIndices.CellToIndex(new IntVec3(rect.minX, 0, rect.minZ));
		int[] array3 = new int[4]
		{
			-map.Size.x - 1,
			-map.Size.x,
			-1,
			0
		};
		int[] array4 = new int[4] { -1, -1, 0, 0 };
		for (int i = rect.minZ; i <= maxZ + 1; i++)
		{
			int num3 = num2 / x;
			int num4 = rect.minX;
			while (num4 <= maxX + 1)
			{
				if (filter != null && !filter(num2))
				{
					array[botLeft] = new Color32(0, 0, 0, 0);
				}
				else
				{
					ColorInt colorInt = new ColorInt(0, 0, 0, 0);
					int num5 = 0;
					bool flag = false;
					for (int j = 0; j < 4; j++)
					{
						int num6 = num2 + array3[j];
						if (num6 >= 0 && num6 < num && num6 / x == num3 + array4[j])
						{
							Thing thing = array2[num6];
							RoofDef roofDef = roofGrid.RoofAt(num6);
							if (roofDef != null && (roofDef.isThickRoof || thing == null || !thing.def.holdsRoof || thing.def.altitudeLayer == AltitudeLayer.DoorMoveable))
							{
								flag = true;
							}
							if (thing == null || !thing.def.blockLight)
							{
								colorInt += map.glowGrid.VisualGlowAt(num6);
								num5++;
							}
						}
					}
					if (num5 > 0)
					{
						array[botLeft] = (colorInt / num5).ProjectToColor32();
					}
					else
					{
						array[botLeft] = new Color32(0, 0, 0, 0);
					}
					if (flag && array[botLeft].a < 100)
					{
						array[botLeft].a = 100;
					}
				}
				num4++;
				botLeft++;
				num2++;
			}
			int num7 = maxX + 2 - rect.minX;
			botLeft -= num7;
			num2 -= num7;
			botLeft += width + 1;
			num2 += map.Size.x;
		}
		CalculateVertexIndices(rect, firstCenterInd, rect.minX, rect.minZ, out var botLeft2, out center, out botRight, out topRight, out var center2);
		int num8 = cellIndices.CellToIndex(rect.minX, rect.minZ);
		for (int k = rect.minZ; k <= maxZ; k++)
		{
			int num9 = rect.minX;
			while (num9 <= maxX)
			{
				if (filter != null && !filter(num8))
				{
					array[center2] = new Color32(0, 0, 0, 0);
				}
				else
				{
					ColorInt colorInt2 = default(ColorInt);
					colorInt2 += array[botLeft2];
					colorInt2 += array[botLeft2 + 1];
					colorInt2 += array[botLeft2 + width + 1];
					colorInt2 += array[botLeft2 + width + 2];
					array[center2] = new Color32((byte)(colorInt2.r / 4), (byte)(colorInt2.g / 4), (byte)(colorInt2.b / 4), (byte)(colorInt2.a / 4));
					if (array[center2].a < 100 && roofGrid.Roofed(num8))
					{
						Thing thing2 = array2[num8];
						if (thing2 == null || !thing2.def.holdsRoof)
						{
							array[center2].a = 100;
						}
					}
				}
				num9++;
				botLeft2++;
				center2++;
				num8++;
			}
			botLeft2++;
			num8 -= width;
			num8 += map.Size.x;
		}
		subMesh.mesh.colors32 = array;
	}

	private static void MakeBaseGeometry(Map map, LayerSubMesh sm, CellRect sectRect, out int firstCenterInd, bool centered = false)
	{
		sectRect.ClipInsideMap(map);
		float num = (centered ? ((float)(-sectRect.minX) - (float)sectRect.Width / 2f) : 0f);
		float num2 = (centered ? ((float)(-sectRect.minZ) - (float)sectRect.Height / 2f) : 0f);
		int capacity = (sectRect.Width + 1) * (sectRect.Height + 1) + sectRect.Area;
		float y = AltitudeLayer.LightingOverlay.AltitudeFor();
		sm.verts.Capacity = capacity;
		for (int i = sectRect.minZ; i <= sectRect.maxZ + 1; i++)
		{
			for (int j = sectRect.minX; j <= sectRect.maxX + 1; j++)
			{
				sm.verts.Add(new Vector3((float)j + num, y, (float)i + num2));
			}
		}
		firstCenterInd = sm.verts.Count;
		for (int k = sectRect.minZ; k <= sectRect.maxZ; k++)
		{
			for (int l = sectRect.minX; l <= sectRect.maxX; l++)
			{
				sm.verts.Add(new Vector3((float)l + num + 0.5f, y, (float)k + num2 + 0.5f));
			}
		}
		sm.tris.Capacity = sectRect.Area * 4 * 3;
		for (int m = sectRect.minZ; m <= sectRect.maxZ; m++)
		{
			for (int n = sectRect.minX; n <= sectRect.maxX; n++)
			{
				CalculateVertexIndices(sectRect, firstCenterInd, n, m, out var botLeft, out var topLeft, out var topRight, out var botRight, out var center);
				sm.tris.Add(botLeft);
				sm.tris.Add(center);
				sm.tris.Add(botRight);
				sm.tris.Add(botLeft);
				sm.tris.Add(topLeft);
				sm.tris.Add(center);
				sm.tris.Add(topLeft);
				sm.tris.Add(topRight);
				sm.tris.Add(center);
				sm.tris.Add(topRight);
				sm.tris.Add(botRight);
				sm.tris.Add(center);
			}
		}
		sm.FinalizeMesh(MeshParts.Verts | MeshParts.Tris);
	}

	private static void CalculateVertexIndices(CellRect sectRect, int firstCenterInd, int worldX, int worldZ, out int botLeft, out int topLeft, out int topRight, out int botRight, out int center)
	{
		int num = worldX - sectRect.minX;
		int num2 = worldZ - sectRect.minZ;
		botLeft = num2 * (sectRect.Width + 1) + num;
		topLeft = (num2 + 1) * (sectRect.Width + 1) + num;
		topRight = (num2 + 1) * (sectRect.Width + 1) + (num + 1);
		botRight = num2 * (sectRect.Width + 1) + (num + 1);
		center = firstCenterInd + (num2 * sectRect.Width + num);
	}
}
