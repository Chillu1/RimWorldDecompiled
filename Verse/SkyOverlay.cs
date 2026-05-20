using UnityEngine;

namespace Verse;

public abstract class SkyOverlay
{
	public Color? ForcedOverlayColor { get; protected set; }

	public static void DrawWorldOverlay(Map map, Material mat, int layer = 0)
	{
		DrawWorldOverlay(map, mat, AltitudeLayer.Weather.AltitudeFor(), layer);
	}

	public static void DrawWorldOverlay(Map map, Material mat, float altitude, int layer = 0)
	{
		Vector3 position = map.Center.ToVector3Shifted().WithY(altitude);
		Graphics.DrawMesh(MeshPool.wholeMapPlane, position, Quaternion.identity, mat, layer);
	}

	public static void DrawScreenOverlay(Material mat, int layer = 0, Camera camera = null)
	{
		DrawScreenOverlay(mat, AltitudeLayer.Weather.AltitudeFor() + 0.03658537f, layer, camera);
	}

	public virtual void Reset()
	{
	}

	public static void DrawScreenOverlay(Material mat, float altitude, int layer = 0, Camera camera = null)
	{
		if (!camera)
		{
			camera = Find.Camera;
		}
		float num = camera.orthographicSize * 2f;
		Vector3 s = new Vector3(num * camera.aspect, 1f, num);
		Vector3 position = camera.transform.position;
		position.y = altitude;
		Matrix4x4 matrix = default(Matrix4x4);
		matrix.SetTRS(position, Quaternion.identity, s);
		Graphics.DrawMesh(MeshPool.plane10, matrix, mat, layer);
	}

	public abstract void TickOverlay(Map map, float lerpFactor);

	public abstract void DrawOverlay(Map map);

	public abstract void SetOverlayColor(Color color);

	public override string ToString()
	{
		return "NoOverlayOverlay";
	}
}
