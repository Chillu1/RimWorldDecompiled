using System;
using System.Collections.Generic;
using UnityEngine;

namespace Verse;

public class CellDrawer
{
	private bool markedForDraw;

	private Material material;

	private bool dirty = true;

	private List<Mesh> meshes = new List<Mesh>();

	private int mapSizeX;

	private int mapSizeZ;

	private Func<int, Color?> colorSource;

	private static List<Vector3> verts = new List<Vector3>();

	private static List<int> tris = new List<int>();

	private static List<Color> colors = new List<Color>();

	private const int MaxCellsPerMesh = 16383;

	public CellDrawer(Func<int, Color?> cellIndexToColor, Material material, int mapSizeX, int mapSizeZ)
	{
		colorSource = cellIndexToColor;
		this.material = material;
		this.mapSizeX = mapSizeX;
		this.mapSizeZ = mapSizeZ;
	}

	public void MarkForDraw()
	{
		markedForDraw = true;
	}

	public void Update()
	{
		if (markedForDraw)
		{
			Draw();
			markedForDraw = false;
		}
	}

	private void Draw()
	{
		if (dirty)
		{
			RegenerateMesh();
		}
		for (int i = 0; i < meshes.Count; i++)
		{
			Graphics.DrawMesh(meshes[i], Vector3.zero, Quaternion.identity, material, 0);
		}
	}

	public void SetDirty()
	{
		dirty = true;
	}

	public void RegenerateMesh()
	{
		for (int i = 0; i < meshes.Count; i++)
		{
			meshes[i].Clear();
		}
		int num = 0;
		int num2 = 0;
		if (meshes.Count < 1)
		{
			Mesh item = new Mesh
			{
				name = "CellDrawer"
			};
			meshes.Add(item);
		}
		Mesh mesh = meshes[num];
		CellRect cellRect = new CellRect(0, 0, mapSizeX, mapSizeZ);
		float y = AltitudeLayer.MapDataOverlay.AltitudeFor();
		for (int j = cellRect.minX; j <= cellRect.maxX; j++)
		{
			for (int k = cellRect.minZ; k <= cellRect.maxZ; k++)
			{
				int arg = CellIndicesUtility.CellToIndex(j, k, mapSizeX);
				Color? color = colorSource(arg);
				if (!color.HasValue)
				{
					continue;
				}
				verts.Add(new Vector3(j, y, k));
				verts.Add(new Vector3(j, y, k + 1));
				verts.Add(new Vector3(j + 1, y, k + 1));
				verts.Add(new Vector3(j + 1, y, k));
				Color value = color.Value;
				colors.Add(value);
				colors.Add(value);
				colors.Add(value);
				colors.Add(value);
				int count = verts.Count;
				tris.Add(count - 4);
				tris.Add(count - 3);
				tris.Add(count - 2);
				tris.Add(count - 4);
				tris.Add(count - 2);
				tris.Add(count - 1);
				num2++;
				if (num2 >= 16383)
				{
					FinalizeWorkingDataIntoMesh(mesh);
					num++;
					if (meshes.Count < num + 1)
					{
						Mesh item2 = new Mesh
						{
							name = "CellDrawer"
						};
						meshes.Add(item2);
					}
					mesh = meshes[num];
					num2 = 0;
				}
			}
		}
		FinalizeWorkingDataIntoMesh(mesh);
		dirty = false;
	}

	private void FinalizeWorkingDataIntoMesh(Mesh mesh)
	{
		if (verts.Count > 0)
		{
			mesh.SetVertices(verts);
			verts.Clear();
			mesh.SetTriangles(tris, 0);
			tris.Clear();
			mesh.SetColors(colors);
			colors.Clear();
		}
	}
}
