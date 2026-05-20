using System;
using System.Collections.Generic;
using UnityEngine;

namespace Verse;

public class CellBoolDrawer
{
	private bool wantDraw;

	private Material material;

	private bool materialCaresAboutVertexColors;

	private bool dirty = true;

	private List<Mesh> meshes = new List<Mesh>();

	private int mapSizeX;

	private int mapSizeZ;

	private float opacity = 0.33f;

	private int renderQueue = 3600;

	private Func<Color> colorGetter;

	private Func<int, Color> extraColorGetter;

	private Func<int, bool> cellBoolGetter;

	private static List<Vector3> verts = new List<Vector3>();

	private static List<int> tris = new List<int>();

	private static List<Color> colors = new List<Color>();

	private const float DefaultOpacity = 0.33f;

	private const int MaxCellsPerMesh = 16383;

	private CellBoolDrawer(int mapSizeX, int mapSizeZ, float opacity = 0.33f)
	{
		this.mapSizeX = mapSizeX;
		this.mapSizeZ = mapSizeZ;
		this.opacity = opacity;
	}

	public CellBoolDrawer(ICellBoolGiver giver, int mapSizeX, int mapSizeZ, float opacity = 0.33f)
		: this(mapSizeX, mapSizeZ, opacity)
	{
		colorGetter = () => giver.Color;
		extraColorGetter = giver.GetCellExtraColor;
		cellBoolGetter = giver.GetCellBool;
	}

	public CellBoolDrawer(ICellBoolGiver giver, int mapSizeX, int mapSizeZ, int renderQueue, float opacity = 0.33f)
		: this(giver, mapSizeX, mapSizeZ, opacity)
	{
		this.renderQueue = renderQueue;
	}

	public CellBoolDrawer(Func<int, bool> cellBoolGetter, Func<Color> colorGetter, Func<int, Color> extraColorGetter, int mapSizeX, int mapSizeZ, float opacity = 0.33f)
		: this(mapSizeX, mapSizeZ, opacity)
	{
		this.colorGetter = colorGetter;
		this.extraColorGetter = extraColorGetter;
		this.cellBoolGetter = cellBoolGetter;
	}

	public CellBoolDrawer(Func<int, bool> cellBoolGetter, Func<Color> colorGetter, Func<int, Color> extraColorGetter, int mapSizeX, int mapSizeZ, int renderQueue, float opacity = 0.33f)
		: this(cellBoolGetter, colorGetter, extraColorGetter, mapSizeX, mapSizeZ, opacity)
	{
		this.renderQueue = renderQueue;
	}

	public void MarkForDraw()
	{
		wantDraw = true;
	}

	public void CellBoolDrawerUpdate()
	{
		if (wantDraw)
		{
			ActuallyDraw();
			wantDraw = false;
		}
	}

	private void ActuallyDraw()
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
				name = "CellBoolDrawer"
			};
			meshes.Add(item);
		}
		Mesh mesh = meshes[num];
		CellRect cellRect = new CellRect(0, 0, mapSizeX, mapSizeZ);
		float y = AltitudeLayer.MapDataOverlay.AltitudeFor();
		bool careAboutVertexColors = false;
		for (int j = cellRect.minX; j <= cellRect.maxX; j++)
		{
			for (int k = cellRect.minZ; k <= cellRect.maxZ; k++)
			{
				int arg = CellIndicesUtility.CellToIndex(j, k, mapSizeX);
				if (!cellBoolGetter(arg))
				{
					continue;
				}
				verts.Add(new Vector3(j, y, k));
				verts.Add(new Vector3(j, y, k + 1));
				verts.Add(new Vector3(j + 1, y, k + 1));
				verts.Add(new Vector3(j + 1, y, k));
				Color color = extraColorGetter(arg);
				colors.Add(color);
				colors.Add(color);
				colors.Add(color);
				colors.Add(color);
				if (color != Color.white)
				{
					careAboutVertexColors = true;
				}
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
							name = "CellBoolDrawer"
						};
						meshes.Add(item2);
					}
					mesh = meshes[num];
					num2 = 0;
				}
			}
		}
		FinalizeWorkingDataIntoMesh(mesh);
		CreateMaterialIfNeeded(careAboutVertexColors);
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

	private void CreateMaterialIfNeeded(bool careAboutVertexColors)
	{
		if (material == null || materialCaresAboutVertexColors != careAboutVertexColors)
		{
			Color color = colorGetter();
			material = SolidColorMaterials.SimpleSolidColorMaterial(new Color(color.r, color.g, color.b, opacity * color.a), careAboutVertexColors);
			materialCaresAboutVertexColors = careAboutVertexColors;
			material.renderQueue = renderQueue;
		}
	}

	public void Notify_ColorChanged()
	{
		material = null;
		SetDirty();
	}
}
