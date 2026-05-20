using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.Sound;
using Verse.Steam;

namespace RimWorld;

public class Dialog_KeyBindings : Window
{
	protected Vector2 scrollPosition;

	protected float contentHeight;

	protected KeyPrefsData keyPrefsData;

	protected Vector2 WindowSize = new Vector2(700f, 760f);

	protected const float EntryHeight = 34f;

	protected const float CategoryHeadingHeight = 40f;

	private static List<KeyBindingDef> keyBindingsWorkingList = new List<KeyBindingDef>();

	public override Vector2 InitialSize => WindowSize;

	public Dialog_KeyBindings()
	{
		forcePause = true;
		onlyOneOfTypeAllowed = true;
		absorbInputAroundWindow = true;
		scrollPosition = new Vector2(0f, 0f);
		keyPrefsData = KeyPrefs.KeyPrefsData.Clone();
		contentHeight = 0f;
		KeyBindingCategoryDef keyBindingCategoryDef = null;
		foreach (KeyBindingDef allDef in DefDatabase<KeyBindingDef>.AllDefs)
		{
			if (keyBindingCategoryDef != allDef.category)
			{
				keyBindingCategoryDef = allDef.category;
				contentHeight += 44f;
			}
			contentHeight += 34f;
		}
	}

	public override void DoWindowContents(Rect inRect)
	{
		Vector2 vector = new Vector2(120f, 40f);
		float y = vector.y;
		Rect rect = new Rect(inRect.x, inRect.y, inRect.width, inRect.height - (y + 10f)).ContractedBy(10f);
		Rect rect2 = new Rect(rect.x, rect.y + rect.height + 10f, rect.width, y);
		Widgets.BeginGroup(rect);
		Rect rect3 = new Rect(0f, 0f, rect.width, 40f);
		Text.Font = GameFont.Medium;
		GenUI.SetLabelAlign(TextAnchor.MiddleCenter);
		Widgets.Label(rect3, "KeyboardConfig".Translate());
		GenUI.ResetLabelAlign();
		Text.Font = GameFont.Small;
		Rect outRect = new Rect(0f, rect3.height, rect.width, rect.height - rect3.height);
		Rect rect4 = new Rect(0f, 0f, outRect.width - 16f, contentHeight);
		Widgets.BeginScrollView(outRect, ref scrollPosition, rect4);
		float curY = 0f;
		KeyBindingCategoryDef keyBindingCategoryDef = null;
		keyBindingsWorkingList.Clear();
		keyBindingsWorkingList.AddRange(DefDatabase<KeyBindingDef>.AllDefs);
		keyBindingsWorkingList.SortBy((KeyBindingDef x) => x.category.index, (KeyBindingDef x) => x.index);
		for (int num = 0; num < keyBindingsWorkingList.Count; num++)
		{
			KeyBindingDef keyBindingDef = keyBindingsWorkingList[num];
			if (keyBindingCategoryDef != keyBindingDef.category)
			{
				bool skipDrawing = curY - scrollPosition.y + 40f < 0f || curY - scrollPosition.y > outRect.height;
				keyBindingCategoryDef = keyBindingDef.category;
				DrawCategoryEntry(keyBindingCategoryDef, rect4.width, ref curY, skipDrawing);
			}
			bool skipDrawing2 = curY - scrollPosition.y + 34f < 0f || curY - scrollPosition.y > outRect.height;
			DrawKeyEntry(keyBindingDef, rect4, ref curY, skipDrawing2);
		}
		Widgets.EndScrollView();
		Widgets.EndGroup();
		Widgets.BeginGroup(rect2);
		Rect rect5 = new Rect(0f, 0f, vector.x, vector.y);
		Rect rect6 = new Rect((rect2.width - vector.x) / 2f, 0f, vector.x, vector.y);
		Rect rect7 = new Rect(rect2.width - vector.x, 0f, vector.x, vector.y);
		if (Widgets.ButtonText(rect6, "ResetButton".Translate()))
		{
			keyPrefsData.ResetToDefaults();
			keyPrefsData.ErrorCheck();
			SoundDefOf.Tick_Low.PlayOneShotOnCamera();
			Event.current.Use();
		}
		if (Widgets.ButtonText(rect5, "CancelButton".Translate()))
		{
			Close();
			Event.current.Use();
		}
		if (Widgets.ButtonText(rect7, "OK".Translate()))
		{
			KeyPrefs.KeyPrefsData = keyPrefsData;
			KeyPrefs.Save();
			Close();
			keyPrefsData.ErrorCheck();
			Event.current.Use();
		}
		Widgets.EndGroup();
	}

