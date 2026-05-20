using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace LudeonTK;

public class Dialog_Debug : Window_Dev
{
	public static DebugActionNode rootNode;

	private DebugActionNode currentNode;

	private Dictionary<DebugTabMenuDef, DebugTabMenu> menus = new Dictionary<DebugTabMenuDef, DebugTabMenu>();

	private static Dictionary<DebugTabMenuDef, DebugActionNode> roots = new Dictionary<DebugTabMenuDef, DebugActionNode>();

	private List<DebugTabMenuDef> menuDefsSorted = new List<DebugTabMenuDef>();

	private DebugTabMenu currentTabMenu;

	private float totalOptionsHeight;

	private string filter;

	private bool focusFilter;

	private int currentHighlightIndex;

	private int prioritizedHighlightedIndex;

	private Vector2 scrollPosition;

	protected float curY;

	protected float curX;

	private int boundingRectCachedForFrame = -1;

	private Rect? boundingRectCached;

	private Rect? boundingRect;

	public float verticalSpacing = 2f;

	private float heightPerColumn;

	private const string FilterControlName = "DebugFilter";

	private const float DebugOptionsGap = 7f;

	private static readonly Color DisallowedColor = new Color(1f, 1f, 1f, 0.3f);

	private static readonly Vector2 FilterInputSize = new Vector2(200f, 30f);

	private const float AssumedBiggestElementHeight = 50f;

	private const float BackButtonWidth = 120f;

	private const float PinnableActionHeight = 22f;

	private const float TabHeight = 32f;

	private const float MaxTabWidth = 200f;

	public override bool IsDebug => true;

	public override Vector2 InitialSize => new Vector2(UI.screenWidth, UI.screenHeight);

	public string Filter => filter;

	private float FilterX
	{
		get
		{
			if (currentNode?.parent == null || !currentNode.parent.IsRoot)
			{
				return 130f;
			}
			return 0f;
		}
	}

	private int HighlightedIndex => currentTabMenu.HighlightedIndex(currentHighlightIndex, prioritizedHighlightedIndex);

	public Rect? BoundingRectCached
	{
		get
		{
			if (boundingRectCachedForFrame != Time.frameCount)
			{
				if (boundingRect.HasValue)
				{
					Rect value = boundingRect.Value;
					Vector2 vector = scrollPosition;
					value.x += vector.x;
					value.y += vector.y;
					boundingRectCached = value;
				}
				boundingRectCachedForFrame = Time.frameCount;
			}
			return boundingRectCached;
		}
	}

	public DebugActionNode CurrentNode => currentNode;

	public Dialog_Debug()
	{
		Setup();
		SwitchTab(DebugTabMenuDefOf.Actions);
	}

	public Dialog_Debug(DebugTabMenuDef def)
	{
		Setup();
		SwitchTab(def);
	}

	public void NewColumn(float columnWidth)
	{
		curY = 0f;
		curX += columnWidth + 17f;
	}

	protected void NewColumnIfNeeded(float columnWidth, float neededHeight)
	{
		if (curY + neededHeight > heightPerColumn)
		{
			NewColumn(columnWidth);
		}
	}

	public Rect GetRect(float height, float columnWidth, float widthPct = 1f)
	{
		NewColumnIfNeeded(columnWidth, height);
		Rect result = new Rect(curX, curY, columnWidth * widthPct, height);
		curY += height;
		return result;
	}

	private void Setup()
	{
		forcePause = true;
		doCloseX = true;
		onlyOneOfTypeAllowed = true;
		absorbInputAroundWindow = true;
		focusFilter = true;
		menuDefsSorted.AddRange(DefDatabase<DebugTabMenuDef>.AllDefs.ToList());
		menuDefsSorted.SortBy((DebugTabMenuDef x) => x.displayOrder, (DebugTabMenuDef y) => y.label);
		currentTabMenu?.Recache();
	}

	public void SwitchTab(DebugTabMenuDef def)
	{
		TrySetupNodeGraph();
		scrollPosition = Vector2.zero;
		currentHighlightIndex = 0;
		prioritizedHighlightedIndex = 0;
		currentTabMenu = (menus.ContainsKey(def) ? menus[def] : DebugTabMenu.CreateMenu(def, this, rootNode));
		currentTabMenu.Enter(roots[def]);
		SoundDefOf.RowTabSelect.PlayOneShotOnCamera();
	}

	public static void TrySetupNodeGraph()
	{
		if (rootNode != null)
		{
			return;
		}
		rootNode = new DebugActionNode("Root");
		foreach (DebugTabMenuDef allDef in DefDatabase<DebugTabMenuDef>.AllDefs)
		{
			roots.Add(allDef, DebugTabMenu.CreateMenu(allDef, null, rootNode).InitActions(rootNode));
		}
	}

