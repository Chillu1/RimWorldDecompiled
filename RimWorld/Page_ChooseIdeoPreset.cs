using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimWorld;

[StaticConstructorOnStartup]
public class Page_ChooseIdeoPreset : Page
{
	private enum PresetSelection
	{
		Classic,
		CustomFluid,
		CustomFixed,
		Load,
		Preset
	}

	private IdeoPresetDef selectedIdeo;

	private Ideo classicIdeo;

	private MemeDef selectedStructure;

	private List<StyleCategoryDef> selectedStyles = new List<StyleCategoryDef> { null };

	private List<ThingStyleCategoryWithPriority> selectedStylesWithPriority = new List<ThingStyleCategoryWithPriority> { null };

	private PresetSelection presetSelection;

	private Vector2 leftScrollPosition;

	private float totalCategoryListHeight;

	private string lastCategoryGroupLabel;

	private Texture2D randomStructureIcon;

	private const float IdeosRowWidth = 620f;

	private const float CategoryDescRowWidth = 300f;

	private const float CategoryDescRowWidthSmall = 500f;

	private const float CategoryRowWidth = 937f;

	private const float MemeIconSize = 50f;

	private const float IdeoBoxMargin = 5f;

	private const float IdeoBoxWidthMin = 110f;

	private static readonly Vector2 ButtonSize = new Vector2(160f, 60f);

	private static readonly Vector2 ButtonSizeSmall = new Vector2(120f, 40f);

	private static readonly Texture2D PlusTex = ContentFinder<Texture2D>.Get("UI/Buttons/Plus");

	private List<int> ideosPerRow = new List<int>();

	public override string PageTitle => "ChooseYourIdeoligion".Translate();

	public Texture2D RandomIcon
	{
		get
		{
			if (randomStructureIcon == null)
			{
				randomStructureIcon = ContentFinder<Texture2D>.Get("UI/Structures/Random");
			}
			return randomStructureIcon;
		}
	}

	public override void PostOpen()
	{
		base.PostOpen();
		IdeoGenerationParms genParms = new IdeoGenerationParms(Find.FactionManager.OfPlayer.def);
		if (!DefDatabase<CultureDef>.AllDefs.Where((CultureDef x) => Find.FactionManager.OfPlayer.def.allowedCultures.Contains(x)).TryRandomElement(out var result))
		{
			result = DefDatabase<CultureDef>.AllDefs.RandomElement();
		}
		classicIdeo = IdeoGenerator.GenerateClassicIdeo(result, genParms, noExpansionIdeo: false);
		Find.IdeoManager.classicMode = false;
		foreach (Faction allFaction in Find.FactionManager.AllFactions)
		{
			if (allFaction != Faction.OfPlayer && allFaction.ideos != null && allFaction.ideos.PrimaryIdeo.memes.NullOrEmpty())
			{
				FactionDef def = allFaction.def;
				if (allFaction.def.fixedIdeo)
				{
					IdeoGenerationParms parms = new IdeoGenerationParms(def, forceNoExpansionIdeo: false, null, null, name: def.ideoName, styles: def.styles, deities: def.deityPresets, hidden: def.hiddenIdeo, description: def.ideoDescription, forcedMemes: def.forcedMemes, classicExtra: false, forceNoWeaponPreference: false, forNewFluidIdeo: false, fixedIdeo: true, requiredPreceptsOnly: def.requiredPreceptsOnly);
					allFaction.ideos.ChooseOrGenerateIdeo(parms);
				}
				else
				{
					allFaction.ideos.ChooseOrGenerateIdeo(new IdeoGenerationParms(allFaction.def));
				}
			}
		}
		AssignIdeoToPlayer(classicIdeo);
		Faction.OfPlayer.ideos.SetPrimary(classicIdeo);
		Find.IdeoManager.RemoveUnusedStartingIdeos();
		if (Find.Storyteller.def.tutorialMode)
		{
			DoNext();
		}
	}

