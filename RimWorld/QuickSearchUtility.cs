using UnityEngine;
using Verse;

namespace RimWorld;

public static class QuickSearchUtility
{
	private static string[] searchingTexts;

	private static string[] SearchingTexts
	{
		get
		{
			if (searchingTexts == null)
			{
				string text = "Searching".Translate();
				searchingTexts = new string[3]
				{
					text + ".",
					text + "..",
					text + "..."
				};
			}
			return searchingTexts;
		}
	}

	public static string CurrentSearchText => SearchingTexts[Time.frameCount / 20 % searchingTexts.Length];

	public static void ResetStaticData()
	{
		searchingTexts = null;
	}
}
