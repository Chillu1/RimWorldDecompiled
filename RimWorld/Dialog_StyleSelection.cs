using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld;

public class Dialog_StyleSelection : Window
{
	private Color baseColor = Color.white;

	private static readonly List<ThingStyleCategoryWithPriority> tmpStyles = new List<ThingStyleCategoryWithPriority>();

	private const float FadeStartMouseDist = 5f;

	private const float FadeFinishMouseDist = 150f;

	private const int MaxButtonCount = 3;

	public override Vector2 InitialSize
	{
		get
		{
			float x = Mathf.Max(Text.CalcSize("StylesInUse".Translate()).x, 84f) + 10f + Margin * 2f;
			float y = Text.LineHeightOf(GameFont.Small) + 20f + 24f + Margin * 2f;
			return new Vector2(x, y);
		}
	}

	protected override float Margin => 4f;

	public Dialog_StyleSelection()
	{
		closeOnAccept = false;
		closeOnCancel = false;
		doWindowBackground = false;
		drawShadow = false;
		closeOnClickedOutside = true;
	}

	public override void DoWindowContents(Rect inRect)
	{
		UpdateBaseColor();
		GUI.color = baseColor;
		Text.Font = GameFont.Small;
		Widgets.DrawWindowBackground(inRect, baseColor);
		GUI.BeginGroup(inRect.ContractedBy(Margin));
		Widgets.Label(new Rect(0f, 0f, inRect.width, Text.LineHeight), "StylesInUse".Translate());
		float num = 0f;
		for (int i = 0; i < Find.IdeoManager.selectedStyleCategories.Count; i++)
		{
			StyleCategoryDef cat = Find.IdeoManager.selectedStyleCategories[i];
			Rect rect = new Rect(num, Text.LineHeight + 10f, 24f, 24f);
			if (Widgets.ButtonImage(rect, cat.Icon, baseColor))
			{
				List<FloatMenuOption> list = new List<FloatMenuOption>();
				foreach (StyleCategoryDef allDef in DefDatabase<StyleCategoryDef>.AllDefs)
				{
					StyleCategoryDef category = allDef;
					if (category != cat && !Find.IdeoManager.selectedStyleCategories.Contains(allDef))
					{
						list.Add(new FloatMenuOption(category.LabelCap, delegate
						{
							Find.IdeoManager.selectedStyleCategories.Replace(cat, category);
							ClearCaches();
						}, category.Icon, Color.white));
					}
				}
				list.Add(new FloatMenuOption("Remove".Translate().CapitalizeFirst(), delegate
				{
					Find.IdeoManager.selectedStyleCategories.Remove(cat);
					ClearCaches();
				}));
				Find.WindowStack.Add(new FloatMenu(list));
			}
			if (Mouse.IsOver(rect))
			{
				Widgets.DrawHighlight(rect);
				TooltipHandler.TipRegion(rect, IdeoUIUtility.StyleTooltip(cat, IdeoEditMode.GameStart, null, tmpStyles, skipDominanceDesc: true));
			}
			num += 28f;
		}
		if (Find.IdeoManager.selectedStyleCategories.Count < 3)
		{
			Rect rect2 = new Rect(num, Text.LineHeight + 10f, 24f, 24f);
			if (Widgets.ButtonImage(rect2, TexButton.Plus, baseColor))
			{
				List<FloatMenuOption> list2 = new List<FloatMenuOption>();
				foreach (StyleCategoryDef allDef2 in DefDatabase<StyleCategoryDef>.AllDefs)
				{
					StyleCategoryDef cat2 = allDef2;
					if (!Find.IdeoManager.selectedStyleCategories.Contains(cat2))
					{
						list2.Add(new FloatMenuOption(cat2.LabelCap, delegate
						{
							Find.IdeoManager.selectedStyleCategories.Add(cat2);
							ClearCaches();
						}, cat2.Icon, Color.white));
					}
				}
				if (list2.Any())
				{
					Find.WindowStack.Add(new FloatMenu(list2));
				}
			}
			if (Mouse.IsOver(rect2))
			{
				Widgets.DrawHighlight(rect2);
				TooltipHandler.TipRegion(rect2, "AddStyleCategory".Translate().Colorize(ColoredText.TipSectionTitleColor) + "\n\n" + "StyleCategoryDescriptionAbstract".Translate().Colorize(ColoredText.SubtleGrayColor) + "\n\n" + "ClickToEdit".Translate().CapitalizeFirst().Colorize(ColorLibrary.Green));
			}
		}
		GUI.EndGroup();
		GUI.color = Color.white;
	}

	private void ClearCaches()
	{
		Faction.OfPlayer.ideos.PrimaryIdeo.style.ResetStylesForThingDef();
		Faction.OfPlayer.ideos.PrimaryIdeo.RecachePossibleBuildables();
		foreach (DesignationCategoryDef allDef in DefDatabase<DesignationCategoryDef>.AllDefs)
		{
			allDef.DirtyCache();
		}
	}

	protected override void SetInitialSizeAndPosition()
	{
		Vector2 initialSize = InitialSize;
		Vector2 mousePositionOnUIInverted = UI.MousePositionOnUIInverted;
		windowRect = new Rect(mousePositionOnUIInverted.x, mousePositionOnUIInverted.y - initialSize.y, initialSize.x, initialSize.y).Rounded();
	}

	private void UpdateBaseColor()
	{
		if (Find.WindowStack.FloatMenu != null)
		{
			baseColor = Color.white;
			return;
		}
		Rect r = new Rect(0f, 0f, InitialSize.x, InitialSize.y).ExpandedBy(5f);
		if (!r.Contains(Event.current.mousePosition))
		{
			float num = GenUI.DistFromRect(r, Event.current.mousePosition);
			baseColor = new Color(1f, 1f, 1f, 1f - num / 145f);
			if (num > 145f)
			{
				Close(doCloseSound: false);
			}
		}
	}
}
