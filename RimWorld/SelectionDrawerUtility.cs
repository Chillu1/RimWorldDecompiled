using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld;

[StaticConstructorOnStartup]
public static class SelectionDrawerUtility
{
	private const float SelJumpDuration = 0.07f;

	private const float SelJumpDistance = 0.2f;

	public static readonly Texture2D SelectedTexGUI = ContentFinder<Texture2D>.Get("UI/Overlays/SelectionBracketGUI");

	private static Vector2[] bracketLocs = new Vector2[4];

	public static void CalculateSelectionBracketPositionsUI<T>(Vector2[] bracketLocs, T obj, Rect rect, Dictionary<T, float> selectTimes, Vector2 textureSize, float jumpDistanceFactor = 1f)
	{
		float value;
		float num = (selectTimes.TryGetValue(obj, out value) ? Mathf.Max(0f, 1f - (Time.realtimeSinceStartup - value) / 0.07f) : 1f);
		float num2 = num * 0.2f * jumpDistanceFactor;
		float num3 = 0.5f * (rect.width - textureSize.x) + num2;
		float num4 = 0.5f * (rect.height - textureSize.y) + num2;
		bracketLocs[0] = new Vector2(rect.center.x - num3, rect.center.y - num4);
		bracketLocs[1] = new Vector2(rect.center.x + num3, rect.center.y - num4);
		bracketLocs[2] = new Vector2(rect.center.x + num3, rect.center.y + num4);
		bracketLocs[3] = new Vector2(rect.center.x - num3, rect.center.y + num4);
	}

	public static void CalculateSelectionBracketPositionsWorld<T>(Vector3[] bracketLocs, T obj, Vector3 worldPos, Vector2 worldSize, Dictionary<T, float> selectTimes, Vector2 textureSize, float jumpDistanceFactor = 1f, float deselectedJumpFactor = 1f)
	{
		float value;
		float num = (selectTimes.TryGetValue(obj, out value) ? Mathf.Max(0f, 1f - (Time.realtimeSinceStartup - value) / 0.07f) : deselectedJumpFactor);
		float num2 = num * 0.2f * jumpDistanceFactor;
		Vector3 vector = worldPos;
		float num3 = 0.5f * (worldSize.x - textureSize.x) + num2;
		float num4 = 0.5f * (worldSize.y - textureSize.y) + num2;
		float y = AltitudeLayer.MetaOverlays.AltitudeFor();
		if (obj is Thing thing)
		{
			ThingDef thingDef = GenConstruct.BuiltDefOf(thing.def) as ThingDef;
			if (thingDef?.building != null && thingDef.building.isAttachment)
			{
				vector += (thing.Rotation.AsVector2 * 0.5f).ToVector3();
			}
		}
		bracketLocs[0] = new Vector3(vector.x - num3, y, vector.z - num4);
		bracketLocs[1] = new Vector3(vector.x + num3, y, vector.z - num4);
		bracketLocs[2] = new Vector3(vector.x + num3, y, vector.z + num4);
		bracketLocs[3] = new Vector3(vector.x - num3, y, vector.z + num4);
	}

	public static void DrawSelectionOverlayOnGUI(object target, Rect rect, float scale, float selectedTextJump)
	{
		CalculateSelectionBracketPositionsUI(textureSize: new Vector2((float)SelectedTexGUI.width * scale, (float)SelectedTexGUI.height * scale), bracketLocs: bracketLocs, obj: target, rect: rect, selectTimes: SelectionDrawer.SelectTimes, jumpDistanceFactor: selectedTextJump);
		DrawSelectionOverlayOnGUI(bracketLocs, scale);
	}

	public static void DrawSelectionOverlayOnGUI(Vector2[] bracketLocs, float selectedTexScale)
	{
		int num = 90;
		for (int i = 0; i < 4; i++)
		{
			Widgets.DrawTextureRotated(bracketLocs[i], SelectedTexGUI, num, selectedTexScale);
			num += 90;
		}
	}

	public static void DrawSelectionOverlayWholeGUI(Rect rect)
	{
		GUI.DrawTexture(rect, TexUI.SelectionBracketWhole);
	}
}