	public override void DoWindowContents(Rect inRect)
	{
		DrawPageTitle(inRect);
		float num = 0f;
		Rect mainRect = GetMainRect(inRect);
		TaggedString taggedString = "ChooseYourIdeoligionDesc".Translate();
		float num2 = Text.CalcHeight(taggedString, mainRect.width);
		Rect rect = mainRect;
		rect.yMin += num;
		Widgets.Label(rect, taggedString);
		num += num2 + 10f;
		DrawStructureAndStyleSelection(inRect);
		Rect outRect = mainRect;
		outRect.width = 954f;
		outRect.yMin += num;
		float num3 = (InitialSize.x - 937f) / 2f;
		float x = (inRect.width - ButtonSize.x - 10f - 500f - 16f) / 2f - num3;
		Widgets.BeginScrollView(viewRect: new Rect(0f - num3, 0f, 921f, totalCategoryListHeight + 100f), outRect: outRect, scrollPosition: ref leftScrollPosition);
		num = 0f;
		lastCategoryGroupLabel = "";
		Rect rect2 = new Rect(x, num + Text.LineHeight, ButtonSize.x, ButtonSize.y);
		DrawSplitCategoryInfo(IdeoPresetCategoryDefOf.Classic, rect2);
		DrawSelectable(rect2, "PlayClassic".Translate(), null, TextAnchor.MiddleCenter, presetSelection == PresetSelection.Classic, tutorAllows: true, null, delegate
		{
			selectedIdeo = null;
			presetSelection = PresetSelection.Classic;
		});
		num = Mathf.Max(0f, rect2.yMax) + Text.LineHeight;
		Widgets.Label(new Rect(0f, num, 300f, Text.LineHeight), "CustomIdeoligions".Translate());
		GUI.color = new Color(1f, 1f, 1f, 0.5f);
		Widgets.DrawLineHorizontal(0f, num + Text.LineHeight + 2f, 901f);
		GUI.color = Color.white;
		num += 12f;
		float a = num;
		Rect rect3 = new Rect(x, num + Text.LineHeight, ButtonSize.x, ButtonSize.y);
		DrawSplitCategoryInfo(IdeoPresetCategoryDefOf.Fluid, rect3);
		DrawSelectable(rect3, "CreateCustomFluid".Translate(), null, TextAnchor.MiddleCenter, presetSelection == PresetSelection.CustomFluid, tutorAllows: true, null, delegate
		{
			selectedIdeo = null;
			presetSelection = PresetSelection.CustomFluid;
		});
		float num4 = Mathf.Max(a, rect3.yMax);
		num = num4 + 10f;
		Rect rect4 = new Rect(x, num + Text.LineHeight, ButtonSize.x, ButtonSize.y);
		DrawSplitCategoryInfo(IdeoPresetCategoryDefOf.Custom, rect4);
		DrawSelectable(rect4, "CreateCustomFixed".Translate(), null, TextAnchor.MiddleCenter, presetSelection == PresetSelection.CustomFixed, tutorAllows: true, null, delegate
		{
			selectedIdeo = null;
			presetSelection = PresetSelection.CustomFixed;
		});
		Rect rect5 = new Rect(rect4.xMax - ButtonSizeSmall.x, rect4.yMax + 10f, ButtonSizeSmall.x, ButtonSizeSmall.y);
		DrawSelectable(rect5, "LoadSaved".Translate() + "...", null, TextAnchor.MiddleCenter, presetSelection == PresetSelection.Load, tutorAllows: true, null, delegate
		{
			selectedIdeo = null;
			presetSelection = PresetSelection.Load;
		});
		num = Mathf.Max(num4, rect5.yMax) + 10f;
		foreach (IdeoPresetCategoryDef item in DefDatabase<IdeoPresetCategoryDef>.AllDefsListForReading.Where((IdeoPresetCategoryDef c) => c != IdeoPresetCategoryDefOf.Classic && c != IdeoPresetCategoryDefOf.Custom && c != IdeoPresetCategoryDefOf.Fluid))
		{
			DrawCategory(item, ref num);
		}
		totalCategoryListHeight = num;
		Widgets.EndScrollView();
		DoBottomButtons(inRect);
		static void DrawSplitCategoryInfo(IdeoPresetCategoryDef cat, Rect buttonRect)
		{
			Rect rect6 = new Rect(buttonRect.xMax + 10f, buttonRect.y, 500f, buttonRect.height);
			Text.Anchor = TextAnchor.MiddleLeft;
			Widgets.Label(rect6, cat.description);
			Text.Anchor = TextAnchor.UpperLeft;
		}
	}

