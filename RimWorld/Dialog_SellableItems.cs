using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class Dialog_SellableItems : Window
	{
		private ThingCategoryDef currentCategory;

		private bool pawnsTabOpen;

		private List<ThingDef> sellableItems = new List<ThingDef>();

		private List<TabRecord> tabs = new List<TabRecord>();

		private Vector2 scrollPosition;

		private ITrader trader;

		private List<ThingDef> cachedSellablePawns;

		private Dictionary<ThingCategoryDef, List<ThingDef>> cachedSellableItemsByCategory = new Dictionary<ThingCategoryDef, List<ThingDef>>();

		private const float RowHeight = 24f;

		private const float TitleRectHeight = 40f;

		private const float RestockTextHeight = 20f;

		private const float BottomAreaHeight = 55f;

		private readonly Vector2 BottomButtonSize = new Vector2(160f, 40f);

		public override Vector2 InitialSize => new Vector2(650f, Mathf.Min(UI.screenHeight, 1000));

		protected override float Margin => 0f;

		public Dialog_SellableItems(ITrader trader)
		{
			forcePause = true;
			absorbInputAroundWindow = true;
			this.trader = trader;
			CalculateSellableItems(trader.TraderKind);
			CalculateTabs();
		}

		public override void DoWindowContents(Rect inRect)
		{
			float num = 40f;
			Rect rect = new Rect(0f, 0f, inRect.width, 40f);
			Text.Font = GameFont.Medium;
			Text.Anchor = TextAnchor.MiddleCenter;
			Widgets.Label(rect, "SellableItemsTitle".Translate().CapitalizeFirst());
			Text.Font = GameFont.Small;
			Text.Anchor = TextAnchor.MiddleCenter;
			ITraderRestockingInfoProvider traderRestockingInfoProvider = trader as ITraderRestockingInfoProvider;
			if (traderRestockingInfoProvider != null)
			{
				int nextRestockTick = traderRestockingInfoProvider.NextRestockTick;
				if (nextRestockTick != -1)
				{
					Widgets.Label(label: "NextTraderRestock".Translate((nextRestockTick - Find.TickManager.TicksGame).TicksToDays().ToString("0.0")), rect: new Rect(0f, num, inRect.width, 20f));
					num += 20f;
				}
				else if (!traderRestockingInfoProvider.EverVisited)
				{
					Widgets.Label(new Rect(0f, num, inRect.width, 20f), "TraderNotVisitedYet".Translate());
					num += 20f;
				}
				else if (traderRestockingInfoProvider.RestockedSinceLastVisit)
				{
					Widgets.Label(new Rect(0f, num, inRect.width, 20f), "TraderRestockedSinceLastVisit".Translate());
					num += 20f;
				}
			}
			Text.Anchor = TextAnchor.UpperLeft;
			inRect.yMin += 64f + num;
			Widgets.DrawMenuSection(inRect);
			TabDrawer.DrawTabs(inRect, tabs, 2);
			inRect = inRect.ContractedBy(17f);
			GUI.BeginGroup(inRect);
			Rect rect2 = inRect.AtZero();
			DoBottomButtons(rect2);
			Rect outRect = rect2;
			outRect.yMax -= 65f;
			List<ThingDef> sellableItemsInCategory = GetSellableItemsInCategory(currentCategory, pawnsTabOpen);
			if (sellableItemsInCategory.Any())
			{
				float height = (float)sellableItemsInCategory.Count * 24f;
				num = 0f;
				Rect viewRect = new Rect(0f, 0f, outRect.width - 16f, height);
				Widgets.BeginScrollView(outRect, ref scrollPosition, viewRect);
				float num2 = scrollPosition.y - 24f;
				float num3 = scrollPosition.y + outRect.height;
				for (int i = 0; i < sellableItemsInCategory.Count; i++)
				{
					if (num > num2 && num < num3)
					{
						Widgets.DefLabelWithIcon(new Rect(0f, num, viewRect.width, 24f), sellableItemsInCategory[i]);
					}
					num += 24f;
				}
				Widgets.EndScrollView();
			}
			else
			{
				Widgets.NoneLabel(0f, outRect.width);
			}
			GUI.EndGroup();
		}

		private void DoBottomButtons(Rect rect)
		{
			if (Widgets.ButtonText(new Rect(rect.width / 2f - BottomButtonSize.x / 2f, rect.height - 55f, BottomButtonSize.x, BottomButtonSize.y), "CloseButton".Translate()))
			{
				Close();
			}
		}

		private void CalculateSellableItems(TraderKindDef trader)
		{
			sellableItems.Clear();
			cachedSellableItemsByCategory.Clear();
			cachedSellablePawns = null;
			List<ThingDef> allDefsListForReading = DefDatabase<ThingDef>.AllDefsListForReading;
			for (int i = 0; i < allDefsListForReading.Count; i++)
			{
				if (allDefsListForReading[i].PlayerAcquirable && !allDefsListForReading[i].IsCorpse && !typeof(MinifiedThing).IsAssignableFrom(allDefsListForReading[i].thingClass) && trader.WillTrade(allDefsListForReading[i]) && TradeUtility.EverPlayerSellable(allDefsListForReading[i]))
				{
					sellableItems.Add(allDefsListForReading[i]);
				}
			}
			sellableItems.SortBy((ThingDef x) => x.label);
		}

		private void CalculateTabs()
		{
			tabs.Clear();
			List<ThingCategoryDef> allDefsListForReading = DefDatabase<ThingCategoryDef>.AllDefsListForReading;
			for (int i = 0; i < allDefsListForReading.Count; i++)
			{
				ThingCategoryDef category = allDefsListForReading[i];
				if (category.parent == ThingCategoryDefOf.Root && AnyTraderWillEverTrade(category))
				{
					if (currentCategory == null)
					{
						currentCategory = category;
					}
					tabs.Add(new TabRecord(category.LabelCap, delegate
					{
						currentCategory = category;
						pawnsTabOpen = false;
					}, () => currentCategory == category));
				}
			}
			tabs.Add(new TabRecord("PawnsTabShort".Translate(), delegate
			{
				currentCategory = null;
				pawnsTabOpen = true;
			}, () => pawnsTabOpen));
		}

		private List<ThingDef> GetSellableItemsInCategory(ThingCategoryDef category, bool pawns)
		{
			if (pawns)
			{
				if (cachedSellablePawns == null)
				{
					cachedSellablePawns = new List<ThingDef>();
					for (int i = 0; i < sellableItems.Count; i++)
					{
						if (sellableItems[i].category == ThingCategory.Pawn)
						{
							cachedSellablePawns.Add(sellableItems[i]);
						}
					}
				}
				return cachedSellablePawns;
			}
			if (cachedSellableItemsByCategory.TryGetValue(category, out List<ThingDef> value))
			{
				return value;
			}
			value = new List<ThingDef>();
			for (int j = 0; j < sellableItems.Count; j++)
			{
				if (sellableItems[j].IsWithinCategory(category))
				{
					value.Add(sellableItems[j]);
				}
			}
			cachedSellableItemsByCategory.Add(category, value);
			return value;
		}

		private bool AnyTraderWillEverTrade(ThingCategoryDef thingCategory)
		{
			List<ThingDef> allDefsListForReading = DefDatabase<ThingDef>.AllDefsListForReading;
			for (int i = 0; i < allDefsListForReading.Count; i++)
			{
				if (!allDefsListForReading[i].IsWithinCategory(thingCategory))
				{
					continue;
				}
				List<TraderKindDef> allDefsListForReading2 = DefDatabase<TraderKindDef>.AllDefsListForReading;
				for (int j = 0; j < allDefsListForReading2.Count; j++)
				{
					if (allDefsListForReading2[j].WillTrade(allDefsListForReading[i]))
					{
						return true;
					}
				}
			}
			return false;
		}
	}
}
