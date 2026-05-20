using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld;

public class SectionLayer_TerrainEdges : SectionLayer
{
	[Flags]
	private enum EdgeDirections
	{
		None = 0,
		North = 1,
		East = 2,
		South = 4,
		West = 8
	}

	[Flags]
	private enum CornerDirections
	{
		None = 0,
		SouthWest = 1,
		NorthWest = 2,
		NorthEast = 4,
		SouthEast = 8
	}

	public enum EdgeType
	{
		OShape,
		UShape,
		CornerInner,
		CornerOuter,
		Flat,
		LoopLeft,
		LoopRight,
		LoopSingle,
		Loop
	}

	private static readonly Vector2[] UVs = new Vector2[4]
	{
		new Vector2(0f, 0f),
		new Vector2(0f, 1f),
		new Vector2(1f, 1f),
		new Vector2(1f, 0f)
	};

	private static readonly Dictionary<EdgeDirections, (EdgeType, Rot4)[]> EdgeMats = new Dictionary<EdgeDirections, (EdgeType, Rot4)[]>
	{
		{
			EdgeDirections.North,
			new(EdgeType, Rot4)[1] { (EdgeType.Flat, Rot4.South) }
		},
		{
			EdgeDirections.East,
			new(EdgeType, Rot4)[1] { (EdgeType.Flat, Rot4.West) }
		},
		{
			EdgeDirections.South,
			new(EdgeType, Rot4)[1] { (EdgeType.Flat, Rot4.North) }
		},
		{
			EdgeDirections.West,
			new(EdgeType, Rot4)[1] { (EdgeType.Flat, Rot4.East) }
		},
		{
			EdgeDirections.North | EdgeDirections.East,
			new(EdgeType, Rot4)[1] { (EdgeType.CornerOuter, Rot4.West) }
		},
		{
			EdgeDirections.East | EdgeDirections.South,
			new(EdgeType, Rot4)[1] { (EdgeType.CornerOuter, Rot4.North) }
		},
		{
			EdgeDirections.South | EdgeDirections.West,
			new(EdgeType, Rot4)[1] { (EdgeType.CornerOuter, Rot4.East) }
		},
		{
			EdgeDirections.North | EdgeDirections.West,
			new(EdgeType, Rot4)[1] { (EdgeType.CornerOuter, Rot4.South) }
		},
		{
			EdgeDirections.North | EdgeDirections.South,
			new(EdgeType, Rot4)[2]
			{
				(EdgeType.Flat, Rot4.South),
				(EdgeType.Flat, Rot4.North)
			}
		},
		{
			EdgeDirections.East | EdgeDirections.West,
			new(EdgeType, Rot4)[2]
			{
				(EdgeType.Flat, Rot4.West),
				(EdgeType.Flat, Rot4.East)
			}
		},
		{
			EdgeDirections.North | EdgeDirections.East | EdgeDirections.South,
			new(EdgeType, Rot4)[1] { (EdgeType.UShape, Rot4.West) }
		},
		{
			EdgeDirections.East | EdgeDirections.South | EdgeDirections.West,
			new(EdgeType, Rot4)[1] { (EdgeType.UShape, Rot4.North) }
		},
		{
			EdgeDirections.North | EdgeDirections.South | EdgeDirections.West,
			new(EdgeType, Rot4)[1] { (EdgeType.UShape, Rot4.East) }
		},
		{
			EdgeDirections.North | EdgeDirections.East | EdgeDirections.West,
			new(EdgeType, Rot4)[1] { (EdgeType.UShape, Rot4.South) }
		},
		{
			EdgeDirections.North | EdgeDirections.East | EdgeDirections.South | EdgeDirections.West,
			new(EdgeType, Rot4)[1] { (EdgeType.OShape, Rot4.North) }
		}
	};

	public override bool Visible => DebugViewSettings.drawTerrain;

	public SectionLayer_TerrainEdges(Section section)
		: base(section)
	{
		relevantChangeTypes = MapMeshFlagDefOf.Terrain;
	}

	public override void Regenerate()
	{
		if (base.Map.Tile.Valid && !base.Map.Tile.LayerDef.isSpace)
		{
			return;
		}
		ClearSubMeshes(MeshParts.All);
		TerrainGrid terrainGrid = base.Map.terrainGrid;
		CellRect cellRect = section.CellRect;
		float altitude = AltitudeLayer.TerrainScatter.AltitudeFor();
		float altitude2 = AltitudeLayer.TerrainEdges.AltitudeFor();
		foreach (IntVec3 item in cellRect)
		{
			if (ShouldDrawRockEdges(item, terrainGrid, out var edges, out var corners))
			{
				TerrainDef terrain = terrainGrid.BaseTerrainAt(item);
				DrawEdges(terrain, item, edges, altitude);
				DrawCorners(terrain, item, edges, corners, altitude);
				if (ShouldDrawPassthrough(item, terrainGrid, out edges, out corners))
				{
					DrawLoop(item + IntVec3.South, terrainGrid, edges, corners, altitude2);
				}
			}
			else if (ShouldDrawLoop(item, terrainGrid, out edges, out corners))
			{
				DrawLoop(item, terrainGrid, edges, corners, altitude2);
			}
		}
		FinalizeMesh(MeshParts.All);
	}