	private void DrawCategory(IdeoPresetCategoryDef category, ref float curY)
	{
		float num = 0f;
		if (category.groupLabel != lastCategoryGroupLabel)
		{
			curY += 16f;
			Widgets.Label(new Rect(0f, curY, 300f, Text.LineHeight), category.groupLabel);
			GUI.color = new Color(1f, 1f, 1f, 0.5f);
			Widgets.DrawLineHorizontal(0f, curY + Text.LineHeight + 2f, 901f);
			GUI.color = Color.white;
			num += Text.LineHeight + 12f;
		}
		if (category.LabelCap != category.groupLabel)
		{
			Rect rect = new Rect(0f, curY + num, 300f, Text.LineHeight);
			num += Text.LineHeight;
			Widgets.Label(rect, category.LabelCap);
		}
		float height = Text.CalcHeight(category.description, 300f);
		Widgets.Label(new Rect(0f, curY + num, 300f, height), category.description);
		curY += num;
		ideosPerRow.Clear();
		int num2 = 0;
		float num3 = 0f;
		ideosPerRow.Add(0);
		foreach (IdeoPresetDef item in DefDatabase<IdeoPresetDef>.AllDefs.Where((IdeoPresetDef i) => i.categoryDef == category))
		{
			float num4 = RectForIdeo(item).width + 17f;
			if (num3 + num4 <= 620f)
			{
				ideosPerRow[num2]++;
				num3 += num4;
			}
			else
			{
				num2++;
				ideosPerRow.Add(1);
				num3 = num4;
			}
		}
		num2 = 0;
		num3 = 0f;
		int num5 = 0;
		foreach (IdeoPresetDef item2 in DefDatabase<IdeoPresetDef>.AllDefs.Where((IdeoPresetDef i) => i.categoryDef == category))
		{
			Rect rect2 = RectForIdeo(item2);
			float num6 = rect2.width + 10f;
			DrawIdeo(new Vector2(num3 + 300f + 17f, curY), item2);
			num3 += num6;
			num5++;
			if (num2 < ideosPerRow.Count && num5 >= ideosPerRow[num2])
			{
				num2++;
				num3 = 0f;
				num5 = 0;
				curY += rect2.height + 10f;
			}
		}
		curY += 8f;
		lastCategoryGroupLabel = category.groupLabel;
	}

	private void DrawSelectable(Rect rect, string label, string tooltip, TextAnchor textAnchor, bool active, bool tutorAllows, Action iconDrawer, Action onSelect)
	{
		TextAnchor anchor = Text.Anchor;
		Widgets.DrawOptionBackground(rect, active);
		Rect rect2 = rect.ContractedBy(2f);
		rect2.xMin += 4f;
		rect2.xMax -= 4f;
		Text.Anchor = textAnchor;
		Widgets.Label(rect2, label);
		Text.Anchor = anchor;
		iconDrawer?.Invoke();
		if (Mouse.IsOver(rect) && !tooltip.NullOrEmpty())
		{
			TooltipHandler.TipRegion(rect, new TipSignal(tooltip, 351941375, TooltipPriority.Ideo));
		}
		if (Widgets.ButtonInvisible(rect) && tutorAllows)
		{
			onSelect?.Invoke();
			SoundDefOf.Checkbox_TurnedOn.PlayOneShotOnCamera();
		}
	}

	private void DrawIdeo(Vector2 offset, IdeoPresetDef ideo)
	{
		Rect rect = RectForIdeo(ideo);
		rect.x += offset.x;
		rect.y += offset.y;
		DrawSelectable(rect, ideo.LabelCap, ideo.LabelCap.Colorize(ColoredText.TipSectionTitleColor) + "\n\n" + ideo.description, Text.Anchor, selectedIdeo == ideo, !TutorSystem.TutorialMode || TutorSystem.AllowAction("IdeoPresetSelectIdeo"), delegate
		{
			float num = 5f;
			float num2 = 55f * (float)((ideo.Icon != null) ? 1 : ideo.memes.Count) + 5f;
			float num3 = rect.width / 2f - num2 / 2f;
			if (ideo.Icon != null)
			{
				GUI.DrawTexture(new Rect(rect.x + num + num3, rect.y + Text.LineHeight + 5f, 50f, 50f), ideo.Icon);
				return;
			}
			foreach (MemeDef meme in ideo.memes)
			{
				Rect rect2 = new Rect(rect.x + num + num3, rect.y + Text.LineHeight + 5f, 50f, 50f);
				DrawMeme(rect2, meme);
				num += 55f;
			}
		}, delegate
		{
			presetSelection = PresetSelection.Preset;
			selectedIdeo = ideo;
		});
	}

