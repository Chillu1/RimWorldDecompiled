using System.Collections.Generic;
using UnityEngine;

namespace Verse.Noise
{
	public static class GradientPresets
	{
		private static Gradient _empty;

		private static Gradient _grayscale;

		private static Gradient _rgb;

		private static Gradient _rgba;

		private static Gradient _terrain;

		public static Gradient Empty => _empty;

		public static Gradient Grayscale => _grayscale;

		public static Gradient RGB => _rgb;

		public static Gradient RGBA => _rgba;

		public static Gradient Terrain => _terrain;

		static GradientPresets()
		{
			List<GradientColorKey> list = new List<GradientColorKey>
			{
				new GradientColorKey(Color.black, 0f),
				new GradientColorKey(Color.white, 1f)
			};
			List<GradientColorKey> list2 = new List<GradientColorKey>
			{
				new GradientColorKey(Color.red, 0f),
				new GradientColorKey(Color.green, 0.5f),
				new GradientColorKey(Color.blue, 1f)
			};
			List<GradientColorKey> list3 = new List<GradientColorKey>
			{
				new GradientColorKey(Color.red, 0f),
				new GradientColorKey(Color.green, 0.333333343f),
				new GradientColorKey(Color.blue, 2f / 3f),
				new GradientColorKey(Color.black, 1f)
			};
			List<GradientAlphaKey> list4 = new List<GradientAlphaKey>
			{
				new GradientAlphaKey(0f, 2f / 3f),
				new GradientAlphaKey(1f, 1f)
			};
			List<GradientColorKey> list5 = new List<GradientColorKey>
			{
				new GradientColorKey(new Color(0f, 0f, 0.5f), 0f),
				new GradientColorKey(new Color(0.125f, 0.25f, 0.5f), 0.4f),
				new GradientColorKey(new Color(0.25f, 0.375f, 0.75f), 0.48f),
				new GradientColorKey(new Color(0f, 0.75f, 0f), 0.5f),
				new GradientColorKey(new Color(0.75f, 0.75f, 0f), 0.625f),
				new GradientColorKey(new Color(0.625f, 0.375f, 0.25f), 0.75f),
				new GradientColorKey(new Color(0.5f, 1f, 1f), 0.875f),
				new GradientColorKey(Color.white, 1f)
			};
			List<GradientAlphaKey> list6 = new List<GradientAlphaKey>
			{
				new GradientAlphaKey(1f, 0f),
				new GradientAlphaKey(1f, 1f)
			};
			_empty = new Gradient();
			_rgb = new Gradient();
			_rgb.SetKeys(list2.ToArray(), list6.ToArray());
			_rgba = new Gradient();
			_rgba.SetKeys(list3.ToArray(), list4.ToArray());
			_grayscale = new Gradient();
			_grayscale.SetKeys(list.ToArray(), list6.ToArray());
			_terrain = new Gradient();
			_terrain.SetKeys(list5.ToArray(), list6.ToArray());
		}
	}
}
