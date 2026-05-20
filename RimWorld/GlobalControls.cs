using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace RimWorld;

public class GlobalControls
{
	public const float Width = 200f;

	private WidgetRow rowVisibility = new WidgetRow();

	private static string indoorsUnroofedStringCached;

	private static int indoorsUnroofedStringCachedRoofCount = -1;

	private static string cachedTemperatureString;

	private static string cachedTemperatureStringForLabel;

	private static float cachedTemperatureStringForTemperature;

	private static TemperatureDisplayMode cachedTemperatureDisplayMode;

	public void GlobalControlsOnGUI()
	{
		if (Event.current.type != EventType.Layout)
		{
			float num = (float)UI.screenWidth - 200f;
			float num2 = UI.screenHeight;
			num2 -= 35f;
			GenUI.DrawTextWinterShadow(new Rect(UI.screenWidth - 270, UI.screenHeight - 450, 270f, 450f));
			num2 -= 4f;
			GlobalControlsUtility.DoPlaySettings(rowVisibility, worldView: false, ref num2);
			num2 -= 4f;
			GlobalControlsUtility.DoTimespeedControls(num, 200f, ref num2);
			num2 -= 4f;
			GlobalControlsUtility.DoDate(num, 200f, ref num2);
			if (!Find.CurrentMap.IsPocketMap)
			{
				Rect rect = new Rect(num - 22f, num2 - 26f, 230f, 26f);
				Find.CurrentMap.weatherManager.DoWeatherGUI(rect);
				num2 -= rect.height;
			}
			Rect rect2 = new Rect(num - 100f, num2 - 26f, 293f, 26f);
			Text.Anchor = TextAnchor.MiddleRight;
			Widgets.Label(rect2, TemperatureString());
			Text.Anchor = TextAnchor.UpperLeft;
			num2 -= 26f;
			float num3 = 154f;
			float num4 = Find.CurrentMap.gameConditionManager.TotalHeightAt(num3);
			Rect rect3 = new Rect((float)UI.screenWidth - num3, num2 - num4, num3, num4);
			Find.CurrentMap.gameConditionManager.DoConditionsUI(rect3);
			num2 -= rect3.height;
			if (DebugViewSettings.showMemoryInfo)
			{
				GlobalControlsUtility.DrawMemoryInfo(num, 200f, ref num2);
			}
			if (DebugViewSettings.showTpsCounter)
			{
				GlobalControlsUtility.DrawTpsCounter(num, 200f, ref num2);
			}
			if (DebugViewSettings.showFpsCounter)
			{
				GlobalControlsUtility.DrawFpsCounter(num, 200f, ref num2);
			}
			if (Prefs.ShowRealtimeClock)
			{
				GlobalControlsUtility.DoRealtimeClock(num, 200f, ref num2);
			}
			TimedDetectionRaids timedDetectionRaids = Find.CurrentMap.Parent?.GetComponent<TimedDetectionRaids>();
			if (timedDetectionRaids != null && timedDetectionRaids.NextRaidCountdownActiveAndVisible)
			{
				Rect rect4 = new Rect(num, num2 - 26f, 193f, 26f);
				Text.Anchor = TextAnchor.MiddleRight;
				DoCountdownTimer(rect4, timedDetectionRaids);
				Text.Anchor = TextAnchor.UpperLeft;
				num2 -= 26f;
			}
			num2 -= 10f;
			Find.LetterStack.LettersOnGUI(num2);
		}
	}

	private static string TemperatureString()
	{
		IntVec3 intVec = UI.MouseCell();
		IntVec3 c = intVec;
		Room room = intVec.GetRoom(Find.CurrentMap);
		if (room == null)
		{
			for (int i = 0; i < 9; i++)
			{
				IntVec3 intVec2 = intVec + GenAdj.AdjacentCellsAndInside[i];
				if (intVec2.InBounds(Find.CurrentMap))
				{
					Room room2 = intVec2.GetRoom(Find.CurrentMap);
					if (room2 != null && ((!room2.PsychologicallyOutdoors && !room2.UsesOutdoorTemperature) || (!room2.PsychologicallyOutdoors && (room == null || room.PsychologicallyOutdoors)) || (room2.PsychologicallyOutdoors && room == null)))
					{
						c = intVec2;
						room = room2;
					}
				}
			}
		}
		if (room == null && intVec.InBounds(Find.CurrentMap))
		{
			Building edifice = intVec.GetEdifice(Find.CurrentMap);
			if (edifice != null)
			{
				foreach (IntVec3 item in edifice.OccupiedRect().ExpandedBy(1).ClipInsideMap(Find.CurrentMap))
				{
					room = item.GetRoom(Find.CurrentMap);
					if (room != null && !room.PsychologicallyOutdoors)
					{
						c = item;
						break;
					}
				}
			}
		}
		string text;
		if (c.InBounds(Find.CurrentMap) && !c.Fogged(Find.CurrentMap) && room != null && !room.PsychologicallyOutdoors)
		{
			if (room.OpenRoofCount == 0)
			{
				text = "Indoors".Translate();
			}
			else
			{
				if (indoorsUnroofedStringCachedRoofCount != room.OpenRoofCount)
				{
					indoorsUnroofedStringCached = "IndoorsUnroofed".Translate() + " (" + room.OpenRoofCount.ToStringCached() + ")";
					indoorsUnroofedStringCachedRoofCount = room.OpenRoofCount;
				}
				text = indoorsUnroofedStringCached;
			}
		}
		else
		{
			text = "Outdoors".Translate().CapitalizeFirst();
		}
		float num = ((room == null || c.Fogged(Find.CurrentMap)) ? Find.CurrentMap.mapTemperature.OutdoorTemp : room.Temperature);
		int num2 = Mathf.RoundToInt(GenTemperature.CelsiusTo(cachedTemperatureStringForTemperature, Prefs.TemperatureMode));
		int num3 = Mathf.RoundToInt(GenTemperature.CelsiusTo(num, Prefs.TemperatureMode));
		if (cachedTemperatureStringForLabel != text || num2 != num3 || cachedTemperatureDisplayMode != Prefs.TemperatureMode)
		{
			cachedTemperatureStringForLabel = text;
			cachedTemperatureStringForTemperature = num;
			cachedTemperatureString = text + " " + num.ToStringTemperature("F0");
			cachedTemperatureDisplayMode = Prefs.TemperatureMode;
		}
		return cachedTemperatureString;
	}

	private static void DoCountdownTimer(Rect rect, TimedDetectionRaids timedForcedExit)
	{
		string detectionCountdownTimeLeftString = timedForcedExit.DetectionCountdownTimeLeftString;
		string text = "CaravanDetectedRaidCountdown".Translate(detectionCountdownTimeLeftString);
		float x = Text.CalcSize(text).x;
		Rect rect2 = new Rect(rect.xMax - x, rect.y, x, rect.height);
		if (Mouse.IsOver(rect2))
		{
			Widgets.DrawHighlight(rect2);
		}
		TooltipHandler.TipRegionByKey(rect2, "CaravanDetectedRaidCountdownTip", detectionCountdownTimeLeftString);
		Widgets.Label(rect2, text);
	}
}
