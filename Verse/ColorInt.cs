using System;
using UnityEngine;

namespace Verse
{
	public struct ColorInt : IEquatable<ColorInt>
	{
		public int r;

		public int g;

		public int b;

		public int a;

		public Color ToColor
		{
			get
			{
				Color result = default(Color);
				result.r = (float)r / 255f;
				result.g = (float)g / 255f;
				result.b = (float)b / 255f;
				result.a = (float)a / 255f;
				return result;
			}
		}

		public Color32 ToColor32
		{
			get
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
				if (r > 255)
				{
					result.r = byte.MaxValue;
				}
				else
				{
					result.r = (byte)r;
				}
				if (g > 255)
				{
					result.g = byte.MaxValue;
				}
				else
				{
					result.g = (byte)g;
				}
				if (b > 255)
				{
					result.b = byte.MaxValue;
				}
				else
				{
					result.b = (byte)b;
				}
				return result;
			}
		}

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
			if (!(o is ColorInt))
			{
				return false;
			}
			return Equals((ColorInt)o);
		}

		public bool Equals(ColorInt other)
		{
			return this == other;
		}

		public override int GetHashCode()
		{
			return r + g * 256 + b * 256 * 256 + a * 256 * 256 * 256;
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
	}
}
