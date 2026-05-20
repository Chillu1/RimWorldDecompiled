using System.Globalization;
using UnityEngine;

namespace Verse;

public static class GenColor
{
	private static float[,,] tmpBuckets;

	private const float redScaleFactor = 1.2929362f;

	private const float redPowerFactor = -0.13320476f;

	private const float blueScaleFactor = 0.5432068f;

	private const float blueOffset = 1.1962541f;

	private const float coolGreenScale = 0.39008158f;

	private const float coolGreenOffset = 0.6318414f;

	private const float warmGreenScale = 1.1298909f;

	private const float warmGreenPower = -0.075514846f;

	public const float minColorTemperature = 1000f;

	public const float maxColorTemperature = 40000f;

	public const float whiteColorTemperature = 6600f;

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

	public static bool IndistinguishableFromFast(this Color colA, Color colB)
	{
		return Mathf.Abs(colA.r - colB.r) + Mathf.Abs(colA.g - colB.g) + Mathf.Abs(colA.b - colB.b) + Mathf.Abs(colA.a - colB.a) < 0.005f;
	}

	public static bool IndistinguishableFrom(this Color colA, Color colB)
	{
		if (Colors32Equal(colA, colB))
		{
			return true;
		}
		Color color = colA - colB;
		return Mathf.Abs(color.r) + Mathf.Abs(color.g) + Mathf.Abs(color.b) + Mathf.Abs(color.a) < 0.005f;
	}

	public static bool WithinDiffThresholdFrom(this Color colA, Color colB, float threshold)
	{
		Color color = colA - colB;
		return Mathf.Abs(color.r) + Mathf.Abs(color.g) + Mathf.Abs(color.b) + Mathf.Abs(color.a) < threshold;
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
		return new Color
		{
			r = (float)r / 255f,
			g = (float)g / 255f,
			b = (float)b / 255f,
			a = (float)a / 255f
		};
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

	public static Color GetDominantColor(this Texture2D texture, int buckets = 25, float minBrightness = 0f)
	{
		if (texture == BaseContent.BadTex)
		{
			return Color.white;
		}
		if (tmpBuckets == null || tmpBuckets.GetLength(0) != buckets)
		{
			tmpBuckets = new float[buckets, buckets, buckets];
		}
		for (int i = 0; i < texture.width; i++)
		{
			for (int j = 0; j < texture.height; j++)
			{
				Color pixel = texture.GetPixel(i, j);
				if (!((pixel.r + pixel.g + pixel.b) / 3f < minBrightness))
				{
					tmpBuckets[Mathf.Clamp((int)(pixel.r * (float)buckets), 0, buckets - 1), Mathf.Clamp((int)(pixel.g * (float)buckets), 0, buckets - 1), Mathf.Clamp((int)(pixel.b * (float)buckets), 0, buckets - 1)] += pixel.a;
				}
			}
		}
		float num = 0f;
		int num2 = 0;
		int num3 = 0;
		int num4 = 0;
		for (int k = 0; k < buckets; k++)
		{
			for (int l = 0; l < buckets; l++)
			{
				for (int m = 0; m < buckets; m++)
				{
					if (tmpBuckets[k, l, m] > num)
					{
						num = tmpBuckets[k, l, m];
						num2 = k;
						num3 = l;
						num4 = m;
					}
				}
			}
		}
		return new Color(((float)num2 + 0.5f) / (float)buckets, ((float)num3 + 0.5f) / (float)buckets, ((float)num4 + 0.5f) / (float)buckets);
	}

	public static Color ClampToValueRange(this Color color, FloatRange range)
	{
		Color.RGBToHSV(color, out var H, out var S, out var V);
		V = range.ClampToRange(V);
		color = Color.HSVToRGB(H, S, V);
		return color;
	}

	public static Color FromColorTemperature(float temperatureKelvin)
	{
		float num = temperatureKelvin / 100f;
		float value;
		float value2;
		float value3;
		if (num <= 66f)
		{
			value = 1f;
			value2 = 0.39008158f * Mathf.Log(num) - 0.6318414f;
			value3 = ((!(num <= 19f)) ? (0.5432068f * Mathf.Log(num - 10f) - 1.1962541f) : 0f);
		}
		else
		{
			num -= 60f;
			value = 1.2929362f * Mathf.Pow(num, -0.13320476f);
			value2 = 1.1298909f * Mathf.Pow(num, -0.075514846f);
			value3 = 1f;
		}
		return new Color(Mathf.Clamp01(value), Mathf.Clamp01(value2), Mathf.Clamp01(value3));
	}

	public static float? ColorTemperature(this Color color)
	{
		float num = Mathf.Max(color.r, color.g, color.b);
		if (num == 0f)
		{
			return null;
		}
		float num2 = color.r / num;
		float num3 = color.g / num;
		float num4 = color.b / num;
		if (num2 == 1f && num3 == 1f && num4 == 1f)
		{
			return 6600f;
		}
		float num7;
		if (num4 < 1f)
		{
			if (num2 < 1f)
			{
				return null;
			}
			float num5 = Mathf.Exp((num3 + 0.6318414f) / 0.39008158f);
			float num6 = ((num4 != 0f) ? (Mathf.Exp((num4 + 1.1962541f) / 0.5432068f) + 10f) : Mathf.Min(19f, num5));
			if (!(Mathf.Abs(num6 - num5) < 1f))
			{
				return null;
			}
			num7 = 50f * num5 + 50f * num6;
		}
		else
		{
			float num8 = Mathf.Exp(Mathf.Log(num3 / 1.1298909f) / -0.075514846f) + 60f;
			float num9 = ((num2 != 1f) ? (Mathf.Exp(Mathf.Log(num2 / 1.2929362f) / -0.13320476f) + 60f) : Mathf.Min(66.98f, Mathf.Max(66f, num8)));
			if (!(Mathf.Abs(num9 - num8) < 1f))
			{
				return null;
			}
			num7 = 50f * num8 + 50f * num9;
		}
		if (num7 >= 900f && num7 <= 40100f)
		{
			return Mathf.Clamp(num7, 1000f, 40000f);
		}
		return null;
	}

	public static Color WithAlpha(this Color color, float alpha)
	{
		Color result = color;
		result.a = alpha;
		return result;
	}
}
