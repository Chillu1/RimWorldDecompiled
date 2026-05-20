using LudeonTK;
using RimWorld;
using UnityEngine;

namespace Verse;

[StaticConstructorOnStartup]
internal class SectionLayer_DebugNoise : SectionLayer
{
	private static Material material;

	public override bool Visible => DebugViewSettings.drawWorldOverlays;

	public SectionLayer_DebugNoise(Section section)
		: base(section)
	{
		relevantChangeTypes = MapMeshFlagDefOf.None;
		material = (material ? material : new Material(ShaderDatabase.VertexColor));
		material.renderQueue = 3800;
	}

	private bool TryGetWindow(out Dialog_DevNoiseMap window)
	{
		return Find.WindowStack.TryGetWindow<Dialog_DevNoiseMap>(out window);
	}

	public override void DrawLayer()
	{
		if (TryGetWindow(out var window) && !(window.alpha <= 0f))
		{
			base.DrawLayer();
		}
	}

	public override void Regenerate()
	{
		float y = AltitudeLayer.Zone.AltitudeFor();
		CellRect cellRect = new CellRect(section.botLeft.x, section.botLeft.z, 17, 17);
		cellRect.ClipInsideMap(base.Map);
		ClearSubMeshes(MeshParts.All);
		if (!TryGetWindow(out var window))
		{
			FinalizeMesh(MeshParts.Verts | MeshParts.Tris | MeshParts.Colors);
			return;
		}
		byte a = (byte)Mathf.Clamp(Mathf.RoundToInt(window.alpha * 255.999f), 0, 255);
		for (int i = cellRect.minX; i <= cellRect.maxX; i++)
		{
			for (int j = cellRect.minZ; j <= cellRect.maxZ; j++)
			{
				byte b = (byte)Mathf.Clamp(Mathf.RoundToInt((float)window.noise.GetValue(i, 0.0, j) * 255.999f), 0, 255);
				Color32 item = new Color32(b, b, b, a);
				if (!window.cutoffEnabled || window.noise.GetValue(i, 0.0, j) >= (double)window.cutoff)
				{
					LayerSubMesh subMesh = GetSubMesh(material);
					int count = subMesh.verts.Count;
					subMesh.verts.Add(new Vector3(i, y, j));
					subMesh.verts.Add(new Vector3(i, y, j + 1));
					subMesh.verts.Add(new Vector3(i + 1, y, j + 1));
					subMesh.verts.Add(new Vector3(i + 1, y, j));
					subMesh.colors.Add(item);
					subMesh.colors.Add(item);
					subMesh.colors.Add(item);
					subMesh.colors.Add(item);
					subMesh.tris.Add(count);
					subMesh.tris.Add(count + 1);
					subMesh.tris.Add(count + 2);
					subMesh.tris.Add(count);
					subMesh.tris.Add(count + 2);
					subMesh.tris.Add(count + 3);
				}
			}
		}
		FinalizeMesh(MeshParts.Verts | MeshParts.Tris | MeshParts.Colors);
	}
}
