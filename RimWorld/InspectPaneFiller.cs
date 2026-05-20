using System;
using UnityEngine;
using Verse;

namespace RimWorld;

[StaticConstructorOnStartup]
public class InspectPaneFiller
{
	private const float BarHeight = 16f;

	private static readonly Texture2D MoodTex = SolidColorMaterials.NewSolidColorTexture(new ColorInt(26, 52, 52).ToColor);

	private static readonly Texture2D BarBGTex = SolidColorMaterials.NewSolidColorTexture(new ColorInt(10, 10, 10).ToColor);

	private static readonly Texture2D HealthTex = SolidColorMaterials.NewSolidColorTexture(new ColorInt(35, 35, 35).ToColor);

	private static readonly Texture2D EnergyTex = SolidColorMaterials.NewSolidColorTexture(new ColorInt(0, 52, 75).ToColor);

	private static readonly Texture2D HungerTex = SolidColorMaterials.NewSolidColorTexture(new ColorInt(184, 156, 90).ToColor);

	private const float BarWidth = 93f;

	private const float BarSpacing = 6f;

	private static bool debug_inspectStringExceptionErrored;

	private static Vector2 inspectStringScrollPos;

	public static void DoPaneContentsFor(ISelectable sel, Rect rect)
	{
		try
		{
			Widgets.BeginGroup(rect);
			float num = 0f;
			Pawn pawn = sel as Pawn;
			if (sel is Thing thing && !thing.def.onlyShowInspectString)
			{
				num += 3f;
				WidgetRow row = new WidgetRow(0f, num);
				DrawHealth(row, thing);
				if (pawn != null)
				{
					if (pawn.IsGhoul && pawn.needs.food != null)
					{
						DrawHunger(row, pawn);
					}
					else
					{
						DrawMood(row, pawn);
					}
					if (pawn.timetable != null && !pawn.IsPrisonerOfColony)
					{
						DrawTimetableSetting(row, pawn);
					}
					DrawAreaAllowed(row, pawn);
					if (pawn.needs?.energy != null)
					{
						DrawMechEnergy(row, pawn);
					}
				}
				num += 18f;
			}
			Rect rect2 = rect.AtZero();
			rect2.yMin = num;
			DrawInspectStringFor(sel, rect2);
		}
		catch (Exception arg)
		{
			Log.ErrorOnce($"Error in DoPaneContentsFor {Find.Selector.FirstSelectedObject}: {arg}", 754672);
		}
		finally
		{
			Widgets.EndGroup();
		}
	}

	public static void DoPaneContentsForStorageGroup(StorageGroup group, Rect rect)
	{
		Widgets.BeginGroup(rect);
		string text = string.Format("\n{0}: {1} ", "StorageGroupLabel".Translate(), group.RenamableLabel.CapitalizeFirst());
		text = ((group.MemberCount <= 1) ? (text + string.Format("({0})", "OneBuilding".Translate())) : (text + string.Format("({0})", "NumBuildings".Translate(group.MemberCount))));
		DrawInspectString(text, rect.AtZero());
		Widgets.EndGroup();
	}

	public static void DrawHealth(WidgetRow row, Thing t)
	{
		float fillPct;
		string label;
		if (!(t is Pawn pawn))
		{
			if (!t.def.useHitPoints)
			{
				return;
			}
			if (t.HitPoints >= t.MaxHitPoints)
			{
				GUI.color = Color.white;
			}
			else if ((float)t.HitPoints > (float)t.MaxHitPoints * 0.5f)
			{
				GUI.color = Color.yellow;
			}
			else if (t.HitPoints > 0)
			{
				GUI.color = Color.red;
			}
			else
			{
				GUI.color = Color.grey;
			}
			fillPct = (float)t.HitPoints / (float)t.MaxHitPoints;
			label = t.HitPoints.ToStringCached() + " / " + t.MaxHitPoints.ToStringCached();
		}
		else
		{
			GUI.color = Color.white;
			fillPct = pawn.health.summaryHealth.SummaryHealthPercent;
			label = HealthUtility.GetGeneralConditionLabel(pawn, shortVersion: true);
		}
		row.FillableBar(93f, 16f, fillPct, label, HealthTex, BarBGTex);
		GUI.color = Color.white;
	}

