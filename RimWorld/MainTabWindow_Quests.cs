using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimWorld
{
	[StaticConstructorOnStartup]
	public class MainTabWindow_Quests : MainTabWindow
	{
		private enum QuestsTab : byte
		{
			Available,
			Active,
			Historical
		}

		private Quest selected;

		private QuestsTab curTab;

		private List<TabRecord> tabs = new List<TabRecord>();

		private Vector2 scrollPosition_available;

		private Vector2 scrollPosition_active;

		private Vector2 scrollPosition_historical;

		private Vector2 selectedQuestScrollPosition;

		private float selectedQuestLastHeight;

		private bool showDebugInfo;

		private List<QuestPart> tmpQuestParts = new List<QuestPart>();

		private string debugSendSignalTextField;

		private const float LeftRectWidthFraction = 0.36f;

		private const float RowHeight = 32f;

		private const float CheckBoxHeight = 24f;

		private const float ShowDebugInfoToggleWidth = 110f;

		private const float DismisIconWidth = 32f;

		private const float TimeInfoWidth = 35f;

		private static readonly Color TimeLimitColor = new Color(1f, 1f, 1f, 0.7f);

		private static readonly Color AcceptanceRequirementsColor = new Color(1f, 0.25f, 0.25f);

		private static readonly Color AcceptanceRequirementsBoxColor = new Color(0.62f, 0.18f, 0.18f);

		private static readonly Color acceptanceRequirementsBoxBgColor = new Color(0.13f, 0.13f, 0.13f);

		private const int RowIconSize = 15;

		private const float RatingWidth = 60f;

		private const float RewardsConfigButtonHeight = 40f;

		private static Texture2D RatingIcon = null;

		private static readonly Texture2D DismissIcon = ContentFinder<Texture2D>.Get("UI/Buttons/Dismiss");

		private static readonly Texture2D ResumeQuestIcon = ContentFinder<Texture2D>.Get("UI/Buttons/UnDismiss");

		private static readonly Texture2D QuestDismissedIcon = ContentFinder<Texture2D>.Get("UI/Icons/DismissedQuestIcon");

		private static List<Quest> tmpQuestsToShow = new List<Quest>();

		private static List<GenUI.AnonymousStackElement> tmpStackElements = new List<GenUI.AnonymousStackElement>();

		private static List<Rect> layoutRewardsRects = new List<Rect>();

		private static List<QuestPart> tmpRemainingQuestParts = new List<QuestPart>();

		private static List<GlobalTargetInfo> tmpLookTargets = new List<GlobalTargetInfo>();

		private static List<GlobalTargetInfo> tmpSelectTargets = new List<GlobalTargetInfo>();

		public override Vector2 RequestedTabSize => new Vector2(1010f, 640f);

		public override void PreOpen()
		{
			base.PreOpen();
			if (RatingIcon == null)
			{
				RatingIcon = ContentFinder<Texture2D>.Get("UI/Icons/ChallengeRatingIcon");
			}
			tabs.Clear();
			tabs.Add(new TabRecord("AvailableQuests".Translate(), delegate
			{
				curTab = QuestsTab.Available;
				selected = null;
			}, () => curTab == QuestsTab.Available));
			tabs.Add(new TabRecord("ActiveQuests".Translate(), delegate
			{
				curTab = QuestsTab.Active;
				selected = null;
			}, () => curTab == QuestsTab.Active));
			tabs.Add(new TabRecord("HistoricalQuests".Translate(), delegate
			{
				curTab = QuestsTab.Historical;
				selected = null;
			}, () => curTab == QuestsTab.Historical));
			Select(selected);
		}

		public override void DoWindowContents(Rect rect)
		{
			base.DoWindowContents(rect);
			Rect rect2 = rect;
			rect2.yMin += 4f;
			rect2.xMax = rect2.width * 0.36f;
			rect2.yMax -= DoRewardsPrefsButton(rect2).height + 4f;
			DoQuestsList(rect2);
			Rect rect3 = rect;
			rect3.yMin += 4f;
			rect3.xMin = rect2.xMax + 17f;
			DoSelectedQuestInfo(rect3);
		}

		[Obsolete]
		public Rect DoFavorCheckBoxes(Rect rect)
		{
			return default(Rect);
		}

		public Rect DoRewardsPrefsButton(Rect rect)
		{
			rect.yMin = rect.yMax - 40f;
			Text.Font = GameFont.Small;
			if (Widgets.ButtonText(rect, "ChooseRewards".Translate()))
			{
				Find.WindowStack.Add(new Dialog_RewardPrefsConfig());
			}
			return rect;
		}

		public void Select(Quest quest)
		{
			if (quest != selected)
			{
				selected = quest;
				selectedQuestScrollPosition = default(Vector2);
				selectedQuestLastHeight = 300f;
			}
			if (quest != null)
			{
				if (quest.dismissed)
				{
					curTab = QuestsTab.Historical;
				}
				else if (quest.State == QuestState.NotYetAccepted)
				{
					curTab = QuestsTab.Available;
				}
				else if (quest.State == QuestState.Ongoing)
				{
					curTab = QuestsTab.Active;
				}
				else
				{
					curTab = QuestsTab.Historical;
				}
			}
		}

		private void DoQuestsList(Rect rect)
		{
			Rect rect2 = rect;
			rect2.yMin += 32f;
			Widgets.DrawMenuSection(rect2);
			TabDrawer.DrawTabs(rect2, tabs);
			SortQuestsByTab();
			if (tmpQuestsToShow.Count != 0)
			{
				Rect rect3 = rect2;
				rect3 = rect3.ContractedBy(10f);
				rect3.xMax += 6f;
				Rect viewRect = new Rect(0f, 0f, rect3.width - 16f, (float)tmpQuestsToShow.Count * 32f);
				Vector2 vector = default(Vector2);
				switch (curTab)
				{
				case QuestsTab.Available:
					Widgets.BeginScrollView(rect3, ref scrollPosition_available, viewRect);
					vector = scrollPosition_available;
					break;
				case QuestsTab.Active:
					Widgets.BeginScrollView(rect3, ref scrollPosition_active, viewRect);
					vector = scrollPosition_active;
					break;
				case QuestsTab.Historical:
					Widgets.BeginScrollView(rect3, ref scrollPosition_historical, viewRect);
					vector = scrollPosition_historical;
					break;
				}
				float num = 0f;
				for (int i = 0; i < tmpQuestsToShow.Count; i++)
				{
					Quest quest = tmpQuestsToShow[i];
					float num2 = vector.y - 32f;
					float num3 = vector.y + rect3.height;
					if (num > num2 && num < num3)
					{
						DoRow(new Rect(0f, num, viewRect.width - 4f, 32f), quest);
					}
					num += 32f;
				}
				tmpQuestsToShow.Clear();
				Widgets.EndScrollView();
			}
			else
			{
				Widgets.NoneLabel(rect2.y + 17f, rect2.width);
			}
		}

		private void SortQuestsByTab()
		{
			List<Quest> questsInDisplayOrder = Find.QuestManager.questsInDisplayOrder;
			tmpQuestsToShow.Clear();
			for (int i = 0; i < questsInDisplayOrder.Count; i++)
			{
				if (ShouldListNow(questsInDisplayOrder[i]))
				{
					tmpQuestsToShow.Add(questsInDisplayOrder[i]);
				}
			}
			switch (curTab)
			{
			case QuestsTab.Available:
				tmpQuestsToShow.SortBy((Quest q) => q.ticksUntilAcceptanceExpiry);
				break;
			case QuestsTab.Active:
				tmpQuestsToShow.SortBy((Quest q) => q.TicksSinceAccepted);
				break;
			case QuestsTab.Historical:
				tmpQuestsToShow.SortBy((Quest q) => q.TicksSinceCleanup);
				break;
			}
		}

		private void DoRow(Rect rect, Quest quest)
		{
			Rect rect2 = rect;
			rect2.width -= 95f;
			Rect rect3 = rect;
			rect3.xMax -= 4f;
			rect3.xMin = rect3.xMax - 35f;
			Rect rect4 = rect;
			rect4.xMax = rect3.xMin;
			rect4.xMin = rect4.xMax - 60f;
			if (selected == quest)
			{
				Widgets.DrawHighlightSelected(rect);
			}
			Text.Anchor = TextAnchor.MiddleLeft;
			Rect rect5 = new Rect(rect2.x + 4f, rect2.y, rect2.width - 4f, rect2.height);
			Widgets.Label(rect5, quest.name.Truncate(rect5.width));
			string timeTip;
			Color color;
			string shortTimeInfo = GetShortTimeInfo(quest, out timeTip, out color);
			if (!shortTimeInfo.NullOrEmpty())
			{
				GUI.color = color;
				Text.Anchor = TextAnchor.MiddleRight;
				Widgets.Label(rect3, shortTimeInfo);
				GUI.color = Color.white;
				if (Mouse.IsOver(rect3))
				{
					TooltipHandler.TipRegion(rect3, () => quest.name + (timeTip.NullOrEmpty() ? "" : ("\n" + timeTip)), quest.id ^ 0x343115E2);
					Widgets.DrawHighlight(rect3);
				}
			}
			if (quest.dismissed && !quest.Historical)
			{
				rect4.x -= 25f;
				Rect rect6 = new Rect(rect4.xMax + 5f, rect4.y + rect4.height / 2f - 7f, 15f, 15f);
				GUI.DrawTexture(rect6, QuestDismissedIcon);
				rect6.height = rect5.height;
				rect6.y = rect5.y;
				if (Mouse.IsOver(rect6))
				{
					TooltipHandler.TipRegion(rect6, "QuestDismissed".Translate());
					Widgets.DrawHighlight(rect6);
				}
			}
			for (int i = 0; i < quest.challengeRating; i++)
			{
				GUI.DrawTexture(new Rect(rect4.xMax - (float)(15 * (i + 1)), rect4.y + rect4.height / 2f - 7f, 15f, 15f), RatingIcon);
			}
			if (Mouse.IsOver(rect4))
			{
				TooltipHandler.TipRegion(rect4, "QuestChallengeRatingTip".Translate());
				Widgets.DrawHighlight(rect4);
			}
			GenUI.ResetLabelAlign();
			if (Widgets.ButtonInvisible(rect))
			{
				Select(quest);
				SoundDefOf.Click.PlayOneShotOnCamera();
			}
		}

		private string GetShortTimeInfo(Quest quest, out string tip, out Color color)
		{
			color = Color.gray;
			if (quest.State == QuestState.NotYetAccepted)
			{
				if (quest.ticksUntilAcceptanceExpiry >= 0)
				{
					color = ColoredText.RedReadable;
					tip = "QuestExpiresIn".Translate(quest.ticksUntilAcceptanceExpiry.ToStringTicksToPeriod());
					return quest.ticksUntilAcceptanceExpiry.ToStringTicksToPeriod(allowSeconds: true, shortForm: true);
				}
			}
			else
			{
				if (quest.Historical)
				{
					tip = "QuestFinishedAgo".Translate(quest.TicksSinceCleanup.ToStringTicksToPeriod());
					return quest.TicksSinceCleanup.ToStringTicksToPeriod(allowSeconds: false, shortForm: true);
				}
				if (quest.EverAccepted)
				{
					foreach (QuestPart item in quest.PartsListForReading)
					{
						QuestPart_Delay questPart_Delay = item as QuestPart_Delay;
						if (questPart_Delay != null && questPart_Delay.State == QuestPartState.Enabled && questPart_Delay.isBad && !questPart_Delay.expiryInfoPart.NullOrEmpty())
						{
							color = ColoredText.RedReadable;
							tip = "QuestExpiresIn".Translate(questPart_Delay.TicksLeft.ToStringTicksToPeriod());
							return questPart_Delay.TicksLeft.ToStringTicksToPeriod(allowSeconds: false, shortForm: true, canUseDecimals: false);
						}
					}
					tip = GetAcceptedAgoByString(quest);
					return quest.TicksSinceAccepted.ToStringTicksToPeriod(allowSeconds: false, shortForm: true);
				}
			}
			tip = null;
			return null;
		}

		private void DoSelectedQuestInfo(Rect rect)
		{
			Widgets.DrawMenuSection(rect);
			if (selected == null)
			{
				Widgets.NoneLabelCenteredVertically(rect, "(" + "NoQuestSelected".Translate() + ")");
				return;
			}
			Rect rect2 = rect.ContractedBy(17f);
			Rect outRect = rect2;
			Rect innerRect = new Rect(0f, 0f, outRect.width, selectedQuestLastHeight);
			Rect rect3 = new Rect(0f, 0f, outRect.width - 16f, selectedQuestLastHeight);
			Rect rect4 = rect3;
			bool flag = rect3.height > rect2.height;
			if (flag)
			{
				rect3.width -= 4f;
				rect4.width -= 16f;
			}
			Widgets.BeginScrollView(outRect, ref selectedQuestScrollPosition, rect3);
			float curY = 0f;
			DoTitle(rect3, ref curY);
			DoDismissButton(rect3, ref curY);
			if (selected != null)
			{
				float curYBeforeAcceptButton = curY;
				DoAcceptButton(rect3, ref curY);
				DoRightAlignedInfo(rect3, ref curY, curYBeforeAcceptButton);
				DoOutcomeInfo(rect3, ref curY);
				DoDescription(rect3, ref curY);
				DoAcceptanceRequirementInfo(innerRect, flag, ref curY);
				DoRewards(rect3, ref curY);
				DoLookTargets(rect3, ref curY);
				DoSelectTargets(rect3, ref curY);
				float num = curY;
				DoDefHyperlinks(rect3, ref curY);
				float num2 = curY;
				curY = num;
				DoFactionInfo(rect4, ref curY);
				DoDebugInfoToggle(rect3, ref curY);
				if (num2 > curY)
				{
					curY = num2;
				}
				DoDebugInfo(rect3, ref curY);
				selectedQuestLastHeight = curY;
			}
			Widgets.EndScrollView();
		}

		private void DoTitle(Rect innerRect, ref float curY)
		{
			Text.Font = GameFont.Medium;
			Rect rect = new Rect(innerRect.x, curY, innerRect.width, 100f);
			Widgets.Label(rect, selected.name.Truncate(rect.width));
			Text.Font = GameFont.Small;
			curY += Text.LineHeight;
			curY += 17f;
		}

		private void DoDismissButton(Rect innerRect, ref float curY)
		{
			Rect rect = new Rect(innerRect.xMax - 32f - 4f, innerRect.y, 32f, 32f);
			Texture2D tex = (!selected.Historical && selected.dismissed) ? ResumeQuestIcon : DismissIcon;
			if (Widgets.ButtonImage(rect, tex))
			{
				if (selected.Historical)
				{
					selected.hiddenInUI = true;
					SoundDefOf.Tick_High.PlayOneShotOnCamera();
					Select(null);
					return;
				}
				selected.dismissed = !selected.dismissed;
				if (selected.dismissed)
				{
					SoundDefOf.Tick_Low.PlayOneShotOnCamera();
					SortQuestsByTab();
					selected = tmpQuestsToShow.FirstOrDefault((Quest x) => ShouldListNow(x));
					tmpQuestsToShow.Clear();
					return;
				}
				SoundDefOf.Tick_High.PlayOneShotOnCamera();
				Select(selected);
			}
			if (Mouse.IsOver(rect))
			{
				string key = selected.Historical ? "DeleteQuest" : (selected.dismissed ? "UnDismissQuest" : "DismissQuest");
				TooltipHandler.TipRegion(rect, key.Translate());
			}
		}

		private void DoDebugInfoToggle(Rect innerRect, ref float curY)
		{
			if (!Prefs.DevMode)
			{
				showDebugInfo = false;
				return;
			}
			Widgets.CheckboxLabeled(new Rect(innerRect.xMax - 110f, curY, 110f, 30f), "Dev: Debug", ref showDebugInfo);
			curY += 30f;
		}

		private void DoAcceptButton(Rect innerRect, ref float curY)
		{
			QuestPart_Choice questPart_Choice = null;
			List<QuestPart> partsListForReading = selected.PartsListForReading;
			for (int i = 0; i < partsListForReading.Count; i++)
			{
				questPart_Choice = (partsListForReading[i] as QuestPart_Choice);
				if (questPart_Choice != null)
				{
					break;
				}
			}
			if (questPart_Choice != null && !Prefs.DevMode)
			{
				return;
			}
			curY += 17f;
			if (selected.State != 0)
			{
				return;
			}
			float num = innerRect.x;
			if (questPart_Choice == null)
			{
				Rect rect = new Rect(num, curY, 180f, 40f);
				if (!QuestUtility.CanAcceptQuest(selected))
				{
					GUI.color = Color.grey;
				}
				if (Widgets.ButtonText(rect, "AcceptQuest".Translate()))
				{
					AcceptQuestByInterface(null, selected.RequiresAccepter);
				}
				num += rect.width + 10f;
				GUI.color = Color.white;
			}
			if (Prefs.DevMode && Widgets.ButtonText(new Rect(num, curY, 180f, 40f), "Dev: Accept instantly"))
			{
				SoundDefOf.Quest_Accepted.PlayOneShotOnCamera();
				if (questPart_Choice.choices.Any())
				{
					questPart_Choice.Choose(questPart_Choice.choices.RandomElement());
				}
				selected.Accept(PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive_FreeColonists_NoCryptosleep.Where((Pawn p) => QuestUtility.CanPawnAcceptQuest(p, selected)).RandomElementWithFallback());
				selected.dismissed = false;
				Select(selected);
			}
			curY += 44f;
		}

		private void DoRightAlignedInfo(Rect innerRect, ref float curY, float curYBeforeAcceptButton)
		{
			bool flag = false;
			Vector2 locForDates = QuestUtility.GetLocForDates();
			float num = curYBeforeAcceptButton;
			if (!selected.initiallyAccepted && selected.EverAccepted)
			{
				if (!flag)
				{
					num += 17f;
					flag = true;
				}
				Rect rect = new Rect(innerRect.x, num, innerRect.width, 25f);
				GUI.color = TimeLimitColor;
				Text.Anchor = TextAnchor.MiddleRight;
				string text = selected.Historical ? GetAcceptedOnByString(selected) : GetAcceptedAgoByString(selected);
				Widgets.Label(rect, text);
				GUI.color = Color.white;
				Text.Anchor = TextAnchor.UpperLeft;
				rect.xMin = rect.xMax - Text.CalcSize(text).x;
				if (Mouse.IsOver(rect))
				{
					TooltipHandler.TipRegion(rect, selected.Historical ? GetAcceptedAgoByString(selected) : GetAcceptedOnByString(selected));
				}
				if (selected.AccepterPawn != null && CameraJumper.CanJump(selected.AccepterPawn))
				{
					Widgets.DrawHighlightIfMouseover(rect);
					if (Widgets.ButtonInvisible(rect))
					{
						CameraJumper.TryJumpAndSelect(selected.AccepterPawn);
						Find.MainTabsRoot.EscapeCurrentTab();
					}
				}
				num += Text.LineHeight;
			}
			else if (selected.Historical)
			{
				if (!flag)
				{
					num += 17f;
					flag = true;
				}
				Rect rect2 = new Rect(innerRect.x, num, innerRect.width, 25f);
				GUI.color = TimeLimitColor;
				Text.Anchor = TextAnchor.MiddleRight;
				TaggedString taggedString = "AppearedOn".Translate(GenDate.DateFullStringWithHourAt(GenDate.TickGameToAbs(selected.appearanceTick), locForDates));
				Widgets.Label(rect2, taggedString);
				GUI.color = Color.white;
				Text.Anchor = TextAnchor.UpperLeft;
				rect2.xMin = rect2.xMax - Text.CalcSize(taggedString).x;
				if (Mouse.IsOver(rect2))
				{
					TooltipHandler.TipRegion(rect2, "AppearedDaysAgo".Translate(((float)selected.TicksSinceAppeared / 60000f).ToString("0.#")));
				}
				num += Text.LineHeight;
			}
			if (selected.State == QuestState.NotYetAccepted && selected.ticksUntilAcceptanceExpiry > 0)
			{
				if (!flag)
				{
					num += 17f;
					flag = true;
				}
				Rect rect3 = new Rect(innerRect.x, num, innerRect.width, 25f);
				GUI.color = TimeLimitColor;
				Text.Anchor = TextAnchor.MiddleRight;
				string text2 = "QuestExpiresIn".Translate(selected.ticksUntilAcceptanceExpiry.ToStringTicksToPeriod());
				Widgets.Label(rect3, text2);
				GUI.color = Color.white;
				Text.Anchor = TextAnchor.UpperLeft;
				rect3.xMin = rect3.xMax - Text.CalcSize(text2).x;
				if (Mouse.IsOver(rect3))
				{
					TooltipHandler.TipRegion(rect3, "QuestExpiresOn".Translate(GenDate.DateFullStringWithHourAt(Find.TickManager.TicksAbs + selected.ticksUntilAcceptanceExpiry, locForDates)));
				}
				num += Text.LineHeight;
			}
			if (selected.State == QuestState.Ongoing)
			{
				tmpQuestParts.Clear();
				tmpQuestParts.AddRange(selected.PartsListForReading);
				tmpQuestParts.SortBy((QuestPart x) => (x is QuestPartActivable) ? ((QuestPartActivable)x).EnableTick : 0);
				for (int i = 0; i < tmpQuestParts.Count; i++)
				{
					QuestPartActivable questPartActivable = tmpQuestParts[i] as QuestPartActivable;
					if (questPartActivable == null || questPartActivable.State != QuestPartState.Enabled)
					{
						continue;
					}
					string expiryInfoPart = questPartActivable.ExpiryInfoPart;
					if (expiryInfoPart.NullOrEmpty())
					{
						continue;
					}
					if (!flag)
					{
						num += 17f;
						flag = true;
					}
					Rect rect4 = new Rect(innerRect.x, num, innerRect.width, 25f);
					GUI.color = TimeLimitColor;
					Text.Anchor = TextAnchor.MiddleRight;
					Widgets.Label(rect4, expiryInfoPart);
					GUI.color = Color.white;
					Text.Anchor = TextAnchor.UpperLeft;
					rect4.xMin = rect4.xMax - Text.CalcSize(expiryInfoPart).x;
					if (Mouse.IsOver(rect4))
					{
						string expiryInfoPartTip = questPartActivable.ExpiryInfoPartTip;
						if (!expiryInfoPartTip.NullOrEmpty())
						{
							TooltipHandler.TipRegion(rect4, expiryInfoPartTip);
						}
					}
					num += Text.LineHeight;
				}
				tmpQuestParts.Clear();
			}
			curY = Mathf.Max(curY, num);
		}

		private void DoAcceptanceRequirementInfo(Rect innerRect, bool scrollBarVisible, ref float curY)
		{
			if (selected.EverAccepted)
			{
				return;
			}
			IEnumerable<string> enumerable = ListUnmetAcceptRequirements();
			int num = enumerable.Count();
			if (num != 0)
			{
				bool flag = num > 1;
				string text = "QuestAcceptanceRequirementsDescription".Translate() + (flag ? ": " : " ") + (flag ? ("\n" + enumerable.ToLineList("  - ", capitalizeItems: true)) : enumerable.First());
				curY += 17f;
				float num2 = 0f;
				float x = innerRect.x + 8f;
				float num3 = innerRect.width - 16f;
				if (scrollBarVisible)
				{
					num3 -= 31f;
				}
				Rect rect = new Rect(x, curY, num3, 10000f);
				num2 += Text.CalcHeight(text, rect.width);
				Rect rect2 = new Rect(x, curY, num3, num2).ExpandedBy(8f);
				Widgets.DrawBoxSolid(rect2, acceptanceRequirementsBoxBgColor);
				GUI.color = AcceptanceRequirementsColor;
				Widgets.Label(rect, text);
				GUI.color = AcceptanceRequirementsBoxColor;
				Widgets.DrawBox(rect2, 2);
				curY += num2;
				GUI.color = Color.white;
			}
		}

		private IEnumerable<string> ListUnmetAcceptRequirements()
		{
			for (int i = 0; i < selected.PartsListForReading.Count; i++)
			{
				QuestPart_RequirementsToAccept questPart_RequirementsToAccept = selected.PartsListForReading[i] as QuestPart_RequirementsToAccept;
				if (questPart_RequirementsToAccept != null)
				{
					AcceptanceReport acceptanceReport = questPart_RequirementsToAccept.CanAccept();
					if (!acceptanceReport.Accepted)
					{
						yield return acceptanceReport.Reason;
					}
				}
			}
		}

		private void DoOutcomeInfo(Rect innerRect, ref float curY)
		{
			if (selected.Historical)
			{
				string text = (selected.State == QuestState.EndedOfferExpired) ? ((string)"QuestOutcomeInfo_OfferExpired".Translate()) : ((selected.State == QuestState.EndedUnknownOutcome || selected.State == QuestState.EndedSuccess) ? ((string)"QuestOutcomeInfo_UnknownOrSuccess".Translate()) : ((selected.State == QuestState.EndedFailed) ? ((string)"QuestOutcomeInfo_Failed".Translate()) : ((selected.State != QuestState.EndedInvalid) ? null : ((string)"QuestOutcomeInfo_Invalid".Translate()))));
				if (!text.NullOrEmpty())
				{
					curY += 17f;
					Widgets.Label(new Rect(innerRect.x, curY, innerRect.width, 25f), text);
					curY += Text.LineHeight;
				}
			}
		}

		private void DoDescription(Rect innerRect, ref float curY)
		{
			StringBuilder stringBuilder = new StringBuilder();
			if (!selected.description.RawText.NullOrEmpty())
			{
				string value = selected.description.Resolve();
				stringBuilder.Append(value);
			}
			tmpQuestParts.Clear();
			tmpQuestParts.AddRange(selected.PartsListForReading);
			tmpQuestParts.SortBy((QuestPart x) => (x is QuestPartActivable) ? ((QuestPartActivable)x).EnableTick : 0);
			for (int i = 0; i < tmpQuestParts.Count; i++)
			{
				QuestPartActivable questPartActivable = tmpQuestParts[i] as QuestPartActivable;
				if (questPartActivable != null && questPartActivable.State != QuestPartState.Enabled)
				{
					continue;
				}
				string descriptionPart = tmpQuestParts[i].DescriptionPart;
				if (!descriptionPart.NullOrEmpty())
				{
					if (stringBuilder.Length != 0)
					{
						stringBuilder.AppendLine();
						stringBuilder.AppendLine();
					}
					stringBuilder.Append(descriptionPart);
				}
			}
			tmpQuestParts.Clear();
			if (stringBuilder.Length != 0)
			{
				curY += 17f;
				Rect rect = new Rect(innerRect.x, curY, innerRect.width, 10000f);
				Widgets.Label(rect, stringBuilder.ToString());
				curY += Text.CalcHeight(stringBuilder.ToString(), rect.width);
			}
		}

		private void DoRewards(Rect innerRect, ref float curY)
		{
			QuestPart_Choice choice = null;
			List<QuestPart> partsListForReading = selected.PartsListForReading;
			for (int i = 0; i < partsListForReading.Count; i++)
			{
				choice = (partsListForReading[i] as QuestPart_Choice);
				if (choice != null)
				{
					break;
				}
			}
			if (choice == null)
			{
				return;
			}
			bool flag = selected.State == QuestState.NotYetAccepted;
			bool flag2 = true;
			if (Event.current.type == EventType.Layout)
			{
				layoutRewardsRects.Clear();
			}
			for (int j = 0; j < choice.choices.Count; j++)
			{
				tmpStackElements.Clear();
				float num = 0f;
				for (int k = 0; k < choice.choices[j].rewards.Count; k++)
				{
					tmpStackElements.AddRange(choice.choices[j].rewards[k].StackElements);
					num += choice.choices[j].rewards[k].TotalMarketValue;
				}
				if (!tmpStackElements.Any())
				{
					continue;
				}
				if (num > 0f)
				{
					TaggedString totalValueStr = "TotalValue".Translate(num.ToStringMoney("F0"));
					tmpStackElements.Add(new GenUI.AnonymousStackElement
					{
						drawer = delegate(Rect r)
						{
							GUI.color = new Color(0.7f, 0.7f, 0.7f);
							Widgets.Label(new Rect(r.x + 5f, r.y, r.width - 10f, r.height), totalValueStr);
							GUI.color = Color.white;
						},
						width = Text.CalcSize(totalValueStr).x + 10f
					});
				}
				if (flag2)
				{
					curY += 17f;
					flag2 = false;
				}
				else
				{
					curY += 10f;
				}
				Rect rect = new Rect(innerRect.x, curY, innerRect.width, 10000f);
				Rect rect2 = rect.ContractedBy(10f);
				if (flag)
				{
					rect2.xMin += 100f;
				}
				if (j < layoutRewardsRects.Count)
				{
					Widgets.DrawBoxSolid(layoutRewardsRects[j], new Color(0.13f, 0.13f, 0.13f));
					GUI.color = new Color(1f, 1f, 1f, 0.3f);
					Widgets.DrawHighlightIfMouseover(layoutRewardsRects[j]);
					GUI.color = Color.white;
				}
				rect.height = GenUI.DrawElementStack(rect2, 24f, tmpStackElements, delegate(Rect r, GenUI.AnonymousStackElement obj)
				{
					obj.drawer(r);
				}, (GenUI.AnonymousStackElement obj) => obj.width, 4f, 5f, allowOrderOptimization: false).height + 20f;
				if (Event.current.type == EventType.Layout)
				{
					layoutRewardsRects.Add(rect);
				}
				if (flag)
				{
					if (!QuestUtility.CanAcceptQuest(selected))
					{
						GUI.color = Color.grey;
					}
					Rect rect3 = new Rect(rect.x, rect.y, 100f, rect.height);
					if (Widgets.ButtonText(rect3, "AcceptQuestFor".Translate() + ":"))
					{
						tmpRemainingQuestParts.Clear();
						tmpRemainingQuestParts.AddRange(selected.PartsListForReading);
						for (int l = 0; l < choice.choices.Count; l++)
						{
							if (j == l)
							{
								continue;
							}
							for (int m = 0; m < choice.choices[l].questParts.Count; m++)
							{
								QuestPart item = choice.choices[l].questParts[m];
								if (!choice.choices[j].questParts.Contains(item))
								{
									tmpRemainingQuestParts.Remove(item);
								}
							}
						}
						bool requiresAccepter = false;
						for (int n = 0; n < tmpRemainingQuestParts.Count; n++)
						{
							if (tmpRemainingQuestParts[n].RequiresAccepter)
							{
								requiresAccepter = true;
								break;
							}
						}
						tmpRemainingQuestParts.Clear();
						QuestPart_Choice.Choice localChoice = choice.choices[j];
						AcceptQuestByInterface(delegate
						{
							choice.Choose(localChoice);
						}, requiresAccepter);
					}
					TooltipHandler.TipRegionByKey(rect3, "AcceptQuestForTip");
					GUI.color = Color.white;
				}
				curY += rect.height;
			}
			if (Event.current.type == EventType.Repaint)
			{
				layoutRewardsRects.Clear();
			}
			tmpStackElements.Clear();
		}

		private void DoLookTargets(Rect innerRect, ref float curY)
		{
			List<Map> maps = Find.Maps;
			int num = 0;
			for (int i = 0; i < maps.Count; i++)
			{
				if (maps[i].IsPlayerHome)
				{
					num++;
				}
			}
			tmpLookTargets.Clear();
			tmpLookTargets.AddRange(selected.QuestLookTargets);
			tmpLookTargets.SortBy(delegate(GlobalTargetInfo x)
			{
				if (x.Thing is Pawn)
				{
					return 0;
				}
				if (x.HasThing)
				{
					return 1;
				}
				if (!x.IsWorldTarget)
				{
					return 2;
				}
				return (!(x.WorldObject is Settlement) || ((Settlement)x.WorldObject).Faction != Faction.OfPlayer) ? 3 : 4;
			}, (GlobalTargetInfo x) => x.Label);
			bool flag = false;
			for (int j = 0; j < tmpLookTargets.Count; j++)
			{
				GlobalTargetInfo globalTargetInfo = tmpLookTargets[j];
				if (globalTargetInfo.HasWorldObject)
				{
					MapParent mapParent = globalTargetInfo.WorldObject as MapParent;
					if (mapParent != null && (!mapParent.HasMap || !mapParent.Map.IsPlayerHome))
					{
						flag = true;
						break;
					}
				}
			}
			bool flag2 = false;
			for (int k = 0; k < tmpLookTargets.Count; k++)
			{
				GlobalTargetInfo globalTargetInfo2 = tmpLookTargets[k];
				if (CameraJumper.CanJump(globalTargetInfo2) && (num != 1 || !(globalTargetInfo2 == Find.AnyPlayerHomeMap.Parent) || flag))
				{
					if (!flag2)
					{
						flag2 = true;
						curY += 17f;
					}
					if (Widgets.ButtonText(new Rect(innerRect.x, curY, innerRect.width, 25f), "JumpToTargetCustom".Translate(globalTargetInfo2.Label), drawBackground: false))
					{
						CameraJumper.TryJumpAndSelect(globalTargetInfo2);
						Find.MainTabsRoot.EscapeCurrentTab();
					}
					curY += 25f;
				}
			}
		}

		private void DoSelectTargets(Rect innerRect, ref float curY)
		{
			bool flag = false;
			for (int i = 0; i < selected.PartsListForReading.Count; i++)
			{
				QuestPart questPart = selected.PartsListForReading[i];
				tmpSelectTargets.Clear();
				tmpSelectTargets.AddRange(questPart.QuestSelectTargets);
				if (tmpSelectTargets.Count == 0)
				{
					continue;
				}
				if (!flag)
				{
					flag = true;
					curY += 4f;
				}
				if (Widgets.ButtonText(new Rect(innerRect.x, curY, innerRect.width, 25f), questPart.QuestSelectTargetsLabel, drawBackground: false))
				{
					Map map = null;
					int num = 0;
					Vector3 zero = Vector3.zero;
					Find.Selector.ClearSelection();
					for (int j = 0; j < tmpSelectTargets.Count; j++)
					{
						GlobalTargetInfo target = tmpSelectTargets[j];
						if (CameraJumper.CanJump(target) && target.HasThing)
						{
							Find.Selector.Select(target.Thing);
							if (map == null)
							{
								map = target.Map;
							}
							else if (target.Map != map)
							{
								num = 0;
								break;
							}
							zero += target.Cell.ToVector3();
							num++;
						}
					}
					if (num > 0)
					{
						CameraJumper.TryJump(new IntVec3(zero / num), map);
					}
					Find.MainTabsRoot.EscapeCurrentTab();
				}
				curY += 25f;
			}
		}

		private void DoFactionInfo(Rect rect, ref float curY)
		{
			curY += 15f;
			foreach (Faction involvedFaction in selected.InvolvedFactions)
			{
				if (involvedFaction != null && !involvedFaction.def.hidden && !involvedFaction.IsPlayer)
				{
					FactionUIUtility.DrawRelatedFactionInfo(rect, involvedFaction, ref curY);
				}
			}
		}

		private void DoDefHyperlinks(Rect rect, ref float curY)
		{
			curY += 25f;
			foreach (Dialog_InfoCard.Hyperlink hyperlink in selected.Hyperlinks)
			{
				float num = Text.CalcHeight(hyperlink.Label, rect.width);
				Widgets.HyperlinkWithIcon(new Rect(rect.x, curY, rect.width / 2f, num), hyperlink, "ViewHyperlink".Translate(hyperlink.Label));
				curY += num;
			}
		}

		private void DoDebugInfo(Rect innerRect, ref float curY)
		{
			if (!showDebugInfo)
			{
				return;
			}
			curY += 17f;
			List<QuestPart> partsListForReading = selected.PartsListForReading;
			if (selected.State == QuestState.Ongoing)
			{
				for (int i = 0; i < partsListForReading.Count; i++)
				{
					partsListForReading[i].DoDebugWindowContents(innerRect, ref curY);
				}
			}
			if (selected.State == QuestState.Ongoing)
			{
				Rect rect = new Rect(innerRect.x, curY, 210f, 25f);
				debugSendSignalTextField = Widgets.TextField(rect, debugSendSignalTextField);
				Rect rect2 = new Rect(innerRect.x + rect.width + 4f, curY, 117f, 25f);
				if (Widgets.ButtonText(rect2, "Send signal"))
				{
					Find.SignalManager.SendSignal(new Signal(debugSendSignalTextField));
					debugSendSignalTextField = "";
				}
				if (Widgets.ButtonText(new Rect(rect2.xMax + 4f, curY, 165f, 25f), "Send defined signal..."))
				{
					List<FloatMenuOption> list = new List<FloatMenuOption>();
					foreach (string item in from x in DebugPossibleSignals(selected).Distinct()
						orderby x
						select x)
					{
						string signalLocal = item;
						list.Add(new FloatMenuOption(signalLocal, delegate
						{
							Find.SignalManager.SendSignal(new Signal(signalLocal));
							debugSendSignalTextField = "";
						}));
					}
					Find.WindowStack.Add(new FloatMenu(list));
				}
				curY += rect.height + 4f;
			}
			string arg = "-----------------";
			arg = arg + "\nId: " + selected.id;
			arg = arg + "\nState: " + selected.State;
			arg += "\nData:";
			arg = arg + "\n" + Scribe.saver.DebugOutputFor(selected);
			arg += "\n";
			arg += "\nActive QuestParts:";
			bool flag = false;
			for (int j = 0; j < partsListForReading.Count; j++)
			{
				QuestPartActivable questPartActivable = partsListForReading[j] as QuestPartActivable;
				if (questPartActivable != null && questPartActivable.State == QuestPartState.Enabled)
				{
					arg = arg + "\n" + questPartActivable.ToString();
					flag = true;
				}
			}
			if (!flag)
			{
				arg += "\nNone";
			}
			Rect rect3 = new Rect(innerRect.x, curY, 180f, 40f);
			if (Widgets.ButtonText(rect3, "Copy debug to clipboard"))
			{
				GUIUtility.systemCopyBuffer = arg;
			}
			curY += rect3.height + 4f;
			Widgets.LongLabel(innerRect.x, innerRect.width, arg, ref curY);
		}

		private bool ShouldListNow(Quest quest)
		{
			switch (curTab)
			{
			case QuestsTab.Available:
				if (quest.State == QuestState.NotYetAccepted && !quest.dismissed)
				{
					return !quest.hiddenInUI;
				}
				return false;
			case QuestsTab.Active:
				if (quest.State == QuestState.Ongoing && !quest.dismissed)
				{
					return !quest.hiddenInUI;
				}
				return false;
			case QuestsTab.Historical:
				if (!quest.hiddenInUI)
				{
					if (!quest.Historical)
					{
						return quest.dismissed;
					}
					return true;
				}
				return false;
			default:
				return false;
			}
		}

		private IEnumerable<string> DebugPossibleSignals(Quest quest)
		{
			string input = Scribe.saver.DebugOutputFor(selected);
			foreach (Match item in Regex.Matches(input, ">(Quest" + quest.id + "\\.[a-zA-Z0-9/\\-\\.]*)<"))
			{
				yield return item.Groups[1].Value;
			}
		}

		private string GetAcceptedAgoByString(Quest quest)
		{
			string value = quest.TicksSinceAccepted.ToStringTicksToPeriod();
			if (!quest.AccepterPawnLabelCap.NullOrEmpty())
			{
				return "AcceptedAgoBy".Translate(value, quest.AccepterPawnLabelCap);
			}
			return "AcceptedAgo".Translate(value);
		}

		private string GetAcceptedOnByString(Quest quest)
		{
			Vector2 locForDates = QuestUtility.GetLocForDates();
			string value = GenDate.DateFullStringWithHourAt(GenDate.TickGameToAbs(quest.acceptanceTick), locForDates);
			if (!quest.AccepterPawnLabelCap.NullOrEmpty())
			{
				return "AcceptedOnBy".Translate(value, quest.AccepterPawnLabelCap);
			}
			return "AcceptedOn".Translate(value);
		}

		private void AcceptQuestByInterface(Action preAcceptAction = null, bool requiresAccepter = false)
		{
			if (QuestUtility.CanAcceptQuest(selected))
			{
				if (requiresAccepter)
				{
					List<FloatMenuOption> list = new List<FloatMenuOption>();
					foreach (Pawn p in PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive_FreeColonists_NoCryptosleep)
					{
						if (!QuestUtility.CanPawnAcceptQuest(p, selected))
						{
							continue;
						}
						Pawn pLocal = p;
						string text = "AcceptWith".Translate(p);
						if (p.royalty != null && p.royalty.AllTitlesInEffectForReading.Any())
						{
							text = text + " (" + p.royalty.MostSeniorTitle.def.GetLabelFor(pLocal) + ")";
						}
						list.Add(new FloatMenuOption(text, delegate
						{
							if (QuestUtility.CanPawnAcceptQuest(pLocal, selected))
							{
								QuestPart_GiveRoyalFavor questPart_GiveRoyalFavor = selected.PartsListForReading.OfType<QuestPart_GiveRoyalFavor>().FirstOrDefault();
								if (questPart_GiveRoyalFavor != null && questPart_GiveRoyalFavor.giveToAccepter)
								{
									IEnumerable<Trait> conceitedTraits = RoyalTitleUtility.GetConceitedTraits(p);
									IEnumerable<Trait> traitsAffectingPsylinkNegatively = RoyalTitleUtility.GetTraitsAffectingPsylinkNegatively(p);
									bool totallyDisabled = p.skills.GetSkill(SkillDefOf.Social).TotallyDisabled;
									bool flag = conceitedTraits.Any();
									bool flag2 = !p.HasPsylink && traitsAffectingPsylinkNegatively.Any();
									if (totallyDisabled || flag || flag2)
									{
										NamedArgument arg = p.Named("PAWN");
										NamedArgument arg2 = questPart_GiveRoyalFavor.faction.Named("FACTION");
										TaggedString t2 = null;
										if (totallyDisabled)
										{
											t2 = "RoyalIncapableOfSocial".Translate(arg, arg2);
										}
										TaggedString t3 = null;
										if (flag)
										{
											t3 = "RoyalWithConceitedTrait".Translate(arg, arg2, conceitedTraits.Select((Trait t) => t.CurrentData.label).ToCommaList(useAnd: true));
										}
										TaggedString t4 = null;
										if (flag2)
										{
											t4 = "RoyalWithTraitAffectingPsylinkNegatively".Translate(arg, arg2, traitsAffectingPsylinkNegatively.Select((Trait t) => t.Label).ToCommaList(useAnd: true));
										}
										TaggedString text2 = "QuestGivesRoyalFavor".Translate(arg, arg2);
										if (totallyDisabled)
										{
											text2 += "\n\n" + t2;
										}
										if (flag)
										{
											text2 += "\n\n" + t3;
										}
										if (flag2)
										{
											text2 += "\n\n" + t4;
										}
										text2 += "\n\n" + "WantToContinue".Translate();
										Find.WindowStack.Add(new Dialog_MessageBox(text2, "Confirm".Translate(), AcceptAction, "GoBack".Translate()));
									}
									else
									{
										AcceptAction();
									}
								}
								else
								{
									AcceptAction();
								}
							}
						}));
						void AcceptAction()
						{
							SoundDefOf.Quest_Accepted.PlayOneShotOnCamera();
							if (preAcceptAction != null)
							{
								preAcceptAction();
							}
							selected.Accept(pLocal);
							Select(selected);
							Messages.Message("MessageQuestAccepted".Translate(pLocal, selected.name), pLocal, MessageTypeDefOf.TaskCompletion, historical: false);
						}
					}
					if (list.Count > 0)
					{
						Find.WindowStack.Add(new FloatMenu(list));
					}
					else
					{
						Messages.Message("MessageNoColonistCanAcceptQuest".Translate(Faction.OfPlayer.def.pawnsPlural), MessageTypeDefOf.RejectInput, historical: false);
					}
				}
				else
				{
					SoundDefOf.Quest_Accepted.PlayOneShotOnCamera();
					if (preAcceptAction != null)
					{
						preAcceptAction();
					}
					selected.Accept(null);
					Select(selected);
				}
			}
			else
			{
				Messages.Message("MessageCannotAcceptQuest".Translate(), MessageTypeDefOf.RejectInput, historical: false);
			}
		}
	}
}
