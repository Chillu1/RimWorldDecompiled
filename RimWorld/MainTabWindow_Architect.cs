using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.Sound;
using Verse.Steam;

namespace RimWorld;

public class MainTabWindow_Architect : MainTabWindow
{
	private List<ArchitectCategoryTab> desPanelsCached = new List<ArchitectCategoryTab>();

	public ArchitectCategoryTab selectedDesPanel;

	private QuickSearchWidget quickSearchWidget = new QuickSearchWidget();

	private bool userForcedSelectionDuringSearch;

	private Designator forceActivatedCommand;

	private bool didInitialUnfocus;

	public const float WinWidth = 200f;

	private const float ButHeight = 32f;

	private static readonly Color InactiveSearchColor = Color.white.ToTransparent(0.4f);

	private static readonly Color NoMatchColor = Color.grey;

	public const int ColumnCount = 2;

	private static readonly Color InactiveColor = new Color(1f, 1f, 1f, 0.1f);

	private static readonly CachedTexture ChangeStyleIcon = new CachedTexture("UI/Icons/ChangeSelectedStyles");

	public float WinHeight => (float)Mathf.CeilToInt((float)desPanelsCached.Count / 2f) * 32f + 28f;

	public override Vector2 RequestedTabSize => new Vector2(200f, WinHeight);

	protected override float Margin => 0f;

	public bool QuickSearchWidgetFocused => quickSearchWidget.CurrentlyFocused();

	public float PaneTopY => (float)UI.screenHeight - WinHeight - 35f;

	public MainTabWindow_Architect()
	{
		closeOnAccept = false;
		quickSearchWidget.inactiveTextColor = InactiveSearchColor;
		CacheDesPanels();
	}

	public override void PreOpen()
	{
		base.PreOpen();
		ResetAndUnfocusQuickSearch();
		forceActivatedCommand = null;
		didInitialUnfocus = false;
	}

	public override void WindowUpdate()
	{
		base.WindowUpdate();
		ArchitectCategoryTab architectCategoryTab = OpenTab();
		if (architectCategoryTab != null && architectCategoryTab.def.showPowerGrid)
		{
			OverlayDrawHandler.DrawPowerGridOverlayThisFrame();
		}
	}

	public override void ExtraOnGUI()
	{
		base.ExtraOnGUI();
		OpenTab()?.DesignationTabOnGUI(forceActivatedCommand);
		forceActivatedCommand = null;
	}

	private ArchitectCategoryTab OpenTab()
	{
		if (!quickSearchWidget.filter.Active || userForcedSelectionDuringSearch)
		{
			return selectedDesPanel;
		}
		if (selectedDesPanel != null && selectedDesPanel.AnySearchMatches)
		{
			return selectedDesPanel;
		}
		foreach (ArchitectCategoryTab item in desPanelsCached)
		{
			if (item.AnySearchMatches)
			{
				return item;
			}
		}
		return null;
	}

	private void DoCategoryButton(ArchitectCategoryTab panel, float butWidth, float curXInd, float curYInd, ArchitectCategoryTab openTab, bool enabled)
	{
		Rect rect = new Rect(curXInd * butWidth, curYInd * 32f, butWidth, 32f);
		rect.height++;
		if (curXInd == 0f)
		{
			rect.width += 1f;
		}
		Color? labelColor = (panel.AnySearchMatches ? ((Color?)null) : new Color?(NoMatchColor));
		string label = panel.def.LabelCap;
		if (!enabled)
		{
			if (!labelColor.HasValue)
			{
				labelColor = InactiveColor;
			}
			GUI.color = InactiveColor;
		}
		if (Widgets.ButtonTextSubtle(rect, label, 0f, 8f, SoundDefOf.Mouseover_Category, new Vector2(-1f, -1f), labelColor, quickSearchWidget.filter.Active && openTab == panel))
		{
			ClickedCategory(panel);
		}
		GUI.color = Color.white;
		if (selectedDesPanel != panel)
		{
			UIHighlighter.HighlightOpportunity(rect, panel.def.cachedHighlightClosedTag);
		}
	}

	private void DoCategoryRow(ArchitectCategoryTab left, ArchitectCategoryTab right, float butWidth, float curYInd, ArchitectCategoryTab openTab)
	{
		if (left != null)
		{
			DoCategoryButton(left, butWidth, 0f, curYInd, openTab, left.Visible);
		}
		if (right != null)
		{
			DoCategoryButton(right, butWidth, 1f, curYInd, openTab, right.Visible);
		}
	}

