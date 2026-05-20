using System.Collections.Generic;
using UnityEngine;

namespace Verse;

[StaticConstructorOnStartup]
public class MapDrawLayer_ExteriorLightingOverlay : MapDrawLayer
{
	private static readonly int RenderLayer = LayerMask.NameToLayer("GravshipExclude");

	private const int Size = 200;

	public override bool Visible
	{
		get
		{
			if (DebugViewSettings.drawLightingOverlay)
			{
				return !base.Map.DrawMapClippers;
			}
			return false;
		}
	}

	public MapDrawLayer_ExteriorLightingOverlay(Map map)
		: base(map)
	{
	}

	public override void Regenerate()
	{
		LayerSubMesh subMesh = GetSubMesh(MatBases.LightOverlay);
		LayerSubMesh subMesh2 = GetSubMesh(MatBases.ShadowMask);
		subMesh.renderLayer = RenderLayer;
		subMesh2.renderLayer = RenderLayer;
		MakeBaseGeometry(base.Map, subMesh);
		MakeBaseGeometry(base.Map, subMesh2);
	}

	private static IEnumerable<Rect> GetClipperRects(Map map)
	{
		yield return new Rect(-200f, 0f, 200f, map.Size.z);
		yield return new Rect(map.Size.x, 0f, 200f, map.Size.z);
		yield return new Rect(-200f, map.Size.z, map.Size.x + 400, 200f);
		yield return new Rect(-200f, -200f, map.Size.x + 400, 200f);
	}

	private static void MakeBaseGeometry(Map map, LayerSubMesh sm)
	{
		float y = AltitudeLayer.LightingOverlay.AltitudeFor();
		sm.Clear(MeshParts.Verts | MeshParts.Tris | MeshParts.Colors);
		foreach (Rect clipperRect in GetClipperRects(map))
		{
			sm.verts.Add(new Vector3(clipperRect.x, y, clipperRect.y));
			sm.verts.Add(new Vector3(clipperRect.x, y, clipperRect.yMax));
			sm.verts.Add(new Vector3(clipperRect.xMax, y, clipperRect.yMax));
			sm.verts.Add(new Vector3(clipperRect.xMax, y, clipperRect.y));
			sm.colors.Add(new Color32(0, 0, 0, 0));
			sm.colors.Add(new Color32(0, 0, 0, 0));
			sm.colors.Add(new Color32(0, 0, 0, 0));
			sm.colors.Add(new Color32(0, 0, 0, 0));
			sm.tris.Add(sm.verts.Count - 4);
			sm.tris.Add(sm.verts.Count - 3);
			sm.tris.Add(sm.verts.Count - 2);
			sm.tris.Add(sm.verts.Count - 4);
			sm.tris.Add(sm.verts.Count - 2);
			sm.tris.Add(sm.verts.Count - 1);
		}
		sm.FinalizeMesh(MeshParts.Verts | MeshParts.Tris | MeshParts.Colors);
	}
}
