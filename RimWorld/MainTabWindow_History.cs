using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimWorld;

[StaticConstructorOnStartup]
public class MainTabWindow_History : MainTabWindow
{
	private enum HistoryTab : byte
	{
		Graph,
		Messages,
		Statistics
	}

	private HistoryAutoRecorderGroup historyAutoRecorderGroup;

	private FloatRange graphSection;

	private Vector2 messagesScrollPos;

	private float messagesLastHeight;

	private List<TabRecord> tabs = new List<TabRecord>();

	private int displayedMessageIndex;

	private static QuickSearchWidget quickSearchWidget = new QuickSearchWidget();

	private static HistoryTab curTab = HistoryTab.Graph;

	private static bool showLetters = true;

	private static bool showMessages;

	private const float MessagesRowHeight = 30f;

	private const float PinColumnSize = 30f;

	private const float PinSize = 22f;

	private const float IconColumnSize = 30f;

	private const float DateSize = 90f;

	private const float SpaceBetweenColumns = 5f;

	private static readonly Vector2 SearchBarOffset = new Vector2(720f, 8f);

	private static readonly Texture2D PinTex = ContentFinder<Texture2D>.Get("UI/Icons/Pin");

	private static readonly Texture2D PinOutlineTex = ContentFinder<Texture2D>.Get("UI/Icons/Pin-Outline");

	private static readonly Color PinOutlineColor = new Color(0.25f, 0.25f, 0.25f, 0.5f);

	private Dictionary<string, string> truncationCache = new Dictionary<string, string>();

	private static List<CurveMark> marks = new List<CurveMark>();

	public override Vector2 RequestedTabSize => new Vector2(1010f, 640f);

	public override void PreOpen()
	{
		base.PreOpen();
		tabs.Clear();
		tabs.Add(new TabRecord("Graph".Translate(), delegate
		{
			curTab = HistoryTab.Graph;
		}, () => curTab == HistoryTab.Graph));
		tabs.Add(new TabRecord("Messages".Translate(), delegate
		{
			curTab = HistoryTab.Messages;
		}, () => curTab == HistoryTab.Messages));
		tabs.Add(new TabRecord("Statistics".Translate(), delegate
		{
			curTab = HistoryTab.Statistics;
		}, () => curTab == HistoryTab.Statistics));
		historyAutoRecorderGroup = Find.History.Groups().FirstOrDefault();
		if (historyAutoRecorderGroup != null)
		{
			graphSection = new FloatRange(0f, (float)Find.TickManager.TicksGame / 60000f);
		}
		List<Map> maps = Find.Maps;
		for (int num = 0; num < maps.Count; num++)
		{
			maps[num].wealthWatcher.ForceRecount();
		}
		quickSearchWidget.Reset();
	}

	public override void DoWindowContents(Rect rect)
	{
		Rect rect2 = rect;
		rect2.yMin += 45f;
		TabDrawer.DrawTabs(rect2, tabs);
		switch (curTab)
		{
		case HistoryTab.Graph:
			DoGraphPage(rect2);
			break;
		case HistoryTab.Messages:
			DoMessagesPage(rect2);
			break;
		case HistoryTab.Statistics:
			DoStatisticsPage(rect2);
			break;
		}
	}

