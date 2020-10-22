using System.Text;
using UnityEngine;

namespace Verse
{
	public class SectionLayer_LightingOverlay : SectionLayer
	{
		private Color32[] glowGrid;

		private int firstCenterInd;

		private CellRect sectRect;

		private const byte RoofedAreaMinSkyCover = 100;

		public override bool Visible => DebugViewSettings.drawLightingOverlay;

		public SectionLayer_LightingOverlay(Section section)
			: base(section)
		{
			relevantChangeTypes = MapMeshFlag.GroundGlow;
		}

		public string GlowReportAt(IntVec3 c)
		{
			Color32[] colors = GetSubMesh(MatBases.LightOverlay).mesh.colors32;
			CalculateVertexIndices(c.x, c.z, out var botLeft, out var topLeft, out var topRight, out var botRight, out var center);
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("BL=" + colors[botLeft]);
			stringBuilder.Append("\nTL=" + colors[topLeft]);
			stringBuilder.Append("\nTR=" + colors[topRight]);
			stringBuilder.Append("\nBR=" + colors[botRight]);
			stringBuilder.Append("\nCenter=" + colors[center]);
			return stringBuilder.ToString();
		}

		public override void Regenerate()
		{
			LayerSubMesh subMesh = GetSubMesh(MatBases.LightOverlay);
			if (subMesh.verts.Count == 0)
			{
				MakeBaseGeometry(subMesh);
			}
			Color32[] array = new Color32[subMesh.verts.Count];
			int maxX = sectRect.maxX;
			int maxZ = sectRect.maxZ;
			int width = sectRect.Width;
			Map map = base.Map;
			int x = map.Size.x;
			Thing[] innerArray = map.edificeGrid.InnerArray;
			Thing[] array2 = innerArray;
			int num = array2.Length;
			RoofGrid roofGrid = map.roofGrid;
			CellIndices cellIndices = map.cellIndices;
			CalculateVertexIndices(sectRect.minX, sectRect.minZ, out var botLeft, out var _, out var _, out var _, out var _);
			int num2 = cellIndices.CellToIndex(new IntVec3(sectRect.minX, 0, sectRect.minZ));
			int[] array3 = new int[4]
			{
				-map.Size.x - 1,
				-map.Size.x,
				-1,
				0
			};
			int[] array4 = new int[4]
			{
				-1,
				-1,
				0,
				0
			};
			for (int i = sectRect.minZ; i <= maxZ + 1; i++)
			{
				int num3 = num2 / x;
				int num4 = sectRect.minX;
				while (num4 <= maxX + 1)
				{
					ColorInt a = new ColorInt(0, 0, 0, 0);
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
								a += glowGrid[num6];
								num5++;
							}
						}
					}
					if (num5 > 0)
					{
						array[botLeft] = (a / num5).ToColor32;
					}
					else
					{
						array[botLeft] = new Color32(0, 0, 0, 0);
					}
					if (flag && array[botLeft].a < 100)
					{
						array[botLeft].a = 100;
					}
					num4++;
					botLeft++;
					num2++;
				}
				int num7 = maxX + 2 - sectRect.minX;
				botLeft -= num7;
				num2 -= num7;
				botLeft += width + 1;
				num2 += map.Size.x;
			}
			CalculateVertexIndices(sectRect.minX, sectRect.minZ, out var botLeft2, out var _, out var _, out var _, out var center2);
			int num8 = cellIndices.CellToIndex(sectRect.minX, sectRect.minZ);
			for (int k = sectRect.minZ; k <= maxZ; k++)
			{
				int num9 = sectRect.minX;
				while (num9 <= maxX)
				{
					ColorInt colorInt = default(ColorInt);
					colorInt += array[botLeft2];
					colorInt += array[botLeft2 + 1];
					colorInt += array[botLeft2 + width + 1];
					colorInt += array[botLeft2 + width + 2];
					array[center2] = new Color32((byte)(colorInt.r / 4), (byte)(colorInt.g / 4), (byte)(colorInt.b / 4), (byte)(colorInt.a / 4));
					if (array[center2].a < 100 && roofGrid.Roofed(num8))
					{
						Thing thing2 = array2[num8];
						if (thing2 == null || !thing2.def.holdsRoof)
						{
							array[center2].a = 100;
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

		private void MakeBaseGeometry(LayerSubMesh sm)
		{
			glowGrid = base.Map.glowGrid.glowGrid;
			sectRect = new CellRect(section.botLeft.x, section.botLeft.z, 17, 17);
			sectRect.ClipInsideMap(base.Map);
			int capacity = (sectRect.Width + 1) * (sectRect.Height + 1) + sectRect.Area;
			float y = AltitudeLayer.LightingOverlay.AltitudeFor();
			sm.verts.Capacity = capacity;
			for (int i = sectRect.minZ; i <= sectRect.maxZ + 1; i++)
			{
				for (int j = sectRect.minX; j <= sectRect.maxX + 1; j++)
				{
					sm.verts.Add(new Vector3(j, y, i));
				}
			}
			firstCenterInd = sm.verts.Count;
			for (int k = sectRect.minZ; k <= sectRect.maxZ; k++)
			{
				for (int l = sectRect.minX; l <= sectRect.maxX; l++)
				{
					sm.verts.Add(new Vector3((float)l + 0.5f, y, (float)k + 0.5f));
				}
			}
			sm.tris.Capacity = sectRect.Area * 4 * 3;
			for (int m = sectRect.minZ; m <= sectRect.maxZ; m++)
			{
				for (int n = sectRect.minX; n <= sectRect.maxX; n++)
				{
					CalculateVertexIndices(n, m, out var botLeft, out var topLeft, out var topRight, out var botRight, out var center);
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

		private void CalculateVertexIndices(int worldX, int worldZ, out int botLeft, out int topLeft, out int topRight, out int botRight, out int center)
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
}
