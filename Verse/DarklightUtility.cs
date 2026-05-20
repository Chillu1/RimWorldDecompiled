using UnityEngine;

namespace Verse
{
	public static class DarklightUtility
	{
		private static FloatRange DarklightHueRange = new FloatRange(0.49f, 0.51f);

		public static readonly Color DefaultDarklight = new Color32(78, 226, 229, byte.MaxValue);

		public static bool IsDarklight(Color color)
		{
			if (color.r > color.g || color.r > color.b)
			{
				return false;
			}
			float num;
			float num2;
			if (color.g > color.b)
			{
				num = color.g;
				num2 = color.b;
			}
			else
			{
				num = color.b;
				num2 = color.g;
			}
			if (num == 0f)
			{
				return false;
			}
			if (color.r > num / 2f)
			{
				return false;
			}
			if (num2 / num <= 0.85f)
			{
				return false;
			}
			return true;
		}

		public static bool IsDarklightAt(IntVec3 position, Map map)
		{
			if (position.InBounds(map) && position.Roofed(map) && (int)map.glowGrid.PsychGlowAt(position) >= 1)
			{
				return IsDarklight(map.glowGrid.VisualGlowAt(position));
			}
			return false;
		}
	}
}