	private void DoStatisticsPage(Rect rect)
	{
		rect.yMin += 17f;
		Widgets.BeginGroup(rect);
		StringBuilder stringBuilder = new StringBuilder();
		TimeSpan timeSpan = new TimeSpan(0, 0, (int)Find.GameInfo.RealPlayTimeInteracting);
		stringBuilder.AppendLine(string.Concat(string.Concat(string.Concat(string.Concat("Playtime".Translate() + ": ", timeSpan.Days.ToString()) + "LetterDay".Translate() + " ", timeSpan.Hours.ToString()) + "LetterHour".Translate() + " ", timeSpan.Minutes.ToString()) + "LetterMinute".Translate() + " ", timeSpan.Seconds.ToString()) + "LetterSecond".Translate());
		stringBuilder.AppendLine("Storyteller".Translate() + ": " + Find.Storyteller.def.LabelCap);
		stringBuilder.AppendLine("Difficulty".Translate() + ": " + Find.Storyteller.difficultyDef.LabelCap);
		if (Find.CurrentMap != null)
		{
			stringBuilder.AppendLine();
			stringBuilder.AppendLine("ThisMapColonyWealthTotal".Translate() + ": " + Find.CurrentMap.wealthWatcher.WealthTotal.ToString("F0"));
			stringBuilder.AppendLine("ThisMapColonyWealthItems".Translate() + ": " + Find.CurrentMap.wealthWatcher.WealthItems.ToString("F0"));
			stringBuilder.AppendLine("ThisMapColonyWealthBuildings".Translate() + ": " + Find.CurrentMap.wealthWatcher.WealthBuildings.ToString("F0"));
			stringBuilder.AppendLine("ThisMapColonyWealthColonistsAndTameAnimals".Translate() + ": " + Find.CurrentMap.wealthWatcher.WealthPawns.ToString("F0"));
		}
		stringBuilder.AppendLine();
		stringBuilder.AppendLine(string.Concat("NumThreatBigs".Translate() + ": ", Find.StoryWatcher.statsRecord.numThreatBigs.ToString()));
		stringBuilder.AppendLine(string.Concat("NumEnemyRaids".Translate() + ": ", Find.StoryWatcher.statsRecord.numRaidsEnemy.ToString()));
		stringBuilder.AppendLine();
		if (Find.CurrentMap != null)
		{
			stringBuilder.AppendLine(string.Concat("ThisMapDamageTaken".Translate() + ": ", Find.CurrentMap.damageWatcher.DamageTakenEver.ToString()));
		}
		stringBuilder.AppendLine(string.Concat("ColonistsKilled".Translate() + ": ", Find.StoryWatcher.statsRecord.colonistsKilled.ToString()));
		stringBuilder.AppendLine();
		stringBuilder.AppendLine(string.Concat("ColonistsLaunched".Translate() + ": ", Find.StoryWatcher.statsRecord.colonistsLaunched.ToString()));
		Text.Font = GameFont.Small;
		Text.Anchor = TextAnchor.UpperLeft;
		Widgets.Label(new Rect(0f, 0f, 400f, 400f), stringBuilder.ToString());
		Widgets.EndGroup();
	}

	private void DoMessagesPage(Rect rect)
	{
		Rect rect2 = rect;
		rect.yMin += 10f;
		Widgets.CheckboxLabeled(new Rect(rect.x, rect.y, 200f, 30f), "ShowLetters".Translate(), ref showLetters, disabled: false, null, null, placeCheckboxNearText: true);
		Widgets.CheckboxLabeled(new Rect(rect.x + 200f, rect.y, 200f, 30f), "ShowMessages".Translate(), ref showMessages, disabled: false, null, null, placeCheckboxNearText: true);
		rect.yMin += 40f;
		bool flag = false;
		Rect outRect = rect;
		Rect viewRect = new Rect(0f, 0f, outRect.width / 2f - 16f, messagesLastHeight);
		List<IArchivable> archivablesListForReading = Find.Archive.ArchivablesListForReading;
		Rect rect3 = new Rect(rect.x + rect.width / 2f + 10f, rect.y, rect.width / 2f - 10f - 16f, rect.height);
		displayedMessageIndex = -1;
		quickSearchWidget.noResultsMatched = !archivablesListForReading.Any();
		Widgets.BeginScrollView(outRect, ref messagesScrollPos, viewRect);
		float num = 0f;
		for (int num2 = archivablesListForReading.Count - 1; num2 >= 0; num2--)
		{
			if ((showLetters || (!(archivablesListForReading[num2] is Letter) && !(archivablesListForReading[num2] is ArchivedDialog))) && (showMessages || !(archivablesListForReading[num2] is Message)))
			{
				flag = true;
				if (num2 > displayedMessageIndex)
				{
					displayedMessageIndex = num2;
				}
				if (num + 30f >= messagesScrollPos.y && num <= messagesScrollPos.y + outRect.height)
				{
					DoArchivableRow(new Rect(0f, num, viewRect.width - 5f, 30f), archivablesListForReading[num2], num2);
				}
				num += 30f;
			}
		}
		messagesLastHeight = num;
		Widgets.EndScrollView();
		if (flag)
		{
			if (displayedMessageIndex >= 0)
			{
				TaggedString label = archivablesListForReading[displayedMessageIndex].ArchivedTooltip.TruncateHeight(rect3.width - 10f, rect3.height - 10f, truncationCache);
				Widgets.Label(rect3.ContractedBy(5f), label);
			}
		}
		else
		{
			Widgets.NoneLabel(rect.yMin + 3f, rect.width, "(" + "NoMessages".Translate() + ")");
		}
		Rect rect4 = new Rect(rect2.x + SearchBarOffset.x, rect2.y + SearchBarOffset.y - Window.QuickSearchSize.y - 10f, Window.QuickSearchSize.x, Window.QuickSearchSize.y);
		quickSearchWidget.OnGUI(rect4, Notify_CommonSearchChanged);
	}

