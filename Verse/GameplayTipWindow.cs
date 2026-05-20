using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse.Steam;

namespace Verse;

public class GameplayTipWindow
{
	private static List<string> allTipsCached;

	private static float lastTimeUpdatedTooltip = -1f;

	private static int currentTipIndex = 0;

	public const float tipUpdateInterval = 17.5f;

	public static readonly Vector2 WindowSize = new Vector2(776f, 60f);

	private static readonly Vector2 TextMargin = new Vector2(15f, 8f);

	private const int InterfaceTipsCount = 11;

	public static void DrawWindow(Vector2 offset, bool useWindowStack)
	{
		if (allTipsCached == null)
		{
			allTipsCached = DefDatabase<TipSetDef>.AllDefsListForReading.SelectMany((TipSetDef set) => (SteamDeck.IsSteamDeck && set == TipSetDefOf.GameplayTips) ? set.tips.Skip(11) : set.tips).InRandomOrder().ToList();
		}
		Rect rect = new Rect(offset.x, offset.y, WindowSize.x, WindowSize.y);
		if (useWindowStack)
		{
			Find.WindowStack.ImmediateWindow(62893997, rect, WindowLayer.Super, delegate
			{
				DrawContents(rect.AtZero());
			});
		}
		else
		{
			Widgets.DrawShadowAround(rect);
			Widgets.DrawWindowBackground(rect);
			DrawContents(rect);
		}
	}

	private static void DrawContents(Rect rect)
	{
		Text.Font = GameFont.Small;
		Text.Anchor = TextAnchor.MiddleCenter;
		if (Time.realtimeSinceStartup - lastTimeUpdatedTooltip > 17.5f || lastTimeUpdatedTooltip < 0f)
		{
			currentTipIndex = (currentTipIndex + 1) % allTipsCached.Count;
			lastTimeUpdatedTooltip = Time.realtimeSinceStartup;
		}
		Rect rect2 = rect;
		rect2.x += TextMargin.x;
		rect2.width -= TextMargin.x * 2f;
		rect2.y += TextMargin.y;
		rect2.height -= TextMargin.y * 2f;
		Widgets.Label(rect2, allTipsCached[currentTipIndex]);
		Text.Anchor = TextAnchor.UpperLeft;
	}

	public static void ResetTipTimer()
	{
		lastTimeUpdatedTooltip = -1f;
	}
}