	private Rect RectForIdeo(IdeoPresetDef ideo)
	{
		float a = Text.CalcSize(ideo.LabelCap).x + 5f + 2f;
		float b = (float)((ideo.Icon != null) ? 1 : ideo.memes.Count) * 55f;
		return new Rect
		{
			width = Mathf.Max(Mathf.Max(a, b) + 5f, 110f),
			height = Text.LineHeight + 50f + 10f
		};
	}

	private void DrawMeme(Rect rect, MemeDef meme)
	{
		Widgets.DrawLightHighlight(rect);
		if (Mouse.IsOver(rect))
		{
			Widgets.DrawHighlight(rect);
			TooltipHandler.TipRegion(rect, meme.LabelCap.Colorize(ColoredText.TipSectionTitleColor) + "\n\n" + meme.description);
		}
		GUI.DrawTexture(rect, meme.Icon);
	}

	private void DrawStructureAndStyleSelection(Rect rect)
	{
		int numStyles = selectedStyles.Count;
		bool num = numStyles < 3;
		float curX = 10f;
		if (num)
		{
			Rect rect2 = GetRect();
			GUI.DrawTexture(rect2.ContractedBy(4f), PlusTex);
			if (Mouse.IsOver(rect2))
			{
				Widgets.DrawHighlight(rect2);
				TooltipHandler.TipRegion(rect2, "AddStyleCategory".Translate() + "\n\n" + "StyleCategoryDescriptionAbstract".Translate().Resolve().Colorize(ColoredText.SubtleGrayColor));
			}
			if (Widgets.ButtonInvisible(rect2) && (!TutorSystem.TutorialMode || TutorSystem.AllowAction("IdeoPresetEditStyle")))
			{
				List<FloatMenuOption> opts = new List<FloatMenuOption>();
				FillAllAvailableStyles(ref opts);
				Find.WindowStack.Add(new FloatMenu(opts));
			}
		}
		for (int num2 = selectedStyles.Count - 1; num2 >= 0; num2--)
		{
			StyleCategoryDef styleCategoryDef = selectedStyles[num2];
			Rect rect3 = GetRect();
			GUI.DrawTexture(rect3.ContractedBy(2f), (styleCategoryDef != null) ? styleCategoryDef.Icon : RandomIcon);
			if (Mouse.IsOver(rect3))
			{
				Widgets.DrawHighlight(rect3);
				TooltipHandler.TipRegion(rect3, (styleCategoryDef != null) ? IdeoUIUtility.StyleTooltip(styleCategoryDef, IdeoEditMode.None, null, selectedStylesWithPriority) : "RandomStyleTip".Translate());
			}
			if (Widgets.ButtonInvisible(rect3) && (!TutorSystem.TutorialMode || TutorSystem.AllowAction("IdeoPresetEditStyle")))
			{
				List<FloatMenuOption> opts2 = new List<FloatMenuOption>();
				FillAllAvailableStyles(ref opts2, num2);
				Find.WindowStack.Add(new FloatMenu(opts2));
			}
		}
		curX += 4f;
		TaggedString taggedString = "Styles".Translate();
		float x = Text.CalcSize(taggedString).x;
		curX += x;
		Rect rect4 = new Rect(rect.xMax - curX, rect.yMin + 6f, x, Text.LineHeight);
		if (Mouse.IsOver(rect4))
		{
			Widgets.DrawHighlight(rect4.ExpandedBy(4f, 2f));
			TooltipHandler.TipRegion(rect4, "StyleCategoryDescriptionAbstract".Translate());
		}
		Widgets.Label(rect4, taggedString);
		curX += 17f;
		Rect rect5 = GetRect();
		GUI.DrawTexture(rect5.ContractedBy(2f), (selectedStructure != null) ? selectedStructure.Icon : RandomIcon);
		if (Mouse.IsOver(rect5))
		{
			Widgets.DrawHighlight(rect5);
			TooltipHandler.TipRegion(rect5, (selectedStructure != null) ? IdeoUIUtility.StructureTooltip(selectedStructure, IdeoEditMode.None) : "RandomStructureTip".Translate());
		}
		if (Widgets.ButtonInvisible(rect5) && (!TutorSystem.TutorialMode || TutorSystem.AllowAction("IdeoPresetEditStructure")))
		{
			List<FloatMenuOption> list = new List<FloatMenuOption>();
			if (selectedStructure != null)
			{
				list.Add(new FloatMenuOption("Random".Translate(), delegate
				{
					selectedStructure = null;
				}, RandomIcon, Color.white));
			}
			foreach (MemeDef meme in DefDatabase<MemeDef>.AllDefsListForReading)
			{
				if (selectedStructure != meme && meme.category == MemeCategory.Structure && IdeoUtility.IsMemeAllowedFor(meme, Find.Scenario.playerFaction.factionDef))
				{
					list.Add(new FloatMenuOption(meme.LabelCap, delegate
					{
						selectedStructure = meme;
					}, meme.Icon, Color.white));
				}
			}
			Find.WindowStack.Add(new FloatMenu(list));
		}
		curX += 4f;
		TaggedString taggedString2 = "Structure".Translate();
		float x2 = Text.CalcSize(taggedString2).x;
		curX += x2;
		Rect rect6 = new Rect(rect.xMax - curX, rect.yMin + 6f, x2, Text.LineHeight);
		if (Mouse.IsOver(rect6))
		{
			Widgets.DrawHighlight(rect6.ExpandedBy(4f, 2f));
			TooltipHandler.TipRegion(rect6, "StructureMemeTip".Translate());
		}
		Widgets.Label(rect6, taggedString2);
		void FillAllAvailableStyles(ref List<FloatMenuOption> reference, int forIndex = -1)
		{
			if (forIndex == -1 || selectedStyles[forIndex] != null)
			{
				reference.Add(new FloatMenuOption("Random".Translate(), delegate
				{
					if (forIndex == -1)
					{
						selectedStyles.Add(null);
					}
					else
					{
						selectedStyles[forIndex] = null;
					}
					RecacheStyleCategoriesWithPriority();
				}, RandomIcon, Color.white));
			}
			foreach (StyleCategoryDef s in DefDatabase<StyleCategoryDef>.AllDefs.Where((StyleCategoryDef styleCategoryDef2) => !styleCategoryDef2.fixedIdeoOnly && !selectedStyles.Contains(styleCategoryDef2)))
			{
				if (forIndex == -1 || selectedStyles[forIndex] != s)
				{
					reference.Add(new FloatMenuOption(s.LabelCap, delegate
					{
						if (forIndex == -1)
						{
							selectedStyles.Add(s);
						}
						else
						{
							selectedStyles[forIndex] = s;
						}
						RecacheStyleCategoriesWithPriority();
					}, s.Icon, Color.white));
				}
			}
			if (forIndex != -1 && numStyles > 1)
			{
				reference.Add(new FloatMenuOption("Remove".Translate(), delegate
				{
					selectedStyles.RemoveAt(forIndex);
					RecacheStyleCategoriesWithPriority();
				}));
			}
		}
		Rect GetRect()
		{
			Rect result = rect;
			result.xMax -= curX;
			result.xMin = result.xMax - 35f;
			result.height = 35f;
			curX += 35f;
			return result;
		}
	}