	private void DrawTabs(Rect rect)
	{
		float num = Mathf.Min(rect.width / (float)menuDefsSorted.Count, 200f);
		for (int i = 0; i < menuDefsSorted.Count; i++)
		{
			DebugTabMenuDef debugTabMenuDef = menuDefsSorted[i];
			Rect rect2 = new Rect(rect.x + (float)i * num, rect.y, num, rect.height).ContractedBy(1f);
			if (debugTabMenuDef == currentTabMenu.def)
			{
				GUI.DrawTexture(rect2, DevGUI.ButtonBackgroundClick);
				Text.Anchor = TextAnchor.MiddleCenter;
				DevGUI.Label(rect2, debugTabMenuDef.LabelCap);
				Text.Anchor = TextAnchor.UpperLeft;
			}
			else if (DevGUI.ButtonText(rect2, debugTabMenuDef.LabelCap))
			{
				SwitchTab(debugTabMenuDef);
			}
		}
	}

	public override void DoWindowContents(Rect inRect)
	{
		if (KeyBindingDefOf.Dev_ChangeSelectedDebugAction.KeyDownEvent)
		{
			int highlightedIndex = HighlightedIndex;
			if (highlightedIndex >= 0)
			{
				for (int i = 0; i < currentTabMenu.Count; i++)
				{
					int index = (highlightedIndex + i + 1) % currentTabMenu.Count;
					if (FilterAllows(currentTabMenu.LabelAtIndex(index)))
					{
						prioritizedHighlightedIndex = index;
						break;
					}
				}
			}
		}
		GUI.SetNextControlName("DebugFilter");
		Text.Font = GameFont.Small;
		Rect rect = new Rect(FilterX, 0f, FilterInputSize.x, FilterInputSize.y);
		filter = DevGUI.TextField(rect, filter);
		Rect rect2 = new Rect(rect.xMax + 10f, rect.y, inRect.width - rect.width - 10f, 32f);
		DrawTabs(rect2);
		if ((Event.current.type == EventType.KeyDown || Event.current.type == EventType.Repaint) && focusFilter)
		{
			GUI.FocusControl("DebugFilter");
			filter = string.Empty;
			focusFilter = false;
		}
		if (Event.current.type == EventType.Layout)
		{
			totalOptionsHeight = 0f;
		}
		Rect outRect = new Rect(inRect);
		outRect.yMin += 42f;
		int num = (int)(InitialSize.x / 200f);
		heightPerColumn = Mathf.Max(outRect.height, (totalOptionsHeight + 50f * (float)(num - 1)) / (float)num);
		curX = 0f;
		curY = 0f;
		Rect rect3 = new Rect(0f, 0f, outRect.width - 16f, heightPerColumn);
		DevGUI.BeginScrollView(outRect, ref scrollPosition, rect3);
		DevGUI.BeginGroup(rect3);
		float columnWidth = (rect3.width - 17f * (float)(num - 1)) / (float)num;
		currentTabMenu.ListOptions(HighlightedIndex, columnWidth);
		DevGUI.EndGroup();
		DevGUI.EndScrollView();
		if (currentNode.parent != null && !currentNode.parent.IsRoot)
		{
			GameFont font = Text.Font;
			Text.Font = GameFont.Small;
			if (DevGUI.ButtonText(new Rect(0f, 0f, 120f, 32f), "Back"))
			{
				currentNode.parent.Enter(this);
			}
			if (!currentNode.IsRoot)
			{
				Text.Anchor = TextAnchor.UpperRight;
				Text.Font = GameFont.Tiny;
				DevGUI.Label(new Rect(0f, 0f, outRect.width - 24f - 10f, 32f), currentNode.Path.Colorize(ColoredText.SubtleGrayColor));
				Text.Anchor = TextAnchor.UpperLeft;
			}
			Text.Font = font;
		}
	}

	public override void OnAcceptKeyPressed()
	{
		if (GUI.GetNameOfFocusedControl() == "DebugFilter")
		{
			int highlightedIndex = HighlightedIndex;
			currentTabMenu.OnAcceptKeyPressed(highlightedIndex);
			Event.current.Use();
		}
	}

	public override void OnCancelKeyPressed()
	{
		if (currentNode.parent != null && !currentNode.parent.IsRoot)
		{
			currentNode.parent.Enter(this);
			Event.current.Use();
		}
		else
		{
			base.OnCancelKeyPressed();
		}
	}

	public static DebugActionNode GetNode(string path)
	{
		TrySetupNodeGraph();
		DebugActionNode debugActionNode = rootNode;
		string[] s = path.Split('\\');
		int i;
		for (i = 0; i < s.Length; i++)
		{
			DebugActionNode debugActionNode2 = debugActionNode.children.FirstOrDefault((DebugActionNode x) => x.label == s[i]);
			if (debugActionNode2 == null)
			{
				return null;
			}
			debugActionNode = debugActionNode2;
			debugActionNode.TrySetupChildren();
		}
		return debugActionNode;
	}

