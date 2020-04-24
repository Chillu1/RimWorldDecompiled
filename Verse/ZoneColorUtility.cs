using System.Collections.Generic;
using UnityEngine;

namespace Verse
{
	public static class ZoneColorUtility
	{
		private static List<Color> growingZoneColors;

		private static List<Color> storageZoneColors;

		private static int nextGrowingZoneColorIndex;

		private static int nextStorageZoneColorIndex;

		private const float ZoneOpacity = 0.09f;

		static ZoneColorUtility()
		{
			growingZoneColors = new List<Color>();
			storageZoneColors = new List<Color>();
			nextGrowingZoneColorIndex = 0;
			nextStorageZoneColorIndex = 0;
			foreach (Color item3 in GrowingZoneColors())
			{
				Color item = new Color(item3.r, item3.g, item3.b, 0.09f);
				growingZoneColors.Add(item);
			}
			foreach (Color item4 in StorageZoneColors())
			{
				Color item2 = new Color(item4.r, item4.g, item4.b, 0.09f);
				storageZoneColors.Add(item2);
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

		private static IEnumerable<Color> GrowingZoneColors()
		{
			yield return Color.Lerp(new Color(0f, 1f, 0f), Color.gray, 0.5f);
			yield return Color.Lerp(new Color(1f, 1f, 0f), Color.gray, 0.5f);
			yield return Color.Lerp(new Color(0.5f, 1f, 0f), Color.gray, 0.5f);
			yield return Color.Lerp(new Color(1f, 1f, 0.5f), Color.gray, 0.5f);
			yield return Color.Lerp(new Color(0.5f, 1f, 0.5f), Color.gray, 0.5f);
		}

		private static IEnumerable<Color> StorageZoneColors()
		{
			yield return Color.Lerp(new Color(1f, 0f, 0f), Color.gray, 0.5f);
			yield return Color.Lerp(new Color(1f, 0f, 1f), Color.gray, 0.5f);
			yield return Color.Lerp(new Color(0f, 0f, 1f), Color.gray, 0.5f);
			yield return Color.Lerp(new Color(1f, 0f, 0.5f), Color.gray, 0.5f);
			yield return Color.Lerp(new Color(0f, 0.5f, 1f), Color.gray, 0.5f);
			yield return Color.Lerp(new Color(0.5f, 0f, 1f), Color.gray, 0.5f);
		}
	}
}
