using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld;

public class Dialog_RewardPrefsConfig : Window
{
	private Vector2 scrollPosition;

	private float viewRectHeight;

	private const float TitleHeight = 40f;

	private const float RowHeight = 45f;

	private const float IconSize = 35f;

	private const float GoodwillWidth = 100f;

	private const float CheckboxOffset = 150f;

	private const float FactionNameWidth = 250f;

	public override Vector2 InitialSize => new Vector2(700f, 440f);

	public Dialog_RewardPrefsConfig()
	{
		forcePause = true;
		doCloseX = true;
		doCloseButton = true;
		absorbInputAroundWindow = true;
		closeOnClickedOutside = true;
	}

	public override void DoWindowContents(Rect inRect)
	{
		Text.Font = GameFont.Medium;
		Widgets.Label(new Rect(0f, 0f, InitialSize.x / 2f, 40f), "ChooseRewards".Translate());
		Text.Font = GameFont.Small;
		string text = "ChooseRewardsDesc".Translate();
		float height = Text.CalcHeight(text, inRect.width);
		Rect rect = new Rect(0f, 40f, inRect.width, height);
		Widgets.Label(rect, text);
		IEnumerable<Faction> allFactionsVisibleInViewOrder = Find.FactionManager.AllFactionsVisibleInViewOrder;
		Rect outRect = new Rect(inRect);
		outRect.yMax -= Window.CloseButSize.y;
		outRect.yMin += 44f + rect.height + 4f;
		float curY = 0f;
		Rect rect2 = new Rect(0f, curY, outRect.width - 16f, viewRectHeight);
		Widgets.BeginScrollView(outRect, ref scrollPosition, rect2);
		int index = 0;
		foreach (Faction item in allFactionsVisibleInViewOrder)
		{
			if (item.IsPlayer)
			{
				continue;
			}
			float curX = 0f;
			if (item.def.HasRoyalTitles)
			{
				DoFactionInfo(rect2, item, ref curX, ref curY, ref index);
				TaggedString label = "AcceptRoyalFavor".Translate(item.Named("FACTION")).CapitalizeFirst();
				Rect rect3 = new Rect(curX, curY, label.GetWidthCached(), 45f);
				Text.Anchor = TextAnchor.MiddleLeft;
				Widgets.Label(rect3, label);
				Text.Anchor = TextAnchor.UpperLeft;
				if (Mouse.IsOver(rect3))
				{
					TooltipHandler.TipRegion(rect3, "AcceptRoyalFavorDesc".Translate(item.Named("FACTION")));
					Widgets.DrawHighlight(rect3);
				}
				Widgets.Checkbox(rect2.width - 150f, curY + 12f, ref item.allowRoyalFavorRewards, 24f, disabled: false, paintable: true);
				curY += 45f;
			}
			if (item.CanEverGiveGoodwillRewards)
			{
				curX = 0f;
				DoFactionInfo(rect2, item, ref curX, ref curY, ref index);
				TaggedString label2 = "AcceptGoodwill".Translate().CapitalizeFirst();
				Rect rect4 = new Rect(curX, curY, label2.GetWidthCached(), 45f);
				Text.Anchor = TextAnchor.MiddleLeft;
				Widgets.Label(rect4, label2);
				Text.Anchor = TextAnchor.UpperLeft;
				if (Mouse.IsOver(rect4))
				{
					TooltipHandler.TipRegion(rect4, "AcceptGoodwillDesc".Translate(item.Named("FACTION")));
					Widgets.DrawHighlight(rect4);
				}
				Widgets.Checkbox(rect2.width - 150f, curY + 12f, ref item.allowGoodwillRewards, 24f, disabled: false, paintable: true);
				Widgets.Label(new Rect(rect2.width - 100f, curY, 100f, 35f), (item.PlayerGoodwill.ToStringWithSign() + "\n" + item.PlayerRelationKind.GetLabelCap()).Colorize(item.PlayerRelationKind.GetColor()));
				curY += 45f;
			}
		}
		if (Event.current.type == EventType.Layout)
		{
			viewRectHeight = curY;
		}
		Widgets.EndScrollView();
	}

	private void DoFactionInfo(Rect rect, Faction faction, ref float curX, ref float curY, ref int index)
	{
		if (index % 2 == 1)
		{
			Widgets.DrawLightHighlight(new Rect(curX, curY, rect.width, 45f));
		}
		FactionUIUtility.DrawFactionIconWithTooltip(new Rect(curX, curY + 5f, 35f, 35f), faction);
		curX += 45f;
		Rect rect2 = new Rect(curX, curY, 250f, 45f);
		Text.Anchor = TextAnchor.MiddleLeft;
		Widgets.Label(rect2, faction.Name);
		Text.Anchor = TextAnchor.UpperLeft;
		curX += 250f;
		if (Mouse.IsOver(rect2))
		{
			TipSignal tip = new TipSignal(() => faction.Name + "\n\n" + faction.def.Description + "\n\n" + faction.PlayerRelationKind.GetLabelCap().Colorize(faction.PlayerRelationKind.GetColor()), faction.loadID ^ 0x4468077);
			TooltipHandler.TipRegion(rect2, tip);
			Widgets.DrawHighlight(rect2);
		}
		index++;
	}
}
