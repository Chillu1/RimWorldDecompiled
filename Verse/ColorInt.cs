using System;
using Unity.Burst;
using UnityEngine;

namespace Verse;

[BurstCompile]
public struct ColorInt : IEquatable<ColorInt>
{
	public int r;

	public int g;

	public int b;

	public int a;

	public Color ToColor => new Color
	{
		r = (float)r / 255f,
		g = (float)g / 255f,
		b = (float)b / 255f,
		a = (float)a / 255f
	};

	public ColorInt(int r, int g, int b)
	{
		this.r = r;
		this.g = g;
		this.b = b;
		a = 255;
	}

	public ColorInt(int r, int g, int b, int a)
	{
		this.r = r;
		this.g = g;
		this.b = b;
		this.a = a;
	}

	public ColorInt(Color32 col)
	{
		r = col.r;
		g = col.g;
		b = col.b;
		a = col.a;
	}

	public ColorInt(Color color)
	{
		r = FloatToByte(color.r);
		g = FloatToByte(color.g);
		b = FloatToByte(color.b);
		a = FloatToByte(color.a);
	}

	private static byte FloatToByte(float value)
	{
		if (value >= 1f)
		{
			return byte.MaxValue;
		}
		if (value <= 0f)
		{
			return 0;
		}
		return (byte)Mathf.Floor(value * 256f);
	}

	public static ColorInt operator +(ColorInt colA, ColorInt colB)
	{
		return new ColorInt(colA.r + colB.r, colA.g + colB.g, colA.b + colB.b, colA.a + colB.a);
	}

	public static ColorInt operator +(ColorInt colA, Color32 colB)
	{
		return new ColorInt(colA.r + colB.r, colA.g + colB.g, colA.b + colB.b, colA.a + colB.a);
	}

	public static ColorInt operator -(ColorInt a, ColorInt b)
	{
		return new ColorInt(a.r - b.r, a.g - b.g, a.b - b.b, a.a - b.a);
	}

	public static ColorInt operator *(ColorInt a, int b)
	{
		return new ColorInt(a.r * b, a.g * b, a.b * b, a.a * b);
	}

	public static ColorInt operator *(ColorInt a, float b)
	{
		return new ColorInt((int)((float)a.r * b), (int)((float)a.g * b), (int)((float)a.b * b), (int)((float)a.a * b));
	}

	public static ColorInt operator /(ColorInt a, int b)
	{
		return new ColorInt(a.r / b, a.g / b, a.b / b, a.a / b);
	}

	public static ColorInt operator /(ColorInt a, float b)
	{
		return new ColorInt((int)((float)a.r / b), (int)((float)a.g / b), (int)((float)a.b / b), (int)((float)a.a / b));
	}

	public static bool operator ==(ColorInt a, ColorInt b)
	{
		if (a.r == b.r && a.g == b.g && a.b == b.b)
		{
			return a.a == b.a;
		}
		return false;
	}

	public static bool operator !=(ColorInt a, ColorInt b)
	{
		if (a.r == b.r && a.g == b.g && a.b == b.b)
		{
			return a.a != b.a;
		}
		return true;
	}

	public override bool Equals(object o)
	{
		if (!(o is ColorInt other))
		{
			return false;
		}
		return Equals(other);
	}

	public bool Equals(ColorInt other)
	{
		return this == other;
	}

	public override int GetHashCode()
	{
		return r + g * 256 + b * 256 * 256 + a * 256 * 256 * 256;
	}

	public override string ToString()
	{
		return $"{r}, {g}, {b}, {a}";
	}

	public void ClampToNonNegative()
	{
		if (r < 0)
		{
			r = 0;
		}
		if (g < 0)
		{
			g = 0;
		}
		if (b < 0)
		{
			b = 0;
		}
		if (a < 0)
		{
			a = 0;
		}
	}

	public Color32 ProjectToColor32()
	{
		Color32 result = default(Color32);
		if (a > 255)
		{
			result.a = byte.MaxValue;
		}
		else
		{
			result.a = (byte)a;
		}
		int num = r;
		if (g > num)
		{
			num = g;
		}
		if (b > num)
		{
			num = b;
		}
		if (num > 255)
		{
			result.r = (byte)(r * 255 / num);
			result.g = (byte)(g * 255 / num);
			result.b = (byte)(b * 255 / num);
		}
		else
		{
			result.r = (byte)r;
			result.g = (byte)g;
			result.b = (byte)b;
		}
		return result;
	}

	[BurstCompile]
	public void ProjectToColor32Fast(out Color32 outColor)
	{
		outColor = default(Color32);
		if (a > 255)
		{
			outColor.a = byte.MaxValue;
		}
		else
		{
			outColor.a = (byte)a;
		}
		int num = r;
		if (g > num)
		{
			num = g;
		}
		if (b > num)
		{
			num = b;
		}
		if (num > 255)
		{
			outColor.r = (byte)(r * 255 / num);
			outColor.g = (byte)(g * 255 / num);
			outColor.b = (byte)(b * 255 / num);
		}
		else
		{
			outColor.r = (byte)r;
			outColor.g = (byte)g;
			outColor.b = (byte)b;
		}
	}

	public void SetHueSaturation(float hue, float sat)
	{
		float v = (float)Mathf.Max(r, g, b) / 255f;
		ColorInt colorInt = FromHdrColor(Color.HSVToRGB(hue, sat, v, hdr: true));
		r = colorInt.r;
		g = colorInt.g;
		b = colorInt.b;
	}

	public static ColorInt FromHdrColor(Color color, float? alphaOverride = null)
	{
		return new ColorInt
		{
			r = Mathf.RoundToInt(color.r * 255f),
			g = Mathf.RoundToInt(color.g * 255f),
			b = Mathf.RoundToInt(color.b * 255f),
			a = Mathf.RoundToInt((alphaOverride ?? color.a) * 255f)
		};
	}
}
