using UnityEngine;

namespace Verse
{
	public static class CellRenderer
	{
		private static int lastCameraUpdateFrame = -1;

		private static CellRect viewRect;

		private static void InitFrame()
		{
			if (Time.frameCount != lastCameraUpdateFrame)
			{
				viewRect = Find.CameraDriver.CurrentViewRect;
				lastCameraUpdateFrame = Time.frameCount;
			}
		}

		private static Material MatFromColorPct(float colorPct, bool transparent)
		{
			return DebugMatsSpectrum.Mat(GenMath.PositiveMod(Mathf.RoundToInt(colorPct * 100f), 100), transparent);
		}

		public static void RenderCell(IntVec3 c, float colorPct = 0.5f)
		{
			RenderCell(c, MatFromColorPct(colorPct, transparent: true));
		}

		public static void RenderCell(IntVec3 c, Material mat)
		{
			InitFrame();
			if (viewRect.Contains(c))
			{
				Vector3 position = c.ToVector3ShiftedWithAltitude(AltitudeLayer.MetaOverlays);
				Graphics.DrawMesh(MeshPool.plane10, position, Quaternion.identity, mat, 0);
			}
		}

		public static void RenderSpot(Vector3 loc, float colorPct = 0.5f)
		{
			RenderSpot(loc, MatFromColorPct(colorPct, transparent: false));
		}

		public static void RenderSpot(Vector3 loc, Material mat, float scale = 0.15f)
		{
			InitFrame();
			if (viewRect.Contains(loc.ToIntVec3()))
			{
				loc.y = AltitudeLayer.MetaOverlays.AltitudeFor();
				Vector3 s = new Vector3(scale, 1f, scale);
				Matrix4x4 matrix = default(Matrix4x4);
				matrix.SetTRS(loc, Quaternion.identity, s);
				Graphics.DrawMesh(MeshPool.circle, matrix, mat, 0);
			}
		}
	}
}
