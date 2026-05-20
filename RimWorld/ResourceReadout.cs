using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld;

public class ResourceReadout
{
	private Vector2 scrollPosition;

	private float lastDrawnHeight;

	private readonly List<ThingCategoryDef> RootThingCategories;

	private const float LineHeightSimple = 24f;

	private const float LineHeightCategorized = 24f;

	private const float DistFromScreenBottom = 200f;

	public ResourceReadout()
	{
		RootThingCategories = DefDatabase<ThingCategoryDef>.AllDefs.Where((ThingCategoryDef cat) => cat.resourceReadoutRoot).ToList();
	}

	public void ResourceReadoutOnGUI()
	{
		if (Event.current.type != EventType.Layout && Current.ProgramState == ProgramState.Playing && Find.MainTabsRoot.OpenTab != MainButtonDefOf.Menu)
		{
			GenUI.DrawTextWinterShadow(new Rect(256f, 512f, -256f, -512f));
			Text.Font = GameFont.Small;
			Rect rect = (Prefs.ResourceReadoutCategorized ? new Rect(2f, 7f, 124f, (float)(UI.screenHeight - 7) - 200f) : new Rect(7f, 7f, 110f, (float)(UI.screenHeight - 7) - 200f));
			Rect rect2 = new Rect(0f, 0f, rect.width, lastDrawnHeight);
			bool num = rect2.height > rect.height;
			if (num)
			{
				Widgets.BeginScrollView(rect, ref scrollPosition, rect2, showScrollbars: false);
			}
			else
			{
				scrollPosition = Vector2.zero;
				Widgets.BeginGroup(rect);
			}
			if (!Prefs.ResourceReadoutCategorized)
			{
				DoReadoutSimple(rect2, rect.height);
			}
			else
			{
				DoReadoutCategorized(rect2);
			}
			if (num)
			{
				Widgets.EndScrollView();
			}
			else
			{
				Widgets.EndGroup();
			}
		}
	}

	private void DoReadoutCategorized(Rect rect)
	{
		Listing_ResourceReadout listing_ResourceReadout = new Listing_ResourceReadout(Find.CurrentMap);
		listing_ResourceReadout.Begin(rect);
		listing_ResourceReadout.nestIndentWidth = 7f;
		listing_ResourceReadout.lineHeight = 24f;
		listing_ResourceReadout.verticalSpacing = 0f;
		for (int i = 0; i < RootThingCategories.Count; i++)
		{
			listing_ResourceReadout.DoCategory(RootThingCategories[i].treeNode, 0, 32);
		}
		listing_ResourceReadout.End();
		lastDrawnHeight = listing_ResourceReadout.CurHeight;
	}

	private void DoReadoutSimple(Rect rect, float outRectHeight)
	{
		Widgets.BeginGroup(rect);
		Text.Anchor = TextAnchor.MiddleLeft;
		float num = 0f;
		foreach (KeyValuePair<ThingDef, int> allCountedAmount in Find.CurrentMap.resourceCounter.AllCountedAmounts)
		{
			if (allCountedAmount.Value > 0 || allCountedAmount.Key.resourceReadoutAlwaysShow)
			{
				Rect rect2 = new Rect(0f, num, 999f, 24f);
				if (rect2.yMax >= scrollPosition.y && rect2.y <= scrollPosition.y + outRectHeight)
				{
					DrawResourceSimple(rect2, allCountedAmount.Key);
				}
				num += 24f;
			}
		}
		Text.Anchor = TextAnchor.UpperLeft;
		lastDrawnHeight = num;
		Widgets.EndGroup();
	}

	public void DrawResourceSimple(Rect rect, ThingDef thingDef)
	{
		DrawIcon(rect.x, rect.y, thingDef);
		rect.y += 2f;
		int count = Find.CurrentMap.resourceCounter.GetCount(thingDef);
		Widgets.Label(new Rect(34f, rect.y, rect.width - 34f, rect.height), count.ToStringCached());
	}

	private void DrawIcon(float x, float y, ThingDef thingDef)
	{
		Rect rect = new Rect(x, y, 27f, 27f);
		Color color = GUI.color;
		Widgets.ThingIcon(rect, thingDef);
		GUI.color = color;
		if (Mouse.IsOver(rect))
		{
			TaggedString taggedString = thingDef.LabelCap + ": " + thingDef.description.CapitalizeFirst();
			TooltipHandler.TipRegion(rect, taggedString);
		}
	}
}