	private void DrawEdges(TerrainDef terrain, IntVec3 c, EdgeDirections edgeDirs, float altitude)
	{
		if (EdgeMats.TryGetValue(edgeDirs, out var value))
		{
			for (int i = 0; i < value.Length; i++)
			{
				var (edgeType, rotation) = value[i];
				AddQuad(terrain, edgeType, c, altitude, rotation);
			}
		}
	}

	private void DrawCorners(TerrainDef terrain, IntVec3 c, EdgeDirections edges, CornerDirections corners, float altitude)
	{
		if (corners.HasFlag(CornerDirections.NorthWest) && !edges.HasFlag(EdgeDirections.North) && !edges.HasFlag(EdgeDirections.West))
		{
			AddQuad(terrain, EdgeType.CornerInner, c, altitude, Rot4.East);
		}
		if (corners.HasFlag(CornerDirections.NorthEast) && !edges.HasFlag(EdgeDirections.North) && !edges.HasFlag(EdgeDirections.East))
		{
			AddQuad(terrain, EdgeType.CornerInner, c, altitude, Rot4.South);
		}
		if (corners.HasFlag(CornerDirections.SouthEast) && !edges.HasFlag(EdgeDirections.South) && !edges.HasFlag(EdgeDirections.East))
		{
			AddQuad(terrain, EdgeType.CornerInner, c, altitude, Rot4.West);
		}
		if (corners.HasFlag(CornerDirections.SouthWest) && !edges.HasFlag(EdgeDirections.South) && !edges.HasFlag(EdgeDirections.West))
		{
			AddQuad(terrain, EdgeType.CornerInner, c, altitude, Rot4.North);
		}
	}

	private void DrawLoop(IntVec3 c, TerrainGrid grid, EdgeDirections edges, CornerDirections corners, float altitude)
	{
		if (edges.HasFlag(EdgeDirections.North))
		{
			TerrainDef terrain = grid.BaseTerrainAt(c + IntVec3.North);
			float num = (float)c.z / (float)base.Map.Size.z;
			altitude += 0.03658537f - num * 0.03658537f;
			if (!corners.HasFlag(CornerDirections.NorthWest) && !corners.HasFlag(CornerDirections.NorthEast))
			{
				AddQuad(terrain, EdgeType.LoopSingle, c + IntVec3.South, altitude, Rot4.North);
			}
			if (corners.HasFlag(CornerDirections.NorthWest) && corners.HasFlag(CornerDirections.NorthEast))
			{
				AddQuad(terrain, EdgeType.Loop, c + IntVec3.South, altitude, Rot4.North, c.GetHashCode());
			}
			if (!corners.HasFlag(CornerDirections.NorthWest) && corners.HasFlag(CornerDirections.NorthEast))
			{
				AddQuad(terrain, EdgeType.LoopLeft, c + IntVec3.South, altitude, Rot4.North);
			}
			if (!corners.HasFlag(CornerDirections.NorthEast) && corners.HasFlag(CornerDirections.NorthWest))
			{
				AddQuad(terrain, EdgeType.LoopRight, c + IntVec3.South, altitude, Rot4.North);
			}
		}
	}

	private void AddQuad(TerrainDef terrain, EdgeType edgeType, IntVec3 c, float altitude, Rot4 rotation, int listIndexOffset = 0)
	{
		Material material = terrain.spaceEdgeGraphicData.GetMaterial(terrain, edgeType, listIndexOffset);
		LayerSubMesh subMesh = GetSubMesh(material);
		int count = subMesh.verts.Count;
		float num = Mathf.Max((float)material.mainTexture.width / (float)material.mainTexture.height, 1f);
		float num2 = Mathf.Max((float)material.mainTexture.height / (float)material.mainTexture.width, 1f);
		int num3 = Mathf.Abs(4 - rotation.AsInt);
		for (int i = 0; i < 4; i++)
		{
			subMesh.verts.Add(new Vector3((float)c.x + UVs[i].x * num, altitude, (float)c.z + UVs[i].y * num2));
			subMesh.uvs.Add(UVs[(num3 + i) % 4]);
		}
		subMesh.tris.Add(count);
		subMesh.tris.Add(count + 1);
		subMesh.tris.Add(count + 2);
		subMesh.tris.Add(count);
		subMesh.tris.Add(count + 2);
		subMesh.tris.Add(count + 3);
	}

