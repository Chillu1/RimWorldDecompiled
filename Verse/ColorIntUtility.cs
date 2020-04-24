using UnityEngine;

namespace Verse
{
	public static class ColorIntUtility
	{
		public static ColorInt AsColorInt(this Color32 col)
		{
			return new ColorInt(col);
		}
	}
}
