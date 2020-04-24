using UnityEngine;

namespace Verse.Noise
{
	public static class NoiseRenderer
	{
		public static IntVec2 renderSize = new IntVec2(200, 200);

		private static Color[] spectrum = new Color[4]
		{
			Color.black,
			Color.blue,
			Color.green,
			Color.white
		};

		public static Texture2D NoiseRendered(ModuleBase noise)
		{
			return NoiseRendered(new CellRect(0, 0, renderSize.x, renderSize.z), noise);
		}

		public static Texture2D NoiseRendered(CellRect rect, ModuleBase noise)
		{
			Texture2D texture2D = new Texture2D(rect.Width, rect.Height);
			texture2D.name = "NoiseRender";
			foreach (IntVec2 item in rect.Cells2D)
			{
				texture2D.SetPixel(item.x, item.z, ColorForValue(noise.GetValue(item)));
			}
			texture2D.Apply();
			return texture2D;
		}

		private static Color ColorForValue(float val)
		{
			val = val * 0.5f + 0.5f;
			return ColorsFromSpectrum.Get(spectrum, val);
		}
	}
}