	private bool ShouldDrawRockEdges(IntVec3 c, TerrainGrid grid, out EdgeDirections edges, out CornerDirections corners)
	{
		edges = EdgeDirections.None;
		corners = CornerDirections.None;
		TerrainDef terrainDef = grid.BaseTerrainAt(c);
		if (terrainDef == null || terrainDef.spaceEdgeGraphicData == null)
		{
			return false;
		}
		for (int i = 0; i < GenAdj.CardinalDirections.Length; i++)
		{
			IntVec3 c2 = c + GenAdj.CardinalDirections[i];
			if (!c2.InBounds(base.Map))
			{
				if (!base.Map.DrawMapClippers)
				{
					edges |= (EdgeDirections)(1 << i);
				}
				continue;
			}
			TerrainDef terrainDef2 = grid.TerrainAt(c2);
			if (terrainDef2 == null || terrainDef2.dontRender)
			{
				edges |= (EdgeDirections)(1 << i);
			}
		}
		for (int j = 0; j < GenAdj.DiagonalDirections.Length; j++)
		{
			IntVec3 c3 = c + GenAdj.DiagonalDirections[j];
			if (!c3.InBounds(base.Map))
			{
				if (!base.Map.DrawMapClippers)
				{
					edges |= (EdgeDirections)(1 << j);
				}
				continue;
			}
			TerrainDef terrainDef3 = grid.TerrainAt(c3);
			if (terrainDef3 == null || terrainDef3.dontRender)
			{
				corners |= (CornerDirections)(1 << j);
			}
		}
		if (edges == EdgeDirections.None)
		{
			return corners != CornerDirections.None;
		}
		return true;
	}

	private bool ShouldDrawPassthrough(IntVec3 c, TerrainGrid grid, out EdgeDirections edges, out CornerDirections corners)
	{
		edges = EdgeDirections.None;
		corners = CornerDirections.None;
		IntVec3 c2 = c + IntVec3.North;
		if (!c2.InBounds(base.Map))
		{
			return false;
		}
		TerrainDef terrainDef = grid.BaseTerrainAt(c2);
		if (terrainDef == null || terrainDef.spaceEdgeGraphicData == null)
		{
			return false;
		}
		IntVec3 c3 = c + IntVec3.West;
		IntVec3 c4 = c + IntVec3.East;
		if (!c3.InBounds(base.Map) || !c4.InBounds(base.Map))
		{
			return false;
		}
		TerrainDef terrainDef2 = grid.TerrainAt(c3);
		TerrainDef terrainDef3 = grid.TerrainAt(c4);
		if ((terrainDef3 != null && terrainDef3.dontRender) || (terrainDef2 != null && terrainDef2.dontRender))
		{
			return false;
		}
		corners = CornerDirections.NorthWest | CornerDirections.NorthEast;
		edges = EdgeDirections.North;
		return true;
	}

	private bool ShouldDrawLoop(IntVec3 c, TerrainGrid grid, out EdgeDirections edges, out CornerDirections corners)
	{
		edges = EdgeDirections.None;
		corners = CornerDirections.None;
		IntVec3 c2 = c + IntVec3.North;
		TerrainDef terrainDef = grid.TerrainAt(c);
		if ((terrainDef != null && !terrainDef.dontRender) || !c2.InBounds(base.Map))
		{
			return false;
		}
		TerrainDef terrainDef2 = grid.BaseTerrainAt(c2);
		if (terrainDef2 == null || terrainDef2.spaceEdgeGraphicData == null)
		{
			return false;
		}
		for (int i = 0; i < GenAdj.CardinalDirections.Length; i++)
		{
			IntVec3 c3 = c + GenAdj.CardinalDirections[i];
			if (!c3.InBounds(base.Map))
			{
				if (!base.Map.DrawMapClippers)
				{
					edges |= (EdgeDirections)(1 << i);
				}
				continue;
			}
			TerrainDef terrainDef3 = grid.TerrainAt(c3);
			if (terrainDef3 != null && !terrainDef3.dontRender)
			{
				edges |= (EdgeDirections)(1 << i);
			}
		}
		for (int j = 0; j < GenAdj.DiagonalDirections.Length; j++)
		{
			IntVec3 c4 = c + GenAdj.DiagonalDirections[j];
			if (!c4.InBounds(base.Map))
			{
				if (!base.Map.DrawMapClippers)
				{
					edges |= (EdgeDirections)(1 << j);
				}
				continue;
			}
			TerrainDef terrainDef4 = grid.TerrainAt(c4);
			if (terrainDef4 != null && !terrainDef4.dontRender)
			{
				corners |= (CornerDirections)(1 << j);
			}
		}
		return edges.HasFlag(EdgeDirections.North);
	}
}
