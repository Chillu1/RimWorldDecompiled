using UnityEngine;

namespace Verse
{
	public static class ColorExtension
	{
		public static Color ToOpaque(this Color c)
		{
			c.a = 1f;
			return c;
		}
	}
}
