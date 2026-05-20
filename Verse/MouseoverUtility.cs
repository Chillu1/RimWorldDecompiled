using UnityEngine;

namespace Verse
{
	public static class MouseoverUtility
	{
		private static string[] glowStrings;

		static MouseoverUtility()
		{
			MakePermaCache();
		}

		public static void Reset()
		{
			MakePermaCache();
		}

		private static void MakePermaCache()
		{
			glowStrings = new string[101];
			for (int i = 0; i <= 100; i++)
			{
				glowStrings[i] = GlowGrid.PsychGlowAtGlow((float)i / 100f).GetLabel() + " (" + ((float)i / 100f).ToStringPercent() + ")";
			}
		}

		public static string GetGlowLabelByValue(float value)
		{
			return glowStrings[Mathf.RoundToInt(value * 100f)];
		}
	}
}