	private void DoArchivableRow(Rect rect, IArchivable archivable, int index)
	{
		Text.Font = GameFont.Small;
		Text.Anchor = TextAnchor.MiddleLeft;
		Text.WordWrap = false;
		Rect rect2 = rect;
		bool flag = quickSearchWidget.filter.Active && quickSearchWidget.filter.Matches(archivable.ArchivedLabel);
		if (flag)
		{
			Widgets.DrawTextHighlight(rect, 0f);
			if (quickSearchWidget.filter.Active && quickSearchWidget.CurrentlyFocused())
			{
				displayedMessageIndex = index;
			}
		}
		else if (index % 2 == 1)
		{
			Widgets.DrawLightHighlight(rect);
		}
		Widgets.DrawHighlightIfMouseover(rect);
		Rect rect3 = rect2;
		rect3.width = 30f;
		rect2.xMin += 35f;
		float num = (Find.Archive.IsPinned(archivable) ? 1f : ((!Mouse.IsOver(rect3)) ? 0f : 0.25f));
		Rect position = new Rect(rect3.x + (rect3.width - 22f) / 2f, rect3.y + (rect3.height - 22f) / 2f, 22f, 22f).Rounded();
		if (num > 0f)
		{
			GUI.color = new Color(1f, 1f, 1f, num);
			GUI.DrawTexture(position, PinTex);
		}
		else
		{
			GUI.color = PinOutlineColor;
			GUI.DrawTexture(position, PinOutlineTex);
		}
		GUI.color = Color.white;
		Rect rect4 = rect2;
		Rect outerRect = rect2;
		outerRect.width = 30f;
		rect2.xMin += 35f;
		Texture archivedIcon = archivable.ArchivedIcon;
		if (archivedIcon != null)
		{
			GUI.color = archivable.ArchivedIconColor;
			Widgets.DrawTextureFitted(outerRect, archivedIcon, 0.8f);
			GUI.color = Color.white;
		}
		Rect rect5 = rect2;
		rect5.width = 90f;
		rect2.xMin += 95f;
		Vector2 location = ((Find.CurrentMap != null) ? Find.WorldGrid.LongLatOf(Find.CurrentMap.Tile) : default(Vector2));
		GUI.color = new Color(0.75f, 0.75f, 0.75f);
		Widgets.Label(label: GenDate.DateShortStringAt(GenDate.TickGameToAbs(archivable.CreatedTicksGame), location).Truncate(rect5.width), rect: rect5);
		GUI.color = Color.white;
		Rect rect6 = rect2;
		if (!flag)
		{
			GUI.color = Color.gray;
		}
		Widgets.Label(rect6, archivable.ArchivedLabel.Truncate(rect6.width));
		GenUI.ResetLabelAlign();
		Text.WordWrap = true;
		GUI.color = Color.white;
		TooltipHandler.TipRegionByKey(rect3, "PinArchivableTip", 200);
		if (Mouse.IsOver(rect4))
		{
			displayedMessageIndex = index;
		}
		if (Widgets.ButtonInvisible(rect3))
		{
			if (Find.Archive.IsPinned(archivable))
			{
				Find.Archive.Unpin(archivable);
				SoundDefOf.Checkbox_TurnedOff.PlayOneShotOnCamera();
			}
			else
			{
				Find.Archive.Pin(archivable);
				SoundDefOf.Checkbox_TurnedOn.PlayOneShotOnCamera();
			}
		}
		if (!Widgets.ButtonInvisible(rect4))
		{
			return;
		}
		if (Event.current.button == 1)
		{
			LookTargets lookTargets = archivable.LookTargets;
			if (CameraJumper.CanJump(lookTargets.TryGetPrimaryTarget()))
			{
				CameraJumper.TryJumpAndSelect(lookTargets.TryGetPrimaryTarget());
				Find.MainTabsRoot.EscapeCurrentTab();
			}
		}
		else
		{
			archivable.OpenArchived();
		}
	}

