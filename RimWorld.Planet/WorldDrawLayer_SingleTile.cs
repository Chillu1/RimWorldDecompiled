using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld.Planet;

public abstract class WorldDrawLayer_SingleTile : WorldDrawLayer
{
	private PlanetTile lastDrawnPlanetTile = new PlanetTile(-1);

	private List<Vector3> verts = new List<Vector3>();

	protected abstract PlanetTile Tile { get; }

	protected abstract Material Material { get; }

	public override bool ShouldRegenerate
	{
		get
		{
			if (!base.ShouldRegenerate)
			{
				return Tile != lastDrawnPlanetTile;
			}
			return true;
		}
	}

	public override IEnumerable Regenerate()
	{
		foreach (object item in base.Regenerate())
		{
			yield return item;
		}
		PlanetTile tile = Tile;
		if (tile.Valid && tile.Layer == planetLayer)
		{
			LayerSubMesh subMesh = GetSubMesh(Material);
			Find.WorldGrid.GetTileVertices(tile, verts);
			int count = subMesh.verts.Count;
			int i = 0;
			for (int count2 = verts.Count; i < count2; i++)
			{
				subMesh.verts.Add(verts[i] + verts[i].normalized * 0.02f);
				subMesh.uvs.Add((GenGeo.RegularPolygonVertexPosition(count2, i) + Vector2.one) / 2f);
				if (i < count2 - 2)
				{
					subMesh.tris.Add(count + i + 2);
					subMesh.tris.Add(count + i + 1);
					subMesh.tris.Add(count);
				}
			}
			FinalizeMesh(MeshParts.All);
		}
		lastDrawnPlanetTile = tile;
	}
}