	private void DoClassic()
	{
		foreach (Faction allFaction in Find.FactionManager.AllFactions)
		{
			if (allFaction.ideos != null)
			{
				allFaction.ideos.RemoveAll();
				allFaction.ideos.SetPrimary(classicIdeo);
			}
		}
		Find.IdeoManager.RemoveUnusedStartingIdeos();
		Find.Scenario.PostIdeoChosen();
		if (Find.Storyteller.def.tutorialMode)
		{
			next.prev = prev;
		}
		else
		{
			next.prev = this;
		}
		base.DoNext();
	}

	private void DoCustomize(bool fluid = false)
	{
		if (TutorSystem.TutorialMode && !TutorSystem.AllowAction("IdeoPresetCustomizeIdeo"))
		{
			return;
		}
		foreach (Ideo item in Find.IdeoManager.IdeosListForReading)
		{
			item.initialPlayerIdeo = false;
		}
		Faction.OfPlayer.ideos.RemoveAll();
		Find.IdeoManager.RemoveUnusedStartingIdeos();
		Page_ConfigureIdeo page_ConfigureIdeo;
		if (fluid)
		{
			page_ConfigureIdeo = new Page_ConfigureFluidIdeo();
			page_ConfigureIdeo.SelectOrMakeNewIdeo();
			page_ConfigureIdeo.ideo.Fluid = true;
		}
		else
		{
			page_ConfigureIdeo = new Page_ConfigureIdeo();
		}
		page_ConfigureIdeo.prev = this;
		page_ConfigureIdeo.next = next;
		next.prev = page_ConfigureIdeo;
		ResetSelection();
		Find.WindowStack.Add(page_ConfigureIdeo);
		Close();
	}

