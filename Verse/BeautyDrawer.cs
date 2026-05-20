using System.Collections.Generic;
using RimWorld;
using UnityEngine;

namespace Verse;

public static class BeautyDrawer
{
	private static HashSet<Thing> beautyCountedThings = new HashSet<Thing>();

	private static Color ColorUgly = Color.red;

	private static Color ColorBeautiful = Color.green;

	public static void BeautyDrawerOnGUI()
	{
		if (Event.current.type == EventType.Repaint && ShouldShow())
		{
			DrawBeautyAroundMouse();
		}
	}

	private static bool ShouldShow()
	{
		if (Mouse.IsInputBlockedNow)
		{
			return false;
		}
		if (!UI.MouseCell().InBounds(Find.CurrentMap) || UI.MouseCell().Fogged(Find.CurrentMap))
		{
			return false;
		}
		if (CellInspectorDrawer.active)
		{
			return true;
		}
		if (!Find.PlaySettings.showBeauty)
		{
			return false;
		}
		return true;
	}

	private static void DrawBeautyAroundMouse()
	{
		BeautyUtility.FillBeautyRelevantCells(UI.MouseCell(), Find.CurrentMap);
		for (int i = 0; i < BeautyUtility.beautyRelevantCells.Count; i++)
		{
			IntVec3 intVec = BeautyUtility.beautyRelevantCells[i];
			float num = BeautyUtility.CellBeauty(intVec, Find.CurrentMap, beautyCountedThings);
			if (num != 0f)
			{
				GenMapUI.DrawThingLabel((Vector3)GenMapUI.LabelDrawPosFor(intVec), Mathf.RoundToInt(num).ToStringCached(), BeautyColor(num, 8f));
			}
		}
		beautyCountedThings.Clear();
	}

	public static Color BeautyColor(float beauty, float scale)
	{
		float value = Mathf.InverseLerp(0f - scale, scale, beauty);
		value = Mathf.Clamp01(value);
		return Color.Lerp(Color.Lerp(ColorUgly, ColorBeautiful, value), Color.white, 0.5f);
	}
}
