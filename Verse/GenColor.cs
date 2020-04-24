using System.Globalization;
using UnityEngine;

namespace Verse
{
	public static class GenColor
	{
		public static Color SaturationChanged(this Color col, float change)
		{
			float r = col.r;
			float g = col.g;
			float b = col.b;
			float num = Mathf.Sqrt(r * r * 0.299f + g * g * 0.587f + b * b * 0.114f);
			r = num + (r - num) * change;
			g = num + (g - num) * change;
			b = num + (b - num) * change;
			return new Color(r, g, b);
		}

		public static bool IndistinguishableFrom(this Color colA, Color colB)
		{
			if (Colors32Equal(colA, colB))
			{
				return true;
			}
			Color color = colA - colB;
			return Mathf.Abs(color.r) + Mathf.Abs(color.g) + Mathf.Abs(color.b) + Mathf.Abs(color.a) < 0.001f;
		}

		public static bool Colors32Equal(Color a, Color b)
		{
			Color32 color = a;
			Color32 color2 = b;
			if (color.r == color2.r && color.g == color2.g && color.b == color2.b)
			{
				return color.a == color2.a;
			}
			return false;
		}

		public static Color RandomColorOpaque()
		{
			return new Color(Rand.Value, Rand.Value, Rand.Value, 1f);
		}

		public static Color FromBytes(int r, int g, int b, int a = 255)
		{
			Color result = default(Color);
			result.r = (float)r / 255f;
			result.g = (float)g / 255f;
			result.b = (float)b / 255f;
			result.a = (float)a / 255f;
			return result;
		}

		public static Color FromHex(string hex)
		{
			if (hex.StartsWith("#"))
			{
				hex = hex.Substring(1);
			}
			if (hex.Length != 6 && hex.Length != 8)
			{
				Log.Error(hex + " is not a valid hex color.");
				return Color.white;
			}
			int r = int.Parse(hex.Substring(0, 2), NumberStyles.HexNumber);
			int g = int.Parse(hex.Substring(2, 2), NumberStyles.HexNumber);
			int b = int.Parse(hex.Substring(4, 2), NumberStyles.HexNumber);
			int a = 255;
			if (hex.Length == 8)
			{
				a = int.Parse(hex.Substring(6, 2), NumberStyles.HexNumber);
			}
			return FromBytes(r, g, b, a);
		}
	}
}