	public override void DoWindowContents(Rect inRect)
	{
		Text.Font = GameFont.Small;
		float butWidth = inRect.width / 2f;
		float num = 0f;
		bool flag = ModsConfig.IdeologyActive && Find.IdeoManager.classicMode;
		ArchitectCategoryTab architectCategoryTab = OpenTab();
		if (KeyBindingDefOf.Accept.KeyDownEvent)
		{
			if (quickSearchWidget.filter.Active && architectCategoryTab?.UniqueSearchMatch != null)
			{
				forceActivatedCommand = architectCategoryTab.UniqueSearchMatch;
				Event.current.Use();
			}
			else if (!SteamDeck.IsSteamDeck)
			{
				Close();
				Event.current.Use();
			}
		}
		for (int i = 0; i < desPanelsCached.Count; i += 2)
		{
			ArchitectCategoryTab architectCategoryTab2 = desPanelsCached[i];
			ArchitectCategoryTab architectCategoryTab3 = ((i + 1 < desPanelsCached.Count) ? desPanelsCached[i + 1] : null);
			if ((architectCategoryTab2.PreferredColumn == 1 || (architectCategoryTab3 != null && architectCategoryTab3.PreferredColumn == 0)) && architectCategoryTab2.PreferredColumn != 0 && (architectCategoryTab3 == null || architectCategoryTab3.PreferredColumn != 1))
			{
				DoCategoryRow(architectCategoryTab3, architectCategoryTab2, butWidth, num, architectCategoryTab);
			}
			else
			{
				DoCategoryRow(architectCategoryTab2, architectCategoryTab3, butWidth, num, architectCategoryTab);
			}
			num += 1f;
		}
		float num2 = inRect.width;
		if (flag)
		{
			num2 -= 32f;
		}
		Rect rect = new Rect(0f, num * 32f + 1f, num2, 24f);
		quickSearchWidget.OnGUI(rect, CacheSearchState);
		if (!didInitialUnfocus)
		{
			UI.UnfocusCurrentControl();
			didInitialUnfocus = true;
		}
		if (flag && Widgets.ButtonImage(new Rect(rect.xMax + 4f, rect.y, 24f, 24f).ContractedBy(2f), ChangeStyleIcon.Texture))
		{
			if (Find.WindowStack.IsOpen<Dialog_StyleSelection>())
			{
				Find.WindowStack.TryRemove(typeof(Dialog_StyleSelection));
			}
			else
			{
				Find.WindowStack.Add(new Dialog_StyleSelection());
			}
		}
	}

	private void CacheDesPanels()
	{
		desPanelsCached.Clear();
		foreach (DesignationCategoryDef item in DefDatabase<DesignationCategoryDef>.AllDefs.OrderByDescending((DesignationCategoryDef dc) => dc.order))
		{
			desPanelsCached.Add(new ArchitectCategoryTab(item, quickSearchWidget.filter));
		}
	}

	private void CacheSearchState()
	{
		bool flag = false;
		foreach (ArchitectCategoryTab item in desPanelsCached)
		{
			item.CacheSearchState();
			flag |= item.AnySearchMatches;
		}
		quickSearchWidget.noResultsMatched = !flag;
		if (!quickSearchWidget.filter.Active)
		{
			userForcedSelectionDuringSearch = false;
		}
	}

	protected void ClickedCategory(ArchitectCategoryTab Pan)
	{
		if (!Pan.Visible)
		{
			Messages.Message("NothingAvailableInCategory".Translate() + ": " + Pan.def.LabelCap, MessageTypeDefOf.RejectInput, historical: false);
			return;
		}
		if (selectedDesPanel == Pan && !quickSearchWidget.filter.Active)
		{
			selectedDesPanel = null;
		}
		else
		{
			selectedDesPanel = Pan;
		}
		if (quickSearchWidget.filter.Active)
		{
			userForcedSelectionDuringSearch = true;
		}
		SoundDefOf.ArchitectCategorySelect.PlayOneShotOnCamera();
	}

	public override void Notify_ClickOutsideWindow()
	{
		base.Notify_ClickOutsideWindow();
		quickSearchWidget.Unfocus();
	}

	public override void OnCancelKeyPressed()
	{
		if (quickSearchWidget.CurrentlyFocused())
		{
			ResetAndUnfocusQuickSearch();
		}
		else
		{
			base.OnCancelKeyPressed();
		}
	}

	private void ResetAndUnfocusQuickSearch()
	{
		quickSearchWidget.Reset();
		quickSearchWidget.Unfocus();
		CacheSearchState();
	}
}
