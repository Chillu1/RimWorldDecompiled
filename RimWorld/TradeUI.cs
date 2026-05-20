using UnityEngine;
using Verse;

namespace RimWorld;

[StaticConstructorOnStartup]
public static class TradeUI
{
	public const float CountColumnWidth = 75f;

	public const float PriceColumnWidth = 100f;

	public const float AdjustColumnWidth = 240f;

	public const float TotalNumbersColumnsWidths = 590f;

	public static readonly Color NoTradeColor = new Color(0.5f, 0.5f, 0.5f);

	public static void DrawTradeableRow(Rect rect, Tradeable trad, int index)
	{
		if (index % 2 == 1)
		{
			Widgets.DrawLightHighlight(rect);
		}
		Text.Font = GameFont.Small;
		Widgets.BeginGroup(rect);
		float width = rect.width;
		int num = trad.CountHeldBy(Transactor.Trader);
		if (num != 0 && trad.IsThing)
		{
			Rect rect2 = new Rect(width - 75f, 0f, 75f, rect.height);
			if (Mouse.IsOver(rect2))
			{
				Widgets.DrawHighlight(rect2);
			}
			Text.Anchor = TextAnchor.MiddleRight;
			Rect rect3 = rect2;
			rect3.xMin += 5f;
			rect3.xMax -= 5f;
			Widgets.Label(rect3, num.ToStringCached());
			TooltipHandler.TipRegionByKey(rect2, "TraderCount");
			Rect rect4 = new Rect(rect2.x - 100f, 0f, 100f, rect.height);
			Text.Anchor = TextAnchor.MiddleRight;
			DrawPrice(rect4, trad, TradeAction.PlayerBuys);
		}
		width -= 175f;
		Rect rect5 = new Rect(width - 240f, 0f, 240f, rect.height);
		if (!trad.TraderWillTrade)
		{
			DrawWillNotTradeText(rect5, "TraderWillNotTrade".Translate());
		}
		else if (ModsConfig.IdeologyActive && TransferableUIUtility.TradeIsPlayerSellingToSlavery(trad, TradeSession.trader.Faction) && !new HistoryEvent(HistoryEventDefOf.SoldSlave, TradeSession.playerNegotiator.Named(HistoryEventArgsNames.Doer)).DoerWillingToDo())
		{
			DrawWillNotTradeText(rect5, "NegotiatorWillNotTradeSlaves".Translate(TradeSession.playerNegotiator));
			if (Mouse.IsOver(rect5))
			{
				Widgets.DrawHighlight(rect5);
				TooltipHandler.TipRegion(rect5, "NegotiatorWillNotTradeSlavesTip".Translate(TradeSession.playerNegotiator, TradeSession.playerNegotiator.Ideo.name));
			}
		}
		else
		{
			bool flash = Time.time - Dialog_Trade.lastCurrencyFlashTime < 1f && trad.IsCurrency;
			TransferableUIUtility.DoCountAdjustInterface(rect5, trad, index, trad.GetMinimumToTransfer(), trad.GetMaximumToTransfer(), flash);
		}
		width -= 240f;
		int num2 = trad.CountHeldBy(Transactor.Colony);
		if (num2 != 0 || trad.IsCurrency)
		{
			Rect rect6 = new Rect(width - 100f, 0f, 100f, rect.height);
			Text.Anchor = TextAnchor.MiddleLeft;
			DrawPrice(rect6, trad, TradeAction.PlayerSells);
			Rect rect7 = new Rect(rect6.x - 75f, 0f, 75f, rect.height);
			if (Mouse.IsOver(rect7))
			{
				Widgets.DrawHighlight(rect7);
			}
			Text.Anchor = TextAnchor.MiddleLeft;
			Rect rect8 = rect7;
			rect8.xMin += 5f;
			rect8.xMax -= 5f;
			Widgets.Label(rect8, num2.ToStringCached());
			TooltipHandler.TipRegionByKey(rect7, "ColonyCount");
		}
		width -= 175f;
		TransferableUIUtility.DoExtraIcons(trad, rect, ref width);
		if (ModsConfig.IdeologyActive)
		{
			TransferableUIUtility.DrawCaptiveTradeInfo(trad, TradeSession.trader, rect, ref width);
		}
		Rect idRect = new Rect(0f, 0f, width, rect.height);
		TransferableUIUtility.DrawTransferableInfo(trad, idRect, trad.TraderWillTrade ? Color.white : NoTradeColor);
		GenUI.ResetLabelAlign();
		Widgets.EndGroup();
	}

	private static void DrawPrice(Rect rect, Tradeable trad, TradeAction action)
	{
		if (trad.IsCurrency || !trad.TraderWillTrade)
		{
			return;
		}
		rect = rect.Rounded();
		if (Mouse.IsOver(rect))
		{
			Widgets.DrawHighlight(rect);
		}
		if (Mouse.IsOver(rect))
		{
			TooltipHandler.TipRegion(rect, new TipSignal(() => trad.GetPriceTooltip(action), trad.GetHashCode() * 297));
		}
		if (action == TradeAction.PlayerBuys)
		{
			switch (trad.PriceTypeFor(action))
			{
			case PriceType.VeryCheap:
				GUI.color = new Color(0f, 1f, 0f);
				break;
			case PriceType.Cheap:
				GUI.color = new Color(0.5f, 1f, 0.5f);
				break;
			case PriceType.Normal:
				GUI.color = Color.white;
				break;
			case PriceType.Expensive:
				GUI.color = new Color(1f, 0.5f, 0.5f);
				break;
			case PriceType.Exorbitant:
				GUI.color = new Color(1f, 0f, 0f);
				break;
			}
		}
		else
		{
			switch (trad.PriceTypeFor(action))
			{
			case PriceType.VeryCheap:
				GUI.color = new Color(1f, 0f, 0f);
				break;
			case PriceType.Cheap:
				GUI.color = new Color(1f, 0.5f, 0.5f);
				break;
			case PriceType.Normal:
				GUI.color = Color.white;
				break;
			case PriceType.Expensive:
				GUI.color = new Color(0.5f, 1f, 0.5f);
				break;
			case PriceType.Exorbitant:
				GUI.color = new Color(0f, 1f, 0f);
				break;
			}
		}
		float priceFor = trad.GetPriceFor(action);
		string label = ((TradeSession.TradeCurrency == TradeCurrency.Silver) ? priceFor.ToStringMoney() : priceFor.ToString());
		Rect rect2 = new Rect(rect);
		rect2.xMax -= 5f;
		rect2.xMin += 5f;
		if (Text.Anchor == TextAnchor.MiddleLeft)
		{
			rect2.xMax += 300f;
		}
		if (Text.Anchor == TextAnchor.MiddleRight)
		{
			rect2.xMin -= 300f;
		}
		Widgets.Label(rect2, label);
		GUI.color = Color.white;
	}

	private static void DrawWillNotTradeText(Rect rect, string text)
	{
		rect.height += 4f;
		rect = rect.Rounded();
		GUI.color = NoTradeColor;
		Text.Font = GameFont.Tiny;
		Text.Anchor = TextAnchor.MiddleCenter;
		Widgets.Label(rect, text);
		Text.Anchor = TextAnchor.UpperLeft;
		Text.Font = GameFont.Small;
		GUI.color = Color.white;
	}
}
