using UnityEngine;
using Verse;

namespace RimWorld;

public static class ScenarioUI
{
	private static float editViewHeight;

	public static void DrawScenarioInfo(Rect rect, Scenario scen, ref Vector2 infoScrollPosition)
	{
		Widgets.DrawMenuSection(rect);
		rect = rect.GetInnerRect();
		if (scen != null)
		{
			string fullInformationText = scen.GetFullInformationText();
			float width = rect.width - 16f;
			float height = 30f + Text.CalcHeight(fullInformationText, width) + 100f;
			Rect viewRect = new Rect(0f, 0f, width, height);
			Widgets.BeginScrollView(rect, ref infoScrollPosition, viewRect);
			Text.Font = GameFont.Medium;
			Widgets.Label(new Rect(0f, 0f, viewRect.width, 30f), scen.name);
			Text.Font = GameFont.Small;
			Widgets.Label(new Rect(0f, 30f, viewRect.width, viewRect.height - 30f), fullInformationText);
			Widgets.EndScrollView();
		}
	}

	public static void DrawScenarioEditInterface(Rect rect, Scenario scen, ref Vector2 infoScrollPosition)
	{
		Widgets.DrawMenuSection(rect);
		rect = rect.GetInnerRect();
		if (scen == null)
		{
			return;
		}
		Rect viewRect = new Rect(0f, 0f, rect.width - 16f, editViewHeight);
		Widgets.BeginScrollView(rect, ref infoScrollPosition, viewRect);
		Rect rect2 = new Rect(0f, 0f, viewRect.width, 99999f);
		Listing_ScenEdit listing_ScenEdit = new Listing_ScenEdit(scen);
		listing_ScenEdit.ColumnWidth = rect2.width;
		listing_ScenEdit.Begin(rect2);
		listing_ScenEdit.Label("ScenarioTitle".Translate());
		scen.name = listing_ScenEdit.TextEntry(scen.name).TrimmedToLength(55);
		listing_ScenEdit.Label("Summary".Translate());
		scen.summary = listing_ScenEdit.TextEntry(scen.summary, 2).TrimmedToLength(300);
		listing_ScenEdit.Label("Description".Translate());
		scen.description = listing_ScenEdit.TextEntry(scen.description, 4).TrimmedToLength(1000);
		listing_ScenEdit.Gap();
		foreach (ScenPart allPart in scen.AllParts)
		{
			allPart.DoEditInterface(listing_ScenEdit);
		}
		listing_ScenEdit.End();
		editViewHeight = listing_ScenEdit.CurHeight + 100f;
		Widgets.EndScrollView();
	}
}
