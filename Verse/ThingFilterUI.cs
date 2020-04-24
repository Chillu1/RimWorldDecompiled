using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse.Sound;

namespace Verse
{
	public static class ThingFilterUI
	{
		private static float viewHeight;

		private const float ExtraViewHeight = 90f;

		private const float RangeLabelTab = 10f;

		private const float RangeLabelHeight = 19f;

		private const float SliderHeight = 28f;

		private const float SliderTab = 20f;

		public static void DoThingFilterConfigWindow(Rect rect, ref Vector2 scrollPosition, ThingFilter filter, ThingFilter parentFilter = null, int openMask = 1, IEnumerable<ThingDef> forceHiddenDefs = null, IEnumerable<SpecialThingFilterDef> forceHiddenFilters = null, bool forceHideHitPointsConfig = false, List<ThingDef> suppressSmallVolumeTags = null, Map map = null)
		{
			Widgets.DrawMenuSection(rect);
			Text.Font = GameFont.Tiny;
			float num = rect.width - 2f;
			Rect rect2 = new Rect(rect.x + 1f, rect.y + 1f, num / 2f, 24f);
			if (Widgets.ButtonText(rect2, "ClearAll".Translate()))
			{
				filter.SetDisallowAll(forceHiddenDefs, forceHiddenFilters);
				SoundDefOf.Checkbox_TurnedOff.PlayOneShotOnCamera();
			}
			if (Widgets.ButtonText(new Rect(rect2.xMax + 1f, rect2.y, rect.xMax - 1f - (rect2.xMax + 1f), 24f), "AllowAll".Translate()))
			{
				filter.SetAllowAll(parentFilter);
				SoundDefOf.Checkbox_TurnedOn.PlayOneShotOnCamera();
			}
			Text.Font = GameFont.Small;
			rect.yMin = rect2.yMax;
			TreeNode_ThingCategory node = ThingCategoryNodeDatabase.RootNode;
			bool flag = true;
			bool flag2 = true;
			if (parentFilter != null)
			{
				node = parentFilter.DisplayRootCategory;
				flag = parentFilter.allowedHitPointsConfigurable;
				flag2 = parentFilter.allowedQualitiesConfigurable;
			}
			Rect viewRect = new Rect(0f, 0f, rect.width - 16f, viewHeight);
			Widgets.BeginScrollView(rect, ref scrollPosition, viewRect);
			float y = 2f;
			if (flag && !forceHideHitPointsConfig)
			{
				DrawHitPointsFilterConfig(ref y, viewRect.width, filter);
			}
			if (flag2)
			{
				DrawQualityFilterConfig(ref y, viewRect.width, filter);
			}
			float num2 = y;
			Rect rect3 = new Rect(0f, y, viewRect.width, 9999f);
			Listing_TreeThingFilter listing_TreeThingFilter = new Listing_TreeThingFilter(filter, parentFilter, forceHiddenDefs, forceHiddenFilters, suppressSmallVolumeTags);
			listing_TreeThingFilter.Begin(rect3);
			listing_TreeThingFilter.DoCategoryChildren(node, 0, openMask, map, isRoot: true);
			listing_TreeThingFilter.End();
			if (Event.current.type == EventType.Layout)
			{
				viewHeight = num2 + listing_TreeThingFilter.CurHeight + 90f;
			}
			Widgets.EndScrollView();
		}

		private static void DrawHitPointsFilterConfig(ref float y, float width, ThingFilter filter)
		{
			Rect rect = new Rect(20f, y, width - 20f, 28f);
			FloatRange range = filter.AllowedHitPointsPercents;
			Widgets.FloatRange(rect, 1, ref range, 0f, 1f, "HitPoints", ToStringStyle.PercentZero);
			filter.AllowedHitPointsPercents = range;
			y += 28f;
			y += 5f;
			Text.Font = GameFont.Small;
		}

		private static void DrawQualityFilterConfig(ref float y, float width, ThingFilter filter)
		{
			Rect rect = new Rect(20f, y, width - 20f, 28f);
			QualityRange range = filter.AllowedQualityLevels;
			Widgets.QualityRange(rect, 876813230, ref range);
			filter.AllowedQualityLevels = range;
			y += 28f;
			y += 5f;
			Text.Font = GameFont.Small;
		}
	}
}