	private void DrawCategoryEntry(KeyBindingCategoryDef category, float width, ref float curY, bool skipDrawing)
	{
		if (!skipDrawing)
		{
			Rect rect = new Rect(0f, curY, width, 40f).ContractedBy(4f);
			Text.Font = GameFont.Medium;
			Widgets.Label(rect, category.LabelCap);
			Text.Font = GameFont.Small;
			if (Mouse.IsOver(rect) && !category.description.NullOrEmpty())
			{
				TooltipHandler.TipRegion(rect, new TipSignal(category.description));
			}
		}
		curY += 40f;
		if (!skipDrawing)
		{
			Color color = GUI.color;
			GUI.color = new Color(0.3f, 0.3f, 0.3f);
			Widgets.DrawLineHorizontal(0f, curY, width);
			GUI.color = color;
		}
		curY += 4f;
	}

	private void DrawKeyEntry(KeyBindingDef keyDef, Rect parentRect, ref float curY, bool skipDrawing)
	{
		if (!skipDrawing)
		{
			Rect rect = new Rect(parentRect.x, parentRect.y + curY, parentRect.width, 34f).ContractedBy(3f);
			GenUI.SetLabelAlign(TextAnchor.MiddleLeft);
			Widgets.Label(rect, keyDef.LabelCap);
			GenUI.ResetLabelAlign();
			float num = 4f;
			Vector2 vector = new Vector2(140f, 28f);
			Rect rect2 = new Rect(rect.x + rect.width - vector.x * 2f - num, rect.y, vector.x, vector.y);
			Rect rect3 = new Rect(rect.x + rect.width - vector.x, rect.y, vector.x, vector.y);
			string key = (SteamDeck.IsSteamDeckInNonKeyboardMode ? "BindingButtonToolTipController" : "BindingButtonToolTip");
			TooltipHandler.TipRegionByKey(rect2, key);
			TooltipHandler.TipRegionByKey(rect3, key);
			if (Widgets.ButtonText(rect2, keyPrefsData.GetBoundKeyCode(keyDef, KeyPrefs.BindingSlot.A).ToStringReadable()))
			{
				SettingButtonClicked(keyDef, KeyPrefs.BindingSlot.A);
			}
			if (Widgets.ButtonText(rect3, keyPrefsData.GetBoundKeyCode(keyDef, KeyPrefs.BindingSlot.B).ToStringReadable()))
			{
				SettingButtonClicked(keyDef, KeyPrefs.BindingSlot.B);
			}
		}
		curY += 34f;
	}

	private void SettingButtonClicked(KeyBindingDef keyDef, KeyPrefs.BindingSlot slot)
	{
		if (Event.current.button == 0)
		{
			Find.WindowStack.Add(new Dialog_DefineBinding(keyPrefsData, keyDef, slot));
			Event.current.Use();
		}
		else if (Event.current.button == 1)
		{
			List<FloatMenuOption> list = new List<FloatMenuOption>();
			list.Add(new FloatMenuOption("ResetBinding".Translate(), delegate
			{
				KeyCode keyCode = ((slot == KeyPrefs.BindingSlot.A) ? keyDef.defaultKeyCodeA : keyDef.defaultKeyCodeB);
				keyPrefsData.SetBinding(keyDef, slot, keyCode);
			}));
			list.Add(new FloatMenuOption("ClearBinding".Translate(), delegate
			{
				keyPrefsData.SetBinding(keyDef, slot, KeyCode.None);
			}));
			Find.WindowStack.Add(new FloatMenu(list));
		}
	}
}
