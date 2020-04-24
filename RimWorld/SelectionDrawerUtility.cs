using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld
{
	[StaticConstructorOnStartup]
	public static class SelectionDrawerUtility
	{
		private const float SelJumpDuration = 0.07f;

		private const float SelJumpDistance = 0.2f;

		public static readonly Texture2D SelectedTexGUI = ContentFinder<Texture2D>.Get("UI/Overlays/SelectionBracketGUI");

		public static void CalculateSelectionBracketPositionsUI<T>(Vector2[] bracketLocs, T obj, Rect rect, Dictionary<T, float> selectTimes, Vector2 textureSize, float jumpDistanceFactor = 1f)
		{
			float value;
			float num = selectTimes.TryGetValue(obj, out value) ? Mathf.Max(0f, 1f - (Time.realtimeSinceStartup - value) / 0.07f) : 1f;
			float num2 = num * 0.2f * jumpDistanceFactor;
			float num3 = 0.5f * (rect.width - textureSize.x) + num2;
			float num4 = 0.5f * (rect.height - textureSize.y) + num2;
			bracketLocs[0] = new Vector2(rect.center.x - num3, rect.center.y - num4);
			bracketLocs[1] = new Vector2(rect.center.x + num3, rect.center.y - num4);
			bracketLocs[2] = new Vector2(rect.center.x + num3, rect.center.y + num4);
			bracketLocs[3] = new Vector2(rect.center.x - num3, rect.center.y + num4);
		}

		public static void CalculateSelectionBracketPositionsWorld<T>(Vector3[] bracketLocs, T obj, Vector3 worldPos, Vector2 worldSize, Dictionary<T, float> selectTimes, Vector2 textureSize, float jumpDistanceFactor = 1f)
		{
			float value;
			float num = selectTimes.TryGetValue(obj, out value) ? Mathf.Max(0f, 1f - (Time.realtimeSinceStartup - value) / 0.07f) : 1f;
			float num2 = num * 0.2f * jumpDistanceFactor;
			float num3 = 0.5f * (worldSize.x - textureSize.x) + num2;
			float num4 = 0.5f * (worldSize.y - textureSize.y) + num2;
			float y = AltitudeLayer.MetaOverlays.AltitudeFor();
			bracketLocs[0] = new Vector3(worldPos.x - num3, y, worldPos.z - num4);
			bracketLocs[1] = new Vector3(worldPos.x + num3, y, worldPos.z - num4);
			bracketLocs[2] = new Vector3(worldPos.x + num3, y, worldPos.z + num4);
			bracketLocs[3] = new Vector3(worldPos.x - num3, y, worldPos.z + num4);
		}
	}
}