	private void DoLoad()
	{
		Dialog_IdeoList_Load window = new Dialog_IdeoList_Load(delegate(Ideo ideo)
		{
			AssignIdeoToPlayer(ideo);
			Find.IdeoManager.RemoveUnusedStartingIdeos();
			Find.Scenario.PostIdeoChosen();
			next.prev = this;
			ResetSelection();
			base.DoNext();
		});
		Find.WindowStack.Add(window);
	}

	private void DoPreset()
	{
		List<MemeDef> list = selectedIdeo.memes.ToList();
		MemeDef result;
		if (selectedStructure != null)
		{
			list.RemoveAll((MemeDef x) => x.category == MemeCategory.Structure);
			list.Add(selectedStructure);
		}
		else if (!list.Any((MemeDef x) => x.category == MemeCategory.Structure) && DefDatabase<MemeDef>.AllDefsListForReading.Where((MemeDef m) => m.category == MemeCategory.Structure && IdeoUtility.IsMemeAllowedFor(m, Find.Scenario.playerFaction.factionDef)).TryRandomElement(out result))
		{
			list.Add(result);
		}
		Ideo ideo = IdeoGenerator.GenerateIdeo(new IdeoGenerationParms(Find.FactionManager.OfPlayer.def, forceNoExpansionIdeo: false, null, null, list, selectedIdeo.classicPlus, forceNoWeaponPreference: true));
		ApplySelectedStylesToIdeo(ideo);
		AssignIdeoToPlayer(ideo);
		Find.IdeoManager.RemoveUnusedStartingIdeos();
		Find.Scenario.PostIdeoChosen();
		base.DoNext();
	}

	protected override void DoNext()
	{
		Find.IdeoManager.classicMode = presetSelection == PresetSelection.Classic;
		switch (presetSelection)
		{
		case PresetSelection.Classic:
			DoClassic();
			break;
		case PresetSelection.CustomFixed:
			DoCustomize();
			break;
		case PresetSelection.CustomFluid:
			DoCustomize(fluid: true);
			break;
		case PresetSelection.Load:
			DoLoad();
			break;
		case PresetSelection.Preset:
			DoPreset();
			break;
		}
	}

	private void ApplySelectedStylesToIdeo(Ideo ideo)
	{
		if (selectedStyles.Count == 1 && selectedStyles[0] == null)
		{
			ideo.foundation.RandomizeStyles();
			return;
		}
		List<StyleCategoryDef> finalizedStyles = selectedStyles.ToList();
		for (int num = finalizedStyles.Count - 1; num >= 0; num--)
		{
			if (finalizedStyles[num] == null)
			{
				if (DefDatabase<StyleCategoryDef>.AllDefs.Where((StyleCategoryDef x) => !x.fixedIdeoOnly && !finalizedStyles.Contains(x)).TryRandomElement(out var result))
				{
					finalizedStyles[num] = result;
				}
				else
				{
					finalizedStyles.RemoveAt(num);
				}
			}
		}
		ideo.thingStyleCategories.Clear();
		for (int num2 = 0; num2 < finalizedStyles.Count; num2++)
		{
			StyleCategoryDef category = finalizedStyles[num2];
			ideo.thingStyleCategories.Add(new ThingStyleCategoryWithPriority(category, 3 - num2));
		}
		ideo.SortStyleCategories();
	}

	private void AssignIdeoToPlayer(Ideo ideo)
	{
		Faction.OfPlayer.ideos.SetPrimary(ideo);
		foreach (Ideo item in Find.IdeoManager.IdeosListForReading)
		{
			item.initialPlayerIdeo = false;
		}
		ideo.initialPlayerIdeo = true;
		Find.IdeoManager.Add(ideo);
	}

	private void RecacheStyleCategoriesWithPriority()
	{
		selectedStylesWithPriority.Clear();
		for (int i = 0; i < selectedStyles.Count; i++)
		{
			StyleCategoryDef styleCategoryDef = selectedStyles[i];
			if (styleCategoryDef != null)
			{
				selectedStylesWithPriority.Add(new ThingStyleCategoryWithPriority(styleCategoryDef, 3 - i));
			}
		}
	}

	private void ResetSelection()
	{
		selectedIdeo = null;
		selectedStructure = null;
		selectedStyles = new List<StyleCategoryDef> { null };
	}
}