	public void SetCurrentNode(DebugActionNode node)
	{
		currentNode = node;
		foreach (DebugActionNode child in currentNode.children)
		{
			child.DirtyLabelCache();
		}
		scrollPosition = Vector2.zero;
		filter = string.Empty;
		currentHighlightIndex = 0;
		prioritizedHighlightedIndex = 0;
		currentTabMenu?.Recache();
	}

	public void DrawNode(DebugActionNode node, float columnWidth, bool highlight)
	{
		if (node.settingsField != null)
		{
			DoCheckbox(node, columnWidth, highlight);
		}
		else
		{
			DoButton(node, columnWidth, highlight);
		}
	}

	public DebugActionButtonResult ButtonDebugPinnable(string label, float columnWidth, bool highlight, bool pinned)
	{
		Text.Font = GameFont.Tiny;
		NewColumnIfNeeded(columnWidth, 22f);
		Rect rect = new Rect(curX, curY, columnWidth - 22f, 22f);
		DebugActionButtonResult result = DebugActionButtonResult.None;
		if (!BoundingRectCached.HasValue || rect.Overlaps(BoundingRectCached.Value))
		{
			result = DevGUI.ButtonDebugPinnable(rect, label, highlight, pinned);
		}
		curY += 22f + verticalSpacing;
		return result;
	}

	public DebugActionButtonResult CheckboxPinnable(string label, float columnWidth, ref bool checkOn, bool highlight, bool pinned)
	{
		Text.Font = GameFont.Tiny;
		NewColumnIfNeeded(columnWidth, 22f);
		Rect rect = new Rect(curX, curY, columnWidth - 22f, 22f);
		DebugActionButtonResult result = DebugActionButtonResult.None;
		if (!BoundingRectCached.HasValue || rect.Overlaps(BoundingRectCached.Value))
		{
			result = DevGUI.CheckboxPinnable(rect, label, ref checkOn, highlight, pinned);
		}
		curY += 22f + verticalSpacing;
		return result;
	}

	private void DoButton(DebugActionNode node, float columnWidth, bool highlight)
	{
		string labelNow = node.LabelNow;
		if (!FilterAllows(labelNow))
		{
			GUI.color = DisallowedColor;
		}
		switch (ButtonDebugPinnable(labelNow, columnWidth, highlight, Prefs.DebugActionsPalette.Contains(node.Path)))
		{
		case DebugActionButtonResult.ButtonPressed:
			node.Enter(this);
			break;
		case DebugActionButtonResult.PinPressed:
			Dialog_DevPalette.ToggleAction(node.Path);
			break;
		}
		GUI.color = Color.white;
		if (Event.current.type == EventType.Layout)
		{
			totalOptionsHeight += 22f + verticalSpacing;
		}
	}

	private void DoCheckbox(DebugActionNode node, float columnWidth, bool highlight)
	{
		string labelNow = node.LabelNow;
		FieldInfo settingsField = node.settingsField;
		bool checkOn = (bool)settingsField.GetValue(null);
		bool flag = checkOn;
		if (!FilterAllows(labelNow))
		{
			GUI.color = DisallowedColor;
		}
		switch (CheckboxPinnable(labelNow, columnWidth, ref checkOn, highlight, Prefs.DebugActionsPalette.Contains(node.Path)))
		{
		case DebugActionButtonResult.ButtonPressed:
			node.Enter(this);
			break;
		case DebugActionButtonResult.PinPressed:
			Dialog_DevPalette.ToggleAction(node.Path);
			break;
		}
		GUI.color = Color.white;
		if (Event.current.type == EventType.Layout)
		{
			totalOptionsHeight += Text.LineHeight;
		}
		if (checkOn != flag)
		{
			settingsField.SetValue(null, checkOn);
			MethodInfo method = settingsField.DeclaringType.GetMethod(settingsField.Name + "Toggled", BindingFlags.Static | BindingFlags.Public);
			if (method != null)
			{
				method.Invoke(null, null);
			}
		}
	}

	public void DoLabel(string label, float columnWidth)
	{
		Text.Font = GameFont.Small;
		float num = Text.CalcHeight(label, columnWidth);
		DevGUI.Label(new Rect(curX, curY, columnWidth, num), label);
		curY += num;
		if (Event.current.type == EventType.Layout)
		{
			totalOptionsHeight += num;
		}
	}

	public void DoGap(float gapSize = 7f)
	{
		curY += gapSize;
		if (Event.current.type == EventType.Layout)
		{
			totalOptionsHeight += gapSize;
		}
	}

	public bool FilterAllows(string label)
	{
		if (filter.NullOrEmpty())
		{
			return true;
		}
		if (label.NullOrEmpty())
		{
			return true;
		}
		return label.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0;
	}

	public static void ResetStaticData()
	{
		rootNode = null;
		roots.Clear();
	}
}
