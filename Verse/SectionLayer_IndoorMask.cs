using System.Collections.Generic;
using RimWorld;
using UnityEngine;

namespace Verse;

public class SectionLayer_IndoorMask : SectionLayer
{
	public override bool Visible => DebugViewSettings.drawShadows;

	public SectionLayer_IndoorMask(Section section)
		: base(section)
	{
		relevantChangeTypes = (ulong)MapMeshFlagDefOf.Buildings | (ulong)MapMeshFlagDefOf.Roofs | (ulong)MapMeshFlagDefOf.FogOfWar;
	}

	public override void Regenerate()
	{
		CellRect cellRect = new CellRect(section.botLeft.x, section.botLeft.z, 17, 17);
		cellRect.ClipInsideMap(base.Map);
		LayerSubMesh subMesh = GetSubMesh(MatBases.IndoorMask);
		LayerSubMesh subMesh2 = GetSubMesh(MatBases.RoofedOutdoorMask);
		LayerSubMesh subMesh3 = GetSubMesh(MatBases.FilledMask);
		LayerSubMesh subMesh4 = GetSubMesh(MatBases.DebugOverlay);
		Map obj = base.Map;
		IEnumerable<IntVec3> cells = cellRect.Cells;
		int area = cellRect.Area;
		bool drawIndoorMask = DebugViewSettings.drawIndoorMask;
		bool drawOutdoorMask = DebugViewSettings.drawOutdoorMask;
		GenerateSectionLayer(obj, cells, area, subMesh, subMesh2, subMesh3, subMesh4, default(Vector3), drawIndoorMask, drawOutdoorMask);
	}

	public static LayerSubMesh BakeGravshipIndoorMesh(Map map, IEnumerable<IntVec3> cells, int cellCount, Material mat, Vector3 center = default(Vector3))
	{
		LayerSubMesh layerSubMesh = MapDrawLayer.CreateFreeSubMesh(mat);
		GenerateSectionLayer(map, cells, cellCount, layerSubMesh, layerSubMesh, null, null, -center);
		return layerSubMesh;
	}

	private static void GenerateSectionLayer(Map map, IEnumerable<IntVec3> cells, int cellCount, LayerSubMesh indoorMesh, LayerSubMesh outdoorMesh, LayerSubMesh filledMesh, LayerSubMesh debugMesh, Vector3 meshOffset = default(Vector3), bool debugIndoor = false, bool debugOutdoor = false)
	{
		ClearSubMesh(indoorMesh, cellCount);
		ClearSubMesh(outdoorMesh, cellCount);
		ClearSubMesh(filledMesh, cellCount);
		ClearSubMesh(debugMesh, cellCount);
		CellIndices cellIndices = map.cellIndices;
		foreach (IntVec3 cell in cells)
		{
			float x = (float)cell.x + meshOffset.x;
			float z = (float)cell.z + meshOffset.z;
			IntVec3 intVec = new IntVec3(cell.x, 0, cell.z);
			if (!intVec.InBounds(map))
			{
				continue;
			}
			bool flag = HideCommon(map, intVec);
			bool flag2 = HideRainFogOverlay(map, intVec);
			if (!flag && !flag2)
			{
				continue;
			}
			Building building = map.edificeGrid.InnerArray[cellIndices.CellToIndex(intVec.x, intVec.z)];
			float overage = ((building == null || (building.def.passability != Traversability.Impassable && !building.def.IsDoor)) ? 0.16f : 0f);
			if (flag)
			{
				Room room = intVec.GetRoom(map);
				if (room == null || !room.ProperRoom)
				{
					Building edifice = intVec.GetEdifice(map);
					RoofDef roof = intVec.GetRoof(map);
					if ((edifice == null || edifice.def.Fillage != FillCategory.Full) && (roof == null || !roof.isThickRoof))
					{
						AppendQuadToMesh(outdoorMesh, x, z, 0f);
						if (!debugIndoor && debugOutdoor)
						{
							AppendQuadToMesh(debugMesh, x, z, 0f);
						}
						continue;
					}
				}
			}
			AppendQuadToMesh(indoorMesh, x, z, overage);
			if (debugIndoor)
			{
				AppendQuadToMesh(debugMesh, x, z, overage);
			}
		}
		if (indoorMesh != null && !indoorMesh.finalized && indoorMesh.verts.Count > 0)
		{
			indoorMesh.FinalizeMesh(MeshParts.Verts | MeshParts.Tris);
		}
		if (outdoorMesh != null && !outdoorMesh.finalized && outdoorMesh.verts.Count > 0)
		{
			outdoorMesh.FinalizeMesh(MeshParts.Verts | MeshParts.Tris);
		}
		if (filledMesh != null && !filledMesh.finalized && filledMesh.verts.Count > 0)
		{
			filledMesh.FinalizeMesh(MeshParts.Verts | MeshParts.Tris);
		}
		if (debugMesh != null && !debugMesh.finalized && debugMesh.verts.Count > 0)
		{
			debugMesh.FinalizeMesh(MeshParts.Verts | MeshParts.Tris);
		}
	}

	private static bool HideRainFogOverlay(Map map, IntVec3 c)
	{
		Building edifice = c.GetEdifice(map);
		if (edifice?.def.building != null && edifice.def.building.isNaturalRock)
		{
			return true;
		}
		return false;
	}

	private static bool HideCommon(Map map, IntVec3 c)
	{
		if (map.fogGrid.IsFogged(c))
		{
			return true;
		}
		if (c.Roofed(map))
		{
			Building edifice = c.GetEdifice(map);
			if (edifice == null)
			{
				return true;
			}
			if (edifice.def.Fillage != FillCategory.Full)
			{
				return true;
			}
			if (edifice.def.size.x > 1 || edifice.def.size.z > 1)
			{
				return true;
			}
			if (edifice.def.holdsRoof)
			{
				return true;
			}
			if (edifice.def.blockWeather)
			{
				return true;
			}
		}
		return false;
	}

	private static void ClearSubMesh(LayerSubMesh mesh, int cellCount)
	{
		if (mesh != null)
		{
			mesh.Clear(MeshParts.All);
			if (DebugViewSettings.drawIndoorMask || DebugViewSettings.drawOutdoorMask)
			{
				mesh.verts.Capacity = cellCount * 2;
				mesh.tris.Capacity = cellCount * 4;
			}
		}
	}

	public static void AppendQuadToMesh(LayerSubMesh mesh, float x, float z, float overage)
	{
		if (mesh != null)
		{
			float y = AltitudeLayer.MetaOverlays.AltitudeFor();
			mesh.verts.Add(new Vector3(x - overage, y, z - overage));
			mesh.verts.Add(new Vector3(x - overage, y, z + 1f + overage));
			mesh.verts.Add(new Vector3(x + 1f + overage, y, z + 1f + overage));
			mesh.verts.Add(new Vector3(x + 1f + overage, y, z - overage));
			int count = mesh.verts.Count;
			mesh.tris.Add(count - 4);
			mesh.tris.Add(count - 3);
			mesh.tris.Add(count - 2);
			mesh.tris.Add(count - 4);
			mesh.tris.Add(count - 2);
			mesh.tris.Add(count - 1);
		}
	}
}