	private static void DrawMood(WidgetRow row, Pawn pawn)
	{
		if (pawn.needs != null && pawn.needs.mood != null)
		{
			row.Gap(6f);
			row.FillableBar(93f, 16f, pawn.needs.mood.CurLevelPercentage, pawn.needs.mood.MoodString.CapitalizeFirst(), MoodTex, BarBGTex);
		}
	}

	private static void DrawTimetableSetting(WidgetRow row, Pawn pawn)
	{
		row.Gap(6f);
		row.FillableBar(93f, 16f, 1f, pawn.timetable.CurrentAssignment.LabelCap, pawn.timetable.CurrentAssignment.ColorTexture);
	}

	private static void DrawAreaAllowed(WidgetRow row, Pawn pawn)
	{
		if (pawn.playerSettings == null || !pawn.playerSettings.SupportsAllowedAreas || pawn.Faction != Faction.OfPlayer || pawn.HostFaction != null || (pawn.IsMutant && !pawn.mutant.Def.respectsAllowedArea))
		{
			return;
		}
		row.Gap(6f);
		bool flag = pawn.playerSettings?.AreaRestrictionInPawnCurrentMap != null;
		Rect rect = row.FillableBar(fillTex: (!flag) ? BaseContent.GreyTex : pawn.playerSettings.AreaRestrictionInPawnCurrentMap.ColorTexture, width: 93f, height: 16f, fillPct: 1f, label: AreaUtility.AreaAllowedLabel(pawn));
		if (Mouse.IsOver(rect))
		{
			if (flag)
			{
				pawn.playerSettings.AreaRestrictionInPawnCurrentMap.MarkForDraw();
			}
			Widgets.DrawBox(rect.ContractedBy(-1f));
		}
		if (Widgets.ButtonInvisible(rect))
		{
			AreaUtility.MakeAllowedAreaListFloatMenu(delegate(Area a)
			{
				pawn.playerSettings.AreaRestrictionInPawnCurrentMap = a;
			}, addNullAreaOption: true, addManageOption: true, pawn.MapHeld);
		}
	}

	private static void DrawMechEnergy(WidgetRow row, Pawn pawn)
	{
		row.Gap(6f);
		row.FillableBar(93f, 16f, pawn.needs.energy.CurLevelPercentage, "Energy".Translate(), EnergyTex, BarBGTex);
	}

	private static void DrawHunger(WidgetRow row, Pawn pawn)
	{
		row.Gap(6f);
		row.FillableBar(93f, 16f, pawn.needs.food.CurLevelPercentage, pawn.needs.food.CurCategory.GetLabel(), HungerTex, BarBGTex);
	}

	public static void DrawInspectStringFor(ISelectable sel, Rect rect)
	{
		string text;
		try
		{
			text = sel.GetInspectString();
			if (sel is Thing thing)
			{
				string inspectStringLowPriority = thing.GetInspectStringLowPriority();
				if (!inspectStringLowPriority.NullOrEmpty())
				{
					if (!text.NullOrEmpty())
					{
						text = text.TrimEndNewlines() + "\n";
					}
					text += inspectStringLowPriority;
				}
			}
		}
		catch (Exception arg)
		{
			text = $"GetInspectString exception on {sel}:\n{arg}";
			if (!debug_inspectStringExceptionErrored)
			{
				Log.Error(text);
				debug_inspectStringExceptionErrored = true;
			}
		}
		if (!text.NullOrEmpty() && GenText.ContainsEmptyLines(text))
		{
			Log.ErrorOnce($"Inspect string for {sel} contains empty lines.\n\nSTART\n{text}\nEND", 837163521);
		}
		DrawInspectString(text, rect);
	}

	public static void DrawInspectString(string str, Rect rect)
	{
		Text.Font = GameFont.Small;
		Widgets.LabelScrollable(rect, str, ref inspectStringScrollPos, dontConsumeScrollEventsIfNoScrollbar: true);
	}
}