	private void DoGraphPage(Rect rect)
	{
		rect.yMin += 17f;
		Widgets.BeginGroup(rect);
		Rect graphRect = new Rect(0f, 0f, rect.width, 450f);
		Rect legendRect = new Rect(0f, graphRect.yMax, rect.width / 2f, 40f);
		Rect rect2 = new Rect(0f, legendRect.yMax, rect.width, 40f);
		if (historyAutoRecorderGroup != null)
		{
			marks.Clear();
			List<Tale> allTalesListForReading = Find.TaleManager.AllTalesListForReading;
			for (int i = 0; i < allTalesListForReading.Count; i++)
			{
				Tale tale = allTalesListForReading[i];
				if (tale.def.type == TaleType.PermanentHistorical && !tale.hidden)
				{
					float x = (float)GenDate.TickAbsToGame(tale.date) / 60000f;
					marks.Add(new CurveMark(x, tale.ShortSummary, tale.def.historyGraphColor));
				}
			}
			historyAutoRecorderGroup.DrawGraph(graphRect, legendRect, graphSection, marks);
		}
		Text.Font = GameFont.Small;
		float num = (float)Find.TickManager.TicksGame / 60000f;
		if (Widgets.ButtonText(new Rect(legendRect.xMin + legendRect.width, legendRect.yMin, 110f, 40f), "Last30Days".Translate()))
		{
			graphSection = new FloatRange(Mathf.Max(0f, num - 30f), num);
			SoundDefOf.Click.PlayOneShotOnCamera();
		}
		if (Widgets.ButtonText(new Rect(legendRect.xMin + legendRect.width + 110f + 4f, legendRect.yMin, 110f, 40f), "Last100Days".Translate()))
		{
			graphSection = new FloatRange(Mathf.Max(0f, num - 100f), num);
			SoundDefOf.Click.PlayOneShotOnCamera();
		}
		if (Widgets.ButtonText(new Rect(legendRect.xMin + legendRect.width + 228f, legendRect.yMin, 110f, 40f), "Last300Days".Translate()))
		{
			graphSection = new FloatRange(Mathf.Max(0f, num - 300f), num);
			SoundDefOf.Click.PlayOneShotOnCamera();
		}
		if (Widgets.ButtonText(new Rect(legendRect.xMin + legendRect.width + 342f, legendRect.yMin, 110f, 40f), "AllDays".Translate()))
		{
			graphSection = new FloatRange(0f, num);
			SoundDefOf.Click.PlayOneShotOnCamera();
		}
		if (Widgets.ButtonText(new Rect(rect2.x, rect2.y, 110f, 40f), "SelectGraph".Translate()))
		{
			List<FloatMenuOption> list = new List<FloatMenuOption>();
			List<HistoryAutoRecorderGroup> list2 = Find.History.Groups();
			for (int j = 0; j < list2.Count; j++)
			{
				HistoryAutoRecorderGroup groupLocal = list2[j];
				if (!groupLocal.def.devModeOnly || Prefs.DevMode)
				{
					list.Add(new FloatMenuOption(groupLocal.def.LabelCap, delegate
					{
						historyAutoRecorderGroup = groupLocal;
					}));
				}
			}
			FloatMenu window = new FloatMenu(list, "SelectGraph".Translate());
			Find.WindowStack.Add(window);
			PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.HistoryTab, KnowledgeAmount.Total);
		}
		Widgets.EndGroup();
	}

	public override void Notify_ClickOutsideWindow()
	{
		quickSearchWidget.Unfocus();
	}
}
