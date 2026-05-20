using UnityEngine;

namespace Verse;

public static class ColorExtension
{
	public static Color ToOpaque(this Color c)
	{
		c.a = 1f;
		return c;
	}

	public static Color ToTransparent(this Color c, float transparency)
	{
		c.a = transparency;
		return c;
	}

	public static Color Min(this Color c, Color other)
	{
		c.r = Mathf.Min(c.r, other.r);
		c.g = Mathf.Min(c.g, other.g);
		c.b = Mathf.Min(c.b, other.b);
		return c;
	}
}
