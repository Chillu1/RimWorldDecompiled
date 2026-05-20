using System.Collections.Generic;
using UnityEngine;

namespace Verse;

public static class ZoneColorUtility
{
	private static List<Color> growingZoneColors;

	private static List<Color> storageZoneColors;

	private static List<Color> fishingZoneColors;

	private static int nextGrowingZoneColorIndex;

	private static int nextStorageZoneColorIndex;

	private static int nextFishingZoneColorIndex;

	private const float ZoneOpacity = 0.09f;

	static ZoneColorUtility()
	{
		growingZoneColors = new List<Color>();
		storageZoneColors = new List<Color>();
		fishingZoneColors = new List<Color>();
		nextGrowingZoneColorIndex = 0;
		nextStorageZoneColorIndex = 0;
		nextFishingZoneColorIndex = 0;
		foreach (Color item4 in GrowingZoneColors())
		{
			Color item = new Color(item4.r, item4.g, item4.b, 0.09f);
			growingZoneColors.Add(item);
		}
		foreach (Color item5 in StorageZoneColors())
		{
			Color item2 = new Color(item5.r, item5.g, item5.b, 0.09f);
			storageZoneColors.Add(item2);
		}
		foreach (Color item6 in FishingZoneColors())
		{
			Color item3 = new Color(item6.r, item6.g, item6.b, 0.09f);
			fishingZoneColors.Add(item3);
		}
	}

	public static Color NextGrowingZoneColor()
	{
		Color result = growingZoneColors[nextGrowingZoneColorIndex];
		nextGrowingZoneColorIndex++;
		if (nextGrowingZoneColorIndex >= growingZoneColors.Count)
		{
			nextGrowingZoneColorIndex = 0;
		}
		return result;
	}

	public static Color NextStorageZoneColor()
	{
		Color result = storageZoneColors[nextStorageZoneColorIndex];
		nextStorageZoneColorIndex++;
		if (nextStorageZoneColorIndex >= storageZoneColors.Count)
		{
			nextStorageZoneColorIndex = 0;
		}
		return result;
	}

	public static Color NextFishingZoneColor()
	{
		Color result = fishingZoneColors[nextFishingZoneColorIndex];
		nextFishingZoneColorIndex++;
		if (nextFishingZoneColorIndex >= fishingZoneColors.Count)
		{
			nextFishingZoneColorIndex = 0;
		}
		return result;
	}

	public static IEnumerable<Color> GrowingZoneColors()
	{
		yield return Color.Lerp(new Color(0f, 1f, 0f), Color.gray, 0.5f);
		yield return Color.Lerp(new Color(1f, 1f, 0f), Color.gray, 0.5f);
		yield return Color.Lerp(new Color(0.5f, 1f, 0f), Color.gray, 0.5f);
		yield return Color.Lerp(new Color(1f, 1f, 0.5f), Color.gray, 0.5f);
		yield return Color.Lerp(new Color(0.5f, 1f, 0.5f), Color.gray, 0.5f);
	}

	public static IEnumerable<Color> StorageZoneColors()
	{
		yield return Color.Lerp(new Color(1f, 0f, 0f), Color.gray, 0.5f);
		yield return Color.Lerp(new Color(1f, 0f, 1f), Color.gray, 0.5f);
		yield return Color.Lerp(new Color(0f, 0f, 1f), Color.gray, 0.5f);
		yield return Color.Lerp(new Color(1f, 0f, 0.5f), Color.gray, 0.5f);
		yield return Color.Lerp(new Color(0f, 0.5f, 1f), Color.gray, 0.5f);
		yield return Color.Lerp(new Color(0.5f, 0f, 1f), Color.gray, 0.5f);
	}

	public static IEnumerable<Color> FishingZoneColors()
	{
		yield return Color.Lerp(new Color(1f, 0.8f, 0f), Color.gray, 0.25f);
		yield return Color.Lerp(new Color(1f, 41f / 85f, 0f), Color.gray, 0.25f);
		yield return Color.Lerp(new Color(1f, 0.36862746f, 0f), Color.gray, 0.25f);
		yield return Color.Lerp(new Color(1f, 0.96862745f, 0f), Color.gray, 0.25f);
		yield return Color.Lerp(new Color(1f, 0.14901961f, 0f), Color.gray, 0.25f);
	}
}
