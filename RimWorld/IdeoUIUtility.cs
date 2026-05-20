using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimWorld;

[StaticConstructorOnStartup]
public static class IdeoUIUtility
{
	private struct StyleCatOverride
	{
		public string label;

		public StyleCategoryDef style;

		public StyleCatOverride(string label, StyleCategoryDef style)
		{
			this.label = label;
			this.style = style;
		}
	}

	private static bool showAll;

	public static bool devEditMode;

	public static Ideo selected;

	private static Ideo currentRitualAmbiencePreviewIdeo;

	private static Sustainer currentRitualAmbiencePreview;

	private const float IdeoIconRectSize = 30f;

	private const float IdeoIconRectGapX = 7f;

	private const float IdeoIconRectGapY = 7f;

	private const float RowMinHeight = 46f;

	private const int IconSize = 70;

	public const int CultureIconSize = 35;

	private const int StyleIconSize = 28;

	public const int ApparelRequirementIconSize = 24;

	public const int ApparelRequirementIconPad = 2;

	public const float IdeoListWidthPct = 0.25f;

	private const float IdeoDetailsPad = 17f;

	public const float CellPad = 6f;

	public const float ExtraSpaceBetweenRows = 4f;

	public const float TextTopPad = 2f;

	public static readonly Vector2 MemeBoxSize = new Vector2(122f, 120f);

	private const float MemeIconSize = 80f;

	private const int MemeIconMargin = 8;

	public static readonly Vector2 PreceptBoxSize = new Vector2(220f, 60f);

	public const float PreceptIconSize = 50f;

	public const int MaxPreceptsPerRow = 3;

	public const int GapBetweenBoxes = 8;

	private const int DescriptionTextBoxHeight = 70;

	private const int DescriptionTextBoxPadding = 40;

	public static readonly Vector2 ButtonSize = new Vector2(145f, 30f);

	private static readonly Vector2 AddPreceptButtonSize = new Vector2(175f, 30f);

	private static readonly Color TutorArrowColor = new Color(0.937f, 0.847f, 0f);

	private static readonly Texture2D ArrowTex = ContentFinder<Texture2D>.Get("UI/Overlays/TutorArrowRight");

	private static readonly Texture2D PlusTex = ContentFinder<Texture2D>.Get("UI/Buttons/Plus");

	public static readonly Texture2D UnlockedTex = ContentFinder<Texture2D>.Get("UI/Overlays/LockedMonochrome");

	public static readonly Texture2D LockedTex = ContentFinder<Texture2D>.Get("UI/Overlays/Locked");

	public static readonly Texture2D PreviewRitualAmbience = ContentFinder<Texture2D>.Get("UI/Buttons/PreviewSound_NotPlaying");

	public static readonly Texture2D PreviewRitualAmbienceOn = ContentFinder<Texture2D>.Get("UI/Buttons/PreviewSound_Playing");

	private static readonly Vector2 SaveLoadButtonSize = new Vector2(70f, 30f);

	private const int FluidIconSize = 25;

	private static Texture2D fluidIdeoIcon;

	private static List<Pawn> tmpPawns = new List<Pawn>();

	private static List<ThingStyleCategoryWithPriority> tmpStyleCategories = new List<ThingStyleCategoryWithPriority>();

	private static List<StyleCatOverride> tmpOverrides = new List<StyleCatOverride>();

	private static List<Precept_Ritual> fluidIdeoRituals = new List<Precept_Ritual>();

	private static List<HistoryEventDef> fluidIdeoQuestSuccessEvents = new List<HistoryEventDef>();

	private static List<Faction> tmpFactions = new List<Faction>();

	private static List<MemeDef> tmpMemesToShow = new List<MemeDef>();

	private static MemeDef tmpMouseOverMeme = null;

	private static List<PreceptDef> tmpRequiredPrecepts = new List<PreceptDef>();

	private static List<string> tmpSortedLabelCaps = new List<string>();

	private static List<Precept> tmpPrecepts = new List<Precept>();

	private static List<ThingDef> tmpUsedThingDefs = new List<ThingDef>();

	private static List<ThingDef> tmpAllowedThingDefs = new List<ThingDef>();

	private static List<ThingDef> tmpAllThingDefs = new List<ThingDef>();

	private static List<string> tempRequiredMemes = new List<string>();

	private static PreceptDef tmpMouseOverPrecept = null;

	private static List<RitualPatternDef> addedPatternDefsTmp = new List<RitualPatternDef>();

	private static readonly Dictionary<IssueDef, List<FloatMenuOption>> issueGroupsTmp = new Dictionary<IssueDef, List<FloatMenuOption>>();

	private static readonly Dictionary<IssueDef, PreceptDef> singlePreceptsTmp = new Dictionary<IssueDef, PreceptDef>();

	private static readonly Dictionary<ThingDef, List<FloatMenuOption>> thingGroupsTmp = new Dictionary<ThingDef, List<FloatMenuOption>>();

	public static Ideo FallbackSelectedIdeo => Faction.OfPlayerSilentFail?.ideos?.PrimaryIdeo ?? Find.IdeoManager.IdeosInViewOrder.FirstOrFallback();

	public static string ClickToEdit => "ClickToEdit".Translate().CapitalizeFirst().Colorize(ColorLibrary.Green);

	private static Texture2D InitialPlayerIdeoTex => ContentFinder<Texture2D>.Get("UI/Icons/InitialPlayerIdeo");

	public static bool DevEditMode
	{
		get
		{
			if (Prefs.DevMode)
			{
				return devEditMode;
			}
			return false;
		}
	}

	public static Texture2D FluidIdeoIcon
	{
		get
		{
			if (fluidIdeoIcon == null)
			{
				fluidIdeoIcon = ContentFinder<Texture2D>.Get("UI/Icons/FluidIdeo");
			}
			return fluidIdeoIcon;
		}
	}

	public static bool PlayerPrimaryIdeoNotShared
	{
		get
		{
			if (Faction.OfPlayer.ideos.PrimaryIdeo == null)
			{
				return false;
			}
			foreach (Faction allFaction in Find.FactionManager.AllFactions)
			{
				if (!allFaction.IsPlayer && allFaction.ideos != null && allFaction.ideos.Has(Faction.OfPlayer.ideos.PrimaryIdeo))
				{
					return false;
				}
			}
			return true;
		}
	}

	public static bool TutorAllowsInteraction(IdeoEditMode editMode)
	{
		if (editMode == IdeoEditMode.Dev)
		{
			return true;
		}
		return TutorSystem.AllowAction("ConfiguringIdeo");
	}

	public static void SetSelected(Ideo ideo)
	{
		selected = ideo;
	}

	public static void UnselectCurrent()
	{
		selected = null;
	}

	public static void DoIdeoIcon(Rect rect, Ideo ideo, bool doTooltip = true, Action extraAction = null)
	{
		if (ideo == null)
		{
			return;
		}
		if (Mouse.IsOver(rect))
		{
			Widgets.DrawHighlight(rect);
			if (doTooltip)
			{
				TooltipHandler.TipRegion(rect, ideo.name);
			}
		}
		ideo.DrawIcon(rect);
		if (extraAction != null && Widgets.ButtonInvisible(rect))
		{
			extraAction();
		}
	}

	public static void OpenIdeoInfo(Ideo ideo)
	{
		selected = ideo;
		if (Current.ProgramState == ProgramState.Playing && !Find.WindowStack.AnyWindowAbsorbingAllInput)
		{
			Find.MainTabsRoot.SetCurrentTab(MainButtonDefOf.Ideos);
		}
		else
		{
			Find.WindowStack.Add(new Dialog_IdeosDuringLanding());
		}
	}

	public static void DoIdeoListAndDetails(Rect fillRect, ref Vector2 scrollPosition_list, ref float scrollViewHeight_list, ref Vector2 scrollPosition_details, ref float scrollViewHeight_details, bool editMode = false, bool showCreateIdeoButton = false, List<Pawn> pawns = null, Ideo onlyEditIdeo = null, Action createCustomBtnActOverride = null, bool forArchonexusRestart = false, Func<Pawn, Ideo> pawnIdeoGetter = null, Action<Ideo> ideoLoadedFromFile = null, bool showLoadExistingIdeoBtn = false, bool allowLoad = true, Action createFluidBtnAct = null)
	{
		Text.Font = GameFont.Small;
		if (Prefs.DevMode)
		{
			Widgets.CheckboxLabeled(new Rect(fillRect.width - 310f, 0f, 150f, 24f), "DEV: Show all", ref showAll);
			Widgets.CheckboxLabeled(new Rect(fillRect.width - 155f, 0f, 150f, 24f), "DEV: Edit mode", ref devEditMode);
		}
		else
		{
			showAll = false;
		}
		Ideo mouseoverIdeo = null;
		Rect fillRect2 = new Rect(fillRect.x, fillRect.y, Mathf.FloorToInt(fillRect.width * 0.25f), fillRect.height);
		DoIdeoList(fillRect2, ref scrollPosition_list, ref scrollViewHeight_list, out mouseoverIdeo, showCreateIdeoButton, pawns, createCustomBtnActOverride, forArchonexusRestart, pawnIdeoGetter, showLoadExistingIdeoBtn, createFluidBtnAct);
		if (selected != null && !Find.IdeoManager.IdeosListForReading.Contains(selected))
		{
			selected = null;
		}
		Ideo ideo = selected ?? mouseoverIdeo ?? (showCreateIdeoButton ? null : FallbackSelectedIdeo);
		Rect rect = new Rect(fillRect2.xMax, fillRect.y, fillRect.width - fillRect2.width, fillRect.height);
		if (ideo != null)
		{
			Rect inRect = rect.ContractedBy(17f);
			inRect.yMax += 8f;
			bool editMode2 = editMode && (onlyEditIdeo == null || onlyEditIdeo == ideo);
			DoIdeoDetails(inRect, ideo, ref scrollPosition_details, ref scrollViewHeight_details, editMode2, ideoLoadedFromFile, allowLoad, allowSave: true, reform: false, forArchonexusRestart);
		}
		else if (showCreateIdeoButton)
		{
			DoInitialIdeoSelection(rect.ContractedBy(17f));
		}
	}

	public static void MakeLoadedIdeoPrimary(Ideo loadIdeo)
	{
		Faction.OfPlayer.ideos.SetPrimary(loadIdeo);
		Find.IdeoManager.RemoveUnusedStartingIdeos();
		SetSelected(loadIdeo);
		Find.WindowStack.WindowOfType<Page_ConfigureIdeo>()?.SelectOrMakeNewIdeo(loadIdeo);
	}

	private static void DoInitialIdeoSelection(Rect fillRect)
	{
		Text.Anchor = TextAnchor.MiddleCenter;
		Rect rect = new Rect(fillRect.center.x - ButtonSize.x, fillRect.center.y, ButtonSize.x * 2f, ButtonSize.y * 2f);
		if (GenFilePaths.AllCustomIdeoFiles.Any())
		{
			float num = ButtonSize.y + 2f;
			if (Widgets.ButtonText(new Rect(fillRect.center.x - ButtonSize.x, fillRect.center.y + num, ButtonSize.x * 2f, ButtonSize.y * 2f), "LoadExistingIdeoligion".Translate() + "..."))
			{
				Find.WindowStack.Add(new Dialog_IdeoList_Load(delegate(Ideo ideo)
				{
					Find.WindowStack.WindowOfType<Page_ConfigureIdeo>()?.SelectOrMakeNewIdeo(ideo);
				}));
			}
			rect.y -= num;
		}
		if (Widgets.ButtonText(rect, "CreateCustomIdeoligion".Translate() + "..."))
		{
			Find.WindowStack.WindowOfType<Page_ConfigureIdeo>()?.SelectOrMakeNewIdeo(selected);
		}
		Widgets.Label(new Rect(fillRect.x + 26f, rect.y - Text.LineHeight, fillRect.width - 52f, Text.LineHeight), "SelectAnIdeo".Translate());
		Text.Anchor = TextAnchor.UpperLeft;
	}

	public static void DoIdeoList(Rect fillRect, ref Vector2 scrollPosition, ref float scrollViewHeight, out Ideo mouseoverIdeo, bool showCreateNewButton, List<Pawn> pawns = null, Action createCustomBtnActOverride = null, bool forArchonexusRestart = false, Func<Pawn, Ideo> pawnIdeoGetter = null, bool showLoadExistingIdeoBtn = false, Action createFluidBtnAct = null)
	{
		mouseoverIdeo = null;
		Page_ConfigureIdeo gameStartConfig = Find.WindowStack.WindowOfType<Page_ConfigureIdeo>();
		bool flag = gameStartConfig is Page_ConfigureFluidIdeo;
		Widgets.BeginGroup(fillRect);
		Text.Font = GameFont.Small;
		GUI.color = Color.white;
		Rect outRect = fillRect.AtZero();
		outRect.yMin += 17f;
		Rect viewRect = new Rect(0f, 0f, fillRect.width - 16f, scrollViewHeight);
		Widgets.BeginScrollView(outRect, ref scrollPosition, viewRect);
		float curY = 0f;
		int num = 0;
		if (gameStartConfig != null)
		{
			DrawIdeoligionSectionHeader("CustomIdeoligionSectionHeader".Translate());
		}
		if (showCreateNewButton)
		{
			if (Widgets.ButtonText(new Rect(0f, curY + 5f, viewRect.width, 36f), (forArchonexusRestart ? "CreateNew" : "CreateCustom").Translate() + "..."))
			{
				if (createCustomBtnActOverride != null)
				{
					createCustomBtnActOverride();
				}
				else if (gameStartConfig != null)
				{
					gameStartConfig.SelectOrMakeNewIdeo();
					selected = gameStartConfig.ideo;
				}
			}
			curY += 46f;
			num++;
			if (forArchonexusRestart && createFluidBtnAct != null)
			{
				if (Widgets.ButtonText(new Rect(0f, curY, viewRect.width, 36f), "CreateFluid".Translate()))
				{
					createFluidBtnAct();
				}
				curY += 46f;
				num++;
			}
			if (showLoadExistingIdeoBtn && GenFilePaths.AllCustomIdeoFiles.Any())
			{
				if (Widgets.ButtonText(new Rect(0f, curY, viewRect.width, 36f), "LoadExisting".Translate() + "..."))
				{
					Find.WindowStack.Add(new Dialog_IdeoList_Load(delegate(Ideo ideo)
					{
						if (gameStartConfig != null)
						{
							gameStartConfig.SelectOrMakeNewIdeo(ideo);
						}
					}));
				}
				curY += 46f;
				num++;
			}
		}
		if (gameStartConfig != null && !PlayerPrimaryIdeoNotShared)
		{
			DrawIdeoligionSectionHeader("FactionIdeoligionSectionHeader".Translate());
		}
		foreach (Ideo item in Find.IdeoManager.IdeosInViewOrder)
		{
			if (pawns != null)
			{
				tmpPawns.Clear();
				for (int num2 = 0; num2 < pawns.Count; num2++)
				{
					if (((pawnIdeoGetter != null) ? pawnIdeoGetter(pawns[num2]) : pawns[num2].Ideo) == item)
					{
						tmpPawns.Add(pawns[num2]);
					}
				}
			}
			DrawIdeoRow(item, ref curY, viewRect, out var mouseover, num, tmpPawns);
			if (mouseover)
			{
				mouseoverIdeo = item;
			}
			num++;
			if (gameStartConfig == null || !PlayerPrimaryIdeoNotShared || item != Faction.OfPlayer.ideos.PrimaryIdeo)
			{
				continue;
			}
			if (!flag)
			{
				curY += 4f;
				if (Widgets.ButtonText(new Rect(0f, curY, viewRect.width, 36f), "DeleteCustom".Translate()))
				{
					Ideo removeCustomIdeo = Faction.OfPlayer.ideos.PrimaryIdeo;
					Faction.OfPlayer.ideos.SetPrimary(Find.IdeoManager.IdeosInViewOrder.First((Ideo newIdeo) => newIdeo != removeCustomIdeo));
					selected = Faction.OfPlayer.ideos.PrimaryIdeo;
					gameStartConfig?.SelectOrMakeNewIdeo(selected);
					Find.IdeoManager.Remove(removeCustomIdeo);
				}
				curY += 46f;
			}
			else
			{
				curY += 17f;
			}
			DrawIdeoligionSectionHeader("FactionIdeoligionSectionHeader".Translate());
		}
		if (Event.current.type == EventType.Layout)
		{
			scrollViewHeight = curY;
		}
		Widgets.EndScrollView();
		GUI.EndGroup();
		tmpPawns.Clear();
		void DrawIdeoligionSectionHeader(string header)
		{
			Widgets.Label(fillRect.x, ref curY, viewRect.width, header);
		}
	}

	public static void DoIdeoSaveLoad(ref float curY, float width, Ideo ideo, Action<Ideo> loadIdeo, bool allowLoad = true, bool allowSave = true, bool forArchonexusRestart = false)
	{
		float num = width / 2f;
		float num2 = Mathf.Max(width - (width - PreceptBoxSize.x * 3f - 16f) / 2f - SaveLoadButtonSize.x, num + 10f);
		curY += 4f;
		float num3 = num2;
		if (allowSave)
		{
			Rect rect = new Rect(num3, curY, SaveLoadButtonSize.x, SaveLoadButtonSize.y);
			num3 -= SaveLoadButtonSize.x + 4f;
			if (Widgets.ButtonText(rect, "Save".Translate()))
			{
				Find.WindowStack.Add(new Dialog_IdeoList_Save(ideo));
			}
		}
		if (allowLoad)
		{
			Rect rect2 = new Rect(num3, curY, SaveLoadButtonSize.x, SaveLoadButtonSize.y);
			num3 -= SaveLoadButtonSize.x;
			if (Widgets.ButtonText(rect2, "Load".Translate()))
			{
				if (forArchonexusRestart)
				{
					Find.WindowStack.Add(new Dialog_IdeoList_Load(loadIdeo));
				}
				else
				{
					Find.WindowStack.Add(new Dialog_IdeoList_Load(loadIdeo, Find.IdeoManager.GetFactionsWithIdeo(ideo, onlyPrimary: true, onlyNpcFactions: true), DevEditMode));
				}
			}
		}
		curY += SaveLoadButtonSize.y + 4f;
	}

	private static void DrawIdeoRow(Ideo ideo, ref float curY, Rect fillRect, out bool mouseover, int row, List<Pawn> pawns = null)
	{
		Rect rect = new Rect(44f, curY, 0f, 46f);
		rect.width = fillRect.width - rect.x;
		Rect rect2 = new Rect(7f, curY + 7f, 30f, 30f);
		float num = (pawns.NullOrEmpty() ? 0f : 32f);
		float height = 46f + num;
		Rect rect3 = new Rect(0f, curY, fillRect.width, height);
		if (row % 2 == 1)
		{
			Widgets.DrawLightHighlight(rect3);
		}
		if (ideo == selected)
		{
			Widgets.DrawHighlightSelected(rect3);
		}
		else
		{
			Widgets.DrawHighlightIfMouseover(rect3);
		}
		Page_ConfigureIdeo page_ConfigureIdeo = Find.WindowStack.WindowOfType<Page_ConfigureIdeo>();
		bool flag = page_ConfigureIdeo is Page_ConfigureFluidIdeo;
		Text.Font = GameFont.Small;
		DoIdeoIcon(rect2, ideo);
		Rect rect4 = rect;
		rect4.y += 3f;
		Widgets.Label(rect4, ideo.name.Truncate(rect4.width));
		float curY2 = rect.y + rect.height / 2f - 2f;
		DoFactionIcons(ideo, rect.x, ref curY2, rect.width, 18f, 18f, null, Find.WindowStack.WindowOfType<Page_ConfigureIdeo>() == null, page_ConfigureIdeo != null && !flag);
		curY += 46f;
		if (pawns != null && pawns.Count > 0)
		{
			curY2 = curY;
			DoPawnIcons(pawns, rect2.x, ref curY2, rect.width, 22f);
			curY += num;
		}
		mouseover = Mouse.IsOver(rect3);
		if (selected != ideo && Widgets.ButtonInvisible(rect3) && TutorSystem.AllowAction("ConfiguringIdeo"))
		{
			selected = ideo;
			SoundDefOf.DialogBoxAppear.PlayOneShotOnCamera();
			if (page_ConfigureIdeo != null && !flag)
			{
				page_ConfigureIdeo.SelectOrMakeNewIdeo(selected);
			}
		}
	}

	public static void DoIdeoDetails(Rect inRect, Ideo ideo, ref Vector2 scrollPosition, ref float viewHeight, bool editMode = false, Action<Ideo> ideoLoadedFromFile = null, bool allowLoad = true, bool allowSave = true, bool reform = false, bool forArchonexusRestart = false)
	{
		if (!ModLister.CheckIdeology("Ideoligion"))
		{
			return;
		}
		IdeoEditMode ideoEditMode = (DevEditMode ? IdeoEditMode.Dev : (editMode ? ((!reform) ? IdeoEditMode.GameStart : IdeoEditMode.Reform) : IdeoEditMode.None));
		if (ideo.createdFromNoExpansionGame && ideoEditMode == IdeoEditMode.None && !ideo.memes.Any())
		{
			Widgets.NoneLabelCenteredVertically(inRect);
			return;
		}
		Rect viewRect = new Rect(0f, 0f, inRect.width - 16f, viewHeight);
		Widgets.BeginScrollView(inRect, ref scrollPosition, viewRect);
		float curY = 0f;
		float num = 0f;
		if (allowLoad || allowSave)
		{
			DoIdeoSaveLoad(ref curY, viewRect.width, ideo, delegate(Ideo loadedIdeo)
			{
				if (ideoLoadedFromFile != null)
				{
					ideoLoadedFromFile(loadedIdeo);
				}
				else
				{
					foreach (Faction item in Find.IdeoManager.GetFactionsWithIdeo(ideo, onlyPrimary: true, onlyNpcFactions: true))
					{
						item.ideos.SetPrimary(loadedIdeo);
					}
					MakeLoadedIdeoPrimary(loadedIdeo);
				}
			}, allowLoad, allowSave, forArchonexusRestart);
		}
		num = curY;
		DoName(ref curY, viewRect.width, ideo, ideoEditMode);
		DoFactions(ref num, viewRect.width, ideo, ideoEditMode);
		if (ideo.Fluid)
		{
			DoFluidIdeo(ref curY, viewRect.width, ideo, ideoEditMode);
		}
		curY += 34f;
		curY = Mathf.Max(curY, num);
		bool flag = false;
		foreach (MemeDef meme in ideo.memes)
		{
			if (meme.category != MemeCategory.Structure)
			{
				flag = true;
				break;
			}
		}
		if (flag || editMode)
		{
			DoMemes(ref curY, viewRect.width, ideo, ideoEditMode);
		}
		DoDescription(ref curY, viewRect.width, ideo, ideoEditMode);
		if (ideo.foundation != null)
		{
			DoFoundationInfo(ref curY, viewRect.width, ideo, ideoEditMode);
		}
		tmpMouseOverPrecept = null;
		DoPrecepts(ref curY, viewRect.width, ideo, ideoEditMode);
		DoAppearanceItems(ideo, ideoEditMode, ref curY, viewRect.width);
		if (Prefs.DevMode)
		{
			DoDebugButtons(ref curY, viewRect.width, ideo);
		}
		if (Event.current.type == EventType.Layout)
		{
			viewHeight = curY + inRect.height / 2f;
		}
		Widgets.EndScrollView();
	}

	public static void DoName(ref float curY, float width, Ideo ideo, IdeoEditMode editMode)
	{
		float b = ((editMode != IdeoEditMode.None) ? 220f : 275f);
		Text.Font = GameFont.Medium;
		float x = Text.CalcSize(ideo.name).x;
		Text.Font = GameFont.Small;
		string text = ideo.adjective.CapitalizeFirst() + " / " + ideo.memberName.CapitalizeFirst();
		x = Mathf.Max(x, Text.CalcSize(text).x);
		x = Mathf.Min(x, b);
		float num = 87f + x;
		float x2 = (width - PreceptBoxSize.x * 3f - 16f) / 2f;
		float num2 = width - (width - PreceptBoxSize.x * 3f - 16f) / 2f;
		float num3 = (width - num) / 2f;
		MemeDef structureMeme = ideo.StructureMeme;
		Rect rect = new Rect(x2, curY, 35f, 35f);
		if (structureMeme != null)
		{
			GUI.DrawTexture(rect, structureMeme.Icon);
			if (Mouse.IsOver(rect))
			{
				Widgets.DrawHighlight(rect);
				TooltipHandler.TipRegion(rect, StructureTooltip(structureMeme, editMode));
			}
			if (editMode != IdeoEditMode.None && Widgets.ButtonInvisible(rect) && TutorAllowsInteraction(editMode))
			{
				Find.WindowStack.Add(new Dialog_ChooseMemes(ideo, MemeCategory.Structure));
			}
		}
		Rect rect2 = new Rect(rect.xMax + 4f, curY, 35f, 35f);
		if (ideo.culture != null)
		{
			GUI.color = ideo.culture.iconColor;
			GUI.DrawTexture(rect2, ideo.culture.Icon);
			GUI.color = Color.white;
			if (editMode != IdeoEditMode.None && Widgets.ButtonInvisible(rect2) && TutorAllowsInteraction(editMode))
			{
				List<FloatMenuOption> list = new List<FloatMenuOption>();
				foreach (CultureDef item in DefDatabase<CultureDef>.AllDefs.OrderBy((CultureDef c) => c.label))
				{
					CultureDef culture = item;
					string label = culture.LabelCap;
					list.Add(new FloatMenuOption(label, delegate
					{
						if (ideo.culture != culture)
						{
							ideo.culture = culture;
							ideo.foundation.RandomizeStyles();
							ideo.style.RecalculateAvailableStyleItems();
							if (ideo.foundation is IdeoFoundation_Deity ideoFoundation_Deity)
							{
								ideoFoundation_Deity.GenerateDeities();
							}
							ideo.RegenerateDescription(force: true);
						}
					}, culture.Icon, culture.iconColor));
				}
				Find.WindowStack.Add(new FloatMenu(list, "ChooseCulture".Translate()));
			}
		}
		if (ideo.culture != null && Mouse.IsOver(rect2))
		{
			Widgets.DrawHighlight(rect2);
			TaggedString taggedString = ("Culture".Translate() + ": " + ideo.culture.LabelCap).Colorize(ColoredText.TipSectionTitleColor) + "\n\n" + ideo.culture.description + "\n\n" + "CultureTip".Translate().Resolve().Colorize(ColoredText.SubtleGrayColor) + ((editMode != IdeoEditMode.None) ? ("\n\n" + ClickToEdit) : string.Empty);
			TooltipHandler.TipRegion(rect2, taggedString);
		}
		float curX = rect.xMin;
		float curY2 = rect.yMax + 10f;
		DoStyles(ref curY2, ref curX, width, ideo, editMode);
		_ = rect.yMax;
		if (Current.ProgramState == ProgramState.Entry)
		{
			if (currentRitualAmbiencePreviewIdeo != ideo || (currentRitualAmbiencePreview != null && currentRitualAmbiencePreview.Ended))
			{
				currentRitualAmbiencePreview = null;
			}
			if (ideo.SoundOngoingRitual != null)
			{
				curX += 10f;
				Rect rect3 = new Rect(curX, rect.yMax + 10f, 32f, 32f);
				TooltipHandler.TipRegion(rect3, () => "TipPreviewRitualAmbienceSound".Translate(), 45834593);
				if (Widgets.ButtonImage(rect3, (currentRitualAmbiencePreview != null) ? PreviewRitualAmbienceOn : PreviewRitualAmbience))
				{
					if (currentRitualAmbiencePreview == null)
					{
						StartPlay();
					}
					else
					{
						currentRitualAmbiencePreview = null;
					}
				}
				curX += 32f;
			}
			if (currentRitualAmbiencePreview != null)
			{
				if (currentRitualAmbiencePreview.def != ideo.SoundOngoingRitual)
				{
					StartPlay();
				}
				currentRitualAmbiencePreview.Maintain();
				Find.MusicManagerEntry.MaintainSilence();
			}
		}
		float num4 = curY;
		DoNameAndSymbol(ref curY, width, ideo, editMode);
		if (editMode != IdeoEditMode.None)
		{
			float x3 = Mathf.Max(num2 - AddPreceptButtonSize.x, num3 + num + 10f);
			Rect rect4 = new Rect(x3, num4, AddPreceptButtonSize.x, AddPreceptButtonSize.y);
			TooltipHandler.TipRegion(rect4, "RandomizeSymbolsTooltip".Translate());
			if (Widgets.ButtonText(rect4, "RandomizeSymbols".Translate()) && TutorAllowsInteraction(editMode))
			{
				ideo.MakeMemeberNamePluralDirty();
				ideo.foundation.RandomizePlace();
				ideo.foundation.GenerateTextSymbols();
				ideo.foundation.GenerateLeaderTitle();
				ideo.foundation.RandomizeIcon();
				ideo.RegenerateAllPreceptNames();
				ideo.RegenerateDescription(force: true);
				SoundDefOf.Tick_High.PlayOneShotOnCamera();
			}
			GUI.color = ColoredText.SubtleGrayColor;
			Text.Anchor = TextAnchor.UpperCenter;
			string text2 = "ClickAnyElementToEditIt".Translate();
			float height = Text.CalcHeight(text2, AddPreceptButtonSize.x);
			Widgets.Label(new Rect(x3, num4 + AddPreceptButtonSize.y + 4f, AddPreceptButtonSize.x, height), text2);
			Text.Anchor = TextAnchor.UpperLeft;
			GUI.color = Color.white;
		}
		void StartPlay()
		{
			SoundInfo info = SoundInfo.OnCamera(MaintenanceType.PerFrame);
			info.forcedPlayOnCamera = true;
			info.testPlay = true;
			currentRitualAmbiencePreview = ideo.SoundOngoingRitual.TrySpawnSustainer(info);
			currentRitualAmbiencePreviewIdeo = ideo;
		}
	}

	public static void DoNameAndSymbol(ref float curY, float width, Ideo ideo, IdeoEditMode editMode)
	{
		float b = ((editMode != IdeoEditMode.None) ? 220f : 275f);
		Text.Font = GameFont.Medium;
		float x = Text.CalcSize(ideo.name).x;
		Text.Font = GameFont.Small;
		string text = ideo.adjective.CapitalizeFirst() + " / " + ideo.memberName.CapitalizeFirst();
		x = Mathf.Max(x, Text.CalcSize(text).x);
		x = Mathf.Min(x, b);
		float num = 87f + x;
		_ = (width - PreceptBoxSize.x * 3f - 16f) / 2f;
		_ = (width - PreceptBoxSize.x * 3f - 16f) / 2f;
		float x2 = (width - num) / 2f;
		Rect rect = new Rect(x2, curY, 70f, 70f);
		GUI.color = ideo.Color;
		GUI.DrawTexture(rect, ideo.Icon);
		GUI.color = Color.white;
		if (editMode != IdeoEditMode.None && Widgets.ButtonInvisible(rect) && TutorAllowsInteraction(editMode))
		{
			Find.WindowStack.Add(new Dialog_ChooseIdeoSymbols(ideo));
		}
		float num2 = curY;
		Rect rect2 = new Rect(rect.xMax + 17f, num2 + 5f, 999f, 30f);
		Text.Font = GameFont.Medium;
		Widgets.Label(rect2, ideo.name.Truncate(x));
		Text.Font = GameFont.Small;
		num2 += 39f;
		GUI.color = new Color(0.65f, 0.65f, 0.65f);
		Widgets.Label(new Rect(rect.xMax + 17f, num2, x, 30f), text.Truncate(x));
		num2 += 18f;
		GUI.color = Color.white;
		Rect rect3 = new Rect(rect.xMin, rect.yMin, x + 70f + 17f + 4f, 70f);
		if (Mouse.IsOver(rect3))
		{
			Widgets.DrawHighlight(rect3);
			string text2 = ("Name".Translate() + ": ").Colorize(ColoredText.TipSectionTitleColor) + ideo.name + "\n" + ("Adjective".Translate() + ": ").Colorize(ColoredText.TipSectionTitleColor) + ideo.adjective.CapitalizeFirst() + "\n" + ("IdeoMembers".Translate() + ": ").Colorize(ColoredText.TipSectionTitleColor) + ideo.memberName.CapitalizeFirst() + "\n" + ("LeaderTitle".Translate() + ": ").Colorize(ColoredText.TipSectionTitleColor) + ideo.leaderTitleMale.CapitalizeFirst() + "\n" + ("WorshipRoom".Translate() + ": ").Colorize(ColoredText.TipSectionTitleColor) + ideo.WorshipRoomLabel.CapitalizeFirst() + "\n" + ((ideo.leaderTitleFemale != ideo.leaderTitleMale) ? (" (" + ideo.leaderTitleFemale.CapitalizeFirst() + ")") : "") + ((editMode != IdeoEditMode.None) ? ("\n" + ClickToEdit) : string.Empty);
			TooltipHandler.TipRegion(rect3, text2);
		}
		if (editMode != IdeoEditMode.None && Widgets.ButtonInvisible(rect3) && TutorAllowsInteraction(editMode))
		{
			Find.WindowStack.Add(new Dialog_ChooseIdeoSymbols(ideo));
		}
		curY += 70f;
		curY += 10f;
	}

	public static void DoStyles(ref float curY, ref float curX, float width, Ideo ideo, IdeoEditMode editMode, int styleIconSize = 28)
	{
		tmpStyleCategories.Clear();
		tmpStyleCategories.AddRange(ideo.thingStyleCategories);
		for (int i = 0; i < 3; i++)
		{
			Rect rect = new Rect(curX, curY, styleIconSize, styleIconSize);
			curX += styleIconSize;
			int index = i;
			if (i < tmpStyleCategories.Count)
			{
				GUI.DrawTexture(rect.ContractedBy(4f), tmpStyleCategories[i].category.Icon);
				if (Mouse.IsOver(rect))
				{
					Widgets.DrawHighlight(rect);
					TooltipHandler.TipRegion(rect, StyleTooltip(tmpStyleCategories[i].category, editMode, ideo, tmpStyleCategories));
				}
				if (editMode == IdeoEditMode.None || !Widgets.ButtonInvisible(rect) || !TutorAllowsInteraction(editMode))
				{
					continue;
				}
				List<FloatMenuOption> list = new List<FloatMenuOption>();
				list.Add(new FloatMenuOption("Remove".Translate(), delegate
				{
					ideo.thingStyleCategories.RemoveAt(index);
					ideo.SortStyleCategories();
					ideo.style.ResetStylesForThingDef();
				}));
				foreach (StyleCategoryDef s in DefDatabase<StyleCategoryDef>.AllDefs.Where((StyleCategoryDef x) => (!x.fixedIdeoOnly || editMode == IdeoEditMode.Dev) && !ideo.thingStyleCategories.Any((ThingStyleCategoryWithPriority y) => y.category == x)))
				{
					list.Add(new FloatMenuOption(s.LabelCap, delegate
					{
						ideo.thingStyleCategories.RemoveAt(index);
						ideo.thingStyleCategories.Insert(index, new ThingStyleCategoryWithPriority(s, 3 - index));
						ideo.SortStyleCategories();
						ideo.style.ResetStylesForThingDef();
					}, s.Icon, Color.white));
				}
				Find.WindowStack.Add(new FloatMenu(list));
			}
			else
			{
				if (editMode == IdeoEditMode.None)
				{
					continue;
				}
				GUI.DrawTexture(rect.ContractedBy(4f), PlusTex);
				if (Mouse.IsOver(rect))
				{
					Widgets.DrawHighlight(rect);
					TooltipHandler.TipRegion(rect, "AddStyleCategory".Translate() + "\n\n" + "StyleCategoryDescription".Translate(ideo.Named("IDEO")).Resolve().Colorize(ColoredText.SubtleGrayColor) + "\n\n" + ClickToEdit);
				}
				if (!Widgets.ButtonInvisible(rect) || !TutorAllowsInteraction(editMode))
				{
					break;
				}
				List<FloatMenuOption> list2 = new List<FloatMenuOption>();
				foreach (StyleCategoryDef s2 in DefDatabase<StyleCategoryDef>.AllDefs.Where((StyleCategoryDef x) => (!x.fixedIdeoOnly || editMode == IdeoEditMode.Dev) && !ideo.thingStyleCategories.Any((ThingStyleCategoryWithPriority y) => y.category == x)))
				{
					list2.Add(new FloatMenuOption(s2.LabelCap, delegate
					{
						ideo.thingStyleCategories.Add(new ThingStyleCategoryWithPriority(s2, 3 - index));
						ideo.SortStyleCategories();
						ideo.style.ResetStylesForThingDef();
					}, s2.Icon, Color.white));
				}
				if (list2.Any())
				{
					Find.WindowStack.Add(new FloatMenu(list2));
				}
				break;
			}
		}
	}

	public static TaggedString StructureTooltip(MemeDef structureMeme, IdeoEditMode editMode)
	{
		return ("StructureMeme".Translate() + ": " + structureMeme.LabelCap).Colorize(ColoredText.TipSectionTitleColor) + "\n\n" + structureMeme.description + "\n\n" + "StructureMemeTip".Translate().Resolve().Colorize(ColoredText.SubtleGrayColor) + ((editMode != IdeoEditMode.None) ? ("\n\n" + ClickToEdit) : string.Empty);
	}

	public static TaggedString StyleTooltip(StyleCategoryDef cat, IdeoEditMode editMode, Ideo ideo, List<ThingStyleCategoryWithPriority> selectedStyles, bool skipDominanceDesc = false)
	{
		string text = "\n\n" + ((ideo != null && !ideo.classicMode) ? "StyleCategoryDescStyleDominance".Translate(ideo.Named("IDEO")) : "StyleCategoryDescStyleDominanceAbstract".Translate()).Resolve();
		if (skipDominanceDesc)
		{
			text = string.Empty;
		}
		return ("StyleCategory".Translate() + ": " + cat.LabelCap).Colorize(ColoredText.TipSectionTitleColor) + text + "\n\n" + GetStyleCategoryDescription(cat, ideo, selectedStyles ?? tmpStyleCategories) + ((editMode != IdeoEditMode.None) ? ("\n\n" + ClickToEdit) : string.Empty) + "\n\n" + ((ideo != null) ? "StyleCategoryDescription".Translate(ideo.Named("IDEO")) : "StyleCategoryDescriptionAbstract".Translate()).Resolve().Colorize(ColoredText.SubtleGrayColor);
	}

	public static string GetStyleCategoryDescription(StyleCategoryDef cat, Ideo ideo, List<ThingStyleCategoryWithPriority> selectedStyles)
	{
		tmpOverrides.Clear();
		string desc = "";
		if (!cat.addDesignators.NullOrEmpty() || !cat.addDesignatorGroups.NullOrEmpty())
		{
			desc += ("IdeoMakesBuildingBuildable".Translate() + ": ").Colorize(ColoredText.TipSectionTitleColor);
			desc += SortedLabelCaps(cat.addDesignators, cat.addDesignatorGroups, "\n  - ");
		}
		bool showedDetails = false;
		for (int i = 0; i < cat.thingDefStyles.Count; i++)
		{
			ThingDef thingDef = cat.thingDefStyles[i].ThingDef;
			if (!thingDef.canGenerateDefaultDesignator && (cat.addDesignators.NullOrEmpty() || !cat.addDesignators.Contains(thingDef)))
			{
				continue;
			}
			bool flag = false;
			for (int j = 0; j < selectedStyles.Count && selectedStyles[j].category != cat; j++)
			{
				if (selectedStyles[j].category.thingDefStyles.Any((ThingDefStyle s) => s.ThingDef == thingDef))
				{
					tmpOverrides.Add(new StyleCatOverride(thingDef.LabelCap, selectedStyles[j].category));
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				if (!showedDetails)
				{
					AppendDetails();
				}
				desc = desc + "\n  - " + thingDef.LabelCap.Resolve();
			}
		}
		if (ideo != null)
		{
			SoundDef soundOngoingRitual = ideo.SoundOngoingRitual;
			if (cat.soundOngoingRitual != null)
			{
				if (soundOngoingRitual == cat.soundOngoingRitual)
				{
					if (!showedDetails)
					{
						AppendDetails();
					}
					desc = desc + "\n  - " + "RitualAmbienceSound".Translate().Resolve();
				}
				else
				{
					StyleCategoryDef style = null;
					foreach (ThingStyleCategoryWithPriority thingStyleCategory in ideo.thingStyleCategories)
					{
						if (thingStyleCategory.category.soundOngoingRitual == soundOngoingRitual)
						{
							style = thingStyleCategory.category;
							break;
						}
					}
					tmpOverrides.Add(new StyleCatOverride("RitualAmbienceSound".Translate(), style));
				}
			}
		}
		if (tmpOverrides.Count > 0)
		{
			tmpOverrides.SortBy((StyleCatOverride ot) => ot.style.index);
			int num = -1;
			foreach (StyleCatOverride tmpOverride in tmpOverrides)
			{
				if (num == -1 || num != tmpOverride.style.index)
				{
					if (!desc.NullOrEmpty())
					{
						desc += "\n\n";
					}
					desc += ("OverriddenByStyle".Translate(tmpOverride.style.LabelCap) + ": ").Colorize(ColoredText.SubtleGrayColor);
					num = tmpOverride.style.index;
				}
				desc += ("\n  - " + tmpOverride.label).Colorize(ColoredText.SubtleGrayColor);
			}
		}
		return desc;
		void AppendDetails()
		{
			if (!desc.NullOrEmpty())
			{
				desc += "\n\n";
			}
			desc += ("StyleCategoryDetails".Translate(cat.label).CapitalizeFirst() + ":").Colorize(ColoredText.TipSectionTitleColor);
			showedDetails = true;
		}
	}

	private static void DoFactions(ref float curY, float width, Ideo ideo, IdeoEditMode editMode)
	{
		if (Find.World != null && Find.WindowStack.WindowOfType<Page_ConfigureIdeo>() == null)
		{
			float startX = width - 150f;
			if (editMode == IdeoEditMode.Dev)
			{
				curY += 60f;
			}
			DoFactionIcons(ideo, startX, ref curY, 9999f, 30f, 20f, "IdeoligionOf".Translate() + ":");
		}
	}

	private static void DoFluidIdeo(ref float curY, float width, Ideo ideo, IdeoEditMode editMode)
	{
		string text = ideo.development.Points + " / " + ideo.development.NextReformationDevelopmentPoints;
		float x = Text.CalcSize(text).x;
		float x2 = (width - PreceptBoxSize.x * 3f - 16f) / 2f;
		Rect position = new Rect(x2, curY, 25f, 25f);
		GUI.DrawTexture(position, FluidIdeoIcon);
		Text.Anchor = TextAnchor.MiddleCenter;
		Rect rect = new Rect(position.xMax + 4f, curY, x, 25f);
		Widgets.Label(rect, text);
		bool canReformNow = ideo.development.CanReformNow;
		if (editMode == IdeoEditMode.Dev && !canReformNow && Widgets.ButtonText(new Rect(rect.xMax + 10f, rect.y, 25f, 25f), "+"))
		{
			ideo.development.points = ideo.development.NextReformationDevelopmentPoints;
		}
		Text.Anchor = TextAnchor.UpperLeft;
		Rect rect2 = new Rect(position.x, position.y, rect.xMax - position.x, 25f);
		if (Mouse.IsOver(rect2))
		{
			TaggedString taggedString = ("CurrentDevelopmentPoints".Translate() + ": ").Colorize(ColoredText.TipSectionTitleColor) + ideo.development.Points + "\n" + ("ReformDevelopmentPoints".Translate().Colorize(ColoredText.TipSectionTitleColor) + ": ").Colorize(ColoredText.TipSectionTitleColor) + ideo.development.NextReformationDevelopmentPoints + "\n\n" + "FluidIdeoTip".Translate() + "\n\n" + ("FluidIdeoTipGetPoints".Translate() + ": ").Colorize(ColoredText.TipSectionTitleColor) + "\n";
			taggedString += "\n- " + "FluidIdeoTipGetPoinsByConversion".Translate();
			fluidIdeoRituals.Clear();
			IdeoDevelopmentUtility.GetAllRitualsThatGiveDevelopmentPoints(ideo, fluidIdeoRituals);
			for (int i = 0; i < fluidIdeoRituals.Count; i++)
			{
				taggedString += "\n- " + fluidIdeoRituals[i].LabelCap + " (" + "Ritual".Translate() + ")";
			}
			fluidIdeoRituals.Clear();
			fluidIdeoQuestSuccessEvents.Clear();
			IdeoDevelopmentUtility.GetAllQuestSuccessEventsThatGiveDevelopmentPoints(ideo, fluidIdeoQuestSuccessEvents);
			for (int j = 0; j < fluidIdeoQuestSuccessEvents.Count; j++)
			{
				taggedString += "\n- " + fluidIdeoQuestSuccessEvents[j].LabelCap + " (" + "QuestLower".Translate() + ")";
			}
			fluidIdeoQuestSuccessEvents.Clear();
			TooltipHandler.TipRegion(rect2, taggedString);
			Widgets.DrawHighlight(rect2);
		}
		if (canReformNow && Widgets.ButtonText(new Rect(rect.xMax + 10f, curY, 100f, 25f), "ReformIdeo".Translate()))
		{
			Find.WindowStack.Add(new Dialog_ReformIdeo(ideo));
		}
		curY += 25f;
	}

	private static float DoFactionIcons(Ideo ideo, float startX, ref float curY, float width, float iconSize, float iconSizeMinor, string label = null, bool showInitialIcon = false, bool showSelectedAsPlayerIdeo = false)
	{
		tmpFactions.Clear();
		bool flag = showSelectedAsPlayerIdeo && selected != null;
		foreach (Faction item in Find.FactionManager.AllFactionsInViewOrder)
		{
			if ((!item.Hidden || showAll) && (!flag || !item.IsPlayer) && item.ideos != null && (item.ideos.IsPrimary(ideo) || item.ideos.IsMinor(ideo)))
			{
				tmpFactions.Add(item);
			}
		}
		if (flag && ideo == selected)
		{
			tmpFactions.Add(Faction.OfPlayer);
		}
		float num = startX;
		if (showInitialIcon && ideo.initialPlayerIdeo)
		{
			Rect rect = new Rect(num, curY, iconSize, iconSize);
			GUI.DrawTexture(rect, InitialPlayerIdeoTex);
			if (Mouse.IsOver(rect))
			{
				TooltipHandler.TipRegion(rect, "InitialPlayerIdeo".Translate());
				Widgets.DrawHighlight(rect);
			}
			num += iconSize + 4f;
		}
		if (tmpFactions.Any())
		{
			tmpFactions.SortByDescending((Faction x) => x.ideos.IsPrimary(ideo));
			if (!label.NullOrEmpty())
			{
				Widgets.Label(startX, ref curY, width, label);
			}
			for (int num2 = 0; num2 < tmpFactions.Count; num2++)
			{
				float num3 = (tmpFactions[num2].ideos.IsPrimary(ideo) ? iconSize : iconSizeMinor);
				if (num + num3 > width - startX)
				{
					num = startX;
					curY += num3 + 4f;
				}
				FactionUIUtility.DrawFactionIconWithTooltip(new Rect(num, curY, num3, num3), tmpFactions[num2]);
				num += num3 + 4f;
			}
			curY += iconSize + 17f;
			return num;
		}
		return startX;
	}

	private static void DoPawnIcons(List<Pawn> pawns, float startX, ref float curY, float width, float iconSize)
	{
		float num = startX;
		for (int i = 0; i < pawns.Count; i++)
		{
			Rect rect = new Rect(num, curY, iconSize, iconSize);
			Widgets.DrawHighlightIfMouseover(rect);
			Widgets.ThingIcon(rect, pawns[i]);
			TooltipHandler.TipRegion(rect, pawns[i].LabelCap);
			num += iconSize + 4f;
		}
	}

	public static void DoDescription(ref float curY, float width, Ideo ideo, IdeoEditMode editMode)
	{
		float num = (width - PreceptBoxSize.x * 3f - 16f) / 2f;
		if (editMode != IdeoEditMode.None)
		{
			Rect rect = new Rect(num, curY, Text.LineHeight, Text.LineHeight);
			if (Widgets.ButtonImage(rect, ideo.descriptionLocked ? LockedTex : UnlockedTex))
			{
				ideo.descriptionLocked = !ideo.descriptionLocked;
				if (ideo.descriptionLocked)
				{
					SoundDefOf.Checkbox_TurnedOn.PlayOneShotOnCamera();
				}
				else
				{
					SoundDefOf.Checkbox_TurnedOff.PlayOneShotOnCamera();
				}
			}
			GUI.color = Color.white;
			if (Mouse.IsOver(rect))
			{
				string text = "LockButtonDesc".Translate() + "\n\n" + (ideo.descriptionLocked ? "LockInOn" : "LockInOff").Translate("Narrative".Translate(), "NarrativeLower".Translate());
				TooltipHandler.TipRegion(rect, text);
			}
		}
		float yMin = curY;
		Widgets.Label(num + Text.LineHeight + 4f, ref curY, width, "CoreNarrative".Translate());
		float width2 = width - num - 80f;
		int num2 = (int)Mathf.Max(70f, Text.CalcHeight(ideo.description, width2));
		Rect rect2 = new Rect(num + 40f, curY, width2, num2);
		Widgets.Label(rect2, ideo.description);
		if (editMode != IdeoEditMode.None)
		{
			Rect rect3 = rect2;
			rect3.yMin = yMin;
			rect3.xMin = num + Text.LineHeight + 4f;
			if (Mouse.IsOver(rect3))
			{
				Widgets.DrawHighlight(rect3);
				TooltipHandler.TipRegion(rect2, string.Concat("CoreNarrativeDesc".Translate(), "\n\n", ClickToEdit));
			}
			if (Widgets.ButtonInvisible(rect3) && TutorAllowsInteraction(editMode))
			{
				Find.WindowStack.Add(new Dialog_EditIdeoDescription(ideo));
			}
		}
		curY += num2;
		curY += 17f;
	}

	public static void DoFoundationInfo(ref float curY, float width, Ideo ideo, IdeoEditMode editMode)
	{
		ideo.foundation.DoInfo(ref curY, width, editMode);
		curY += 17f;
	}

	private static void DoMemes(ref float curY, float width, Ideo ideo, IdeoEditMode editMode)
	{
		float num = (width - PreceptBoxSize.x * 3f - 16f) / 2f;
		float curY2 = curY;
		Widgets.Label(num, ref curY2, width, "Memes".Translate());
		tmpMemesToShow.Clear();
		for (int i = 0; i < ideo.memes.Count; i++)
		{
			if (ideo.memes[i].category != MemeCategory.Structure)
			{
				tmpMemesToShow.Add(ideo.memes[i]);
			}
		}
		float num2 = (float)tmpMemesToShow.Count * MemeBoxSize.x + (float)((tmpMemesToShow.Count - 1) * 8);
		float num3 = (width - num2) / 2f;
		if (editMode == IdeoEditMode.GameStart && tmpMemesToShow.Any())
		{
			DrawKnowledgeTip(ConceptDefOf.EditingMemes, curY + Text.LineHeight, num);
		}
		tmpMouseOverMeme = null;
		for (int j = 0; j < tmpMemesToShow.Count; j++)
		{
			Rect rect = new Rect(num3 + (float)j * MemeBoxSize.x + (float)(j * 8), curY, MemeBoxSize.x, MemeBoxSize.y);
			if (editMode == IdeoEditMode.GameStart)
			{
				UIHighlighter.HighlightOpportunity(rect, "MemeBox");
			}
			DoMeme(rect, tmpMemesToShow[j], ideo, editMode);
		}
		curY += MemeBoxSize.y;
		curY += 17f;
	}

	private static void DrawKnowledgeTip(ConceptDef conceptDef, float curY, float labelAlignOffset)
	{
		if (Find.World == null && TutorSystem.AdaptiveTrainingEnabled && !PlayerKnowledgeDatabase.IsComplete(conceptDef))
		{
			Rect rect = new Rect(new Rect(0f, curY + Text.LineHeight, labelAlignOffset / 2f + 26f, Text.LineHeight * 2f));
			GUI.color = TutorArrowColor;
			GUI.DrawTexture(new Rect(rect.xMax - 1f, rect.y, rect.height, rect.height), ArrowTex);
			GUI.color = Color.white;
			Widgets.DrawWindowBackgroundTutor(rect);
			Text.Anchor = TextAnchor.MiddleCenter;
			GUI.color = Color.white;
			Widgets.Label(rect.ContractedBy(2f), ClickToEdit);
			Text.Anchor = TextAnchor.UpperLeft;
		}
	}

	public static void DoMeme(Rect memeBox, MemeDef meme, Ideo ideo = null, IdeoEditMode editMode = IdeoEditMode.None, bool drawHighlight = true, Action selectedOverride = null)
	{
		if (!ModLister.CheckIdeology("Memes"))
		{
			return;
		}
		if (drawHighlight)
		{
			Widgets.DrawLightHighlight(memeBox);
			GUI.color = new Color(1f, 1f, 1f, 0.04f);
			Widgets.DrawBox(memeBox);
			GUI.color = Color.white;
		}
		if (Mouse.IsOver(memeBox))
		{
			Widgets.DrawHighlight(memeBox);
			TooltipHandler.TipRegion(memeBox, GetMemeTip(meme, ideo) + ((editMode != IdeoEditMode.None) ? ("\n\n" + ClickToEdit) : string.Empty));
			tmpMouseOverMeme = meme;
		}
		else if (tmpMouseOverPrecept != null && IsPreceptRelatedForUI(meme, tmpMouseOverPrecept))
		{
			Widgets.DrawHighlight(memeBox);
		}
		GUI.DrawTexture(new Rect(memeBox.x + (memeBox.width - 80f) / 2f, memeBox.y + 8f, 80f, 80f), meme.Icon);
		if (meme.impact > 0)
		{
			Rect rect = memeBox.RightPartPixels(18f).TopPartPixels(18f);
			rect.x -= 4f;
			rect.y += 4f;
			IdeoImpactUtility.DrawImpactIcon(rect, meme.impact);
		}
		Rect rect2 = new Rect(memeBox.x, memeBox.yMax - Text.LineHeight * 2f + 4f, memeBox.width, Text.LineHeight * 2f - 4f).ContractedBy(10f, 0f);
		Text.Anchor = TextAnchor.MiddleCenter;
		Widgets.Label(rect2, meme.LabelCap);
		GenUI.ResetLabelAlign();
		if (editMode != IdeoEditMode.None && Widgets.ButtonInvisible(memeBox) && TutorAllowsInteraction(editMode))
		{
			if (selectedOverride != null)
			{
				selectedOverride();
				return;
			}
			PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.EditingMemes, KnowledgeAmount.Total);
			Find.WindowStack.Add(new Dialog_ChooseMemes(ideo, meme.category));
		}
	}

	private static string GetMemeTip(MemeDef meme, Ideo ideo = null)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine(meme.LabelCap.Colorize(ColoredText.TipSectionTitleColor));
		if (meme.impact > 0)
		{
			stringBuilder.AppendLine(("IdeoImpact".Translate() + ": " + IdeoImpactUtility.MemeImpactLabel(meme.impact).CapitalizeFirst()).Colorize(ColoredText.ImpactColor));
		}
		stringBuilder.AppendLine();
		stringBuilder.AppendLine(meme.description);
		List<PreceptDef> allDefsListForReading = DefDatabase<PreceptDef>.AllDefsListForReading;
		if (!meme.requireOne.NullOrEmpty())
		{
			tmpRequiredPrecepts.Clear();
			for (int i = 0; i < meme.requireOne.Count; i++)
			{
				List<PreceptDef> list = meme.requireOne[i];
				if (list.Count == 1)
				{
					tmpRequiredPrecepts.Add(list[0]);
				}
			}
			if (tmpRequiredPrecepts.Count > 0)
			{
				stringBuilder.AppendInNewLine(("RequiredPrecepts".Translate() + ":").Colorize(ColoredText.TipSectionTitleColor));
				stringBuilder.AppendLine();
				for (int j = 0; j < tmpRequiredPrecepts.Count; j++)
				{
					stringBuilder.AppendLine("  - " + tmpRequiredPrecepts[j].issue.LabelCap + ": " + tmpRequiredPrecepts[j].LabelCap);
				}
				tmpRequiredPrecepts.Clear();
			}
			for (int k = 0; k < meme.requireOne.Count; k++)
			{
				List<PreceptDef> list2 = meme.requireOne[k];
				if (list2.Count > 1)
				{
					stringBuilder.AppendInNewLine(("RequiresOnePrecept".Translate() + ":").Colorize(ColoredText.TipSectionTitleColor));
					stringBuilder.AppendLine();
					for (int l = 0; l < list2.Count; l++)
					{
						stringBuilder.AppendLine("  - " + list2[l].issue.LabelCap + ": " + list2[l].LabelCap);
					}
				}
			}
		}
		if (meme.selectOneOrNone != null && meme.selectOneOrNone.preceptThingPairs.Count > 0)
		{
			stringBuilder.AppendInNewLine(("ChanceToHavePrecept".Translate() + ":").Colorize(ColoredText.TipSectionTitleColor));
			stringBuilder.AppendLine();
			List<PreceptThingPair> preceptThingPairs = meme.selectOneOrNone.preceptThingPairs;
			for (int m = 0; m < preceptThingPairs.Count; m++)
			{
				stringBuilder.AppendLine("  - " + preceptThingPairs[m].thing.LabelCap + ": " + preceptThingPairs[m].precept.LabelCap);
			}
		}
		if (!meme.requiredRituals.NullOrEmpty())
		{
			stringBuilder.AppendInNewLine(("RequiredRituals".Translate() + ":").Colorize(ColoredText.TipSectionTitleColor));
			stringBuilder.AppendLine();
			for (int n = 0; n < meme.requiredRituals.Count; n++)
			{
				stringBuilder.AppendLine("  - " + (meme.requiredRituals[n].pattern.shortDescOverride.CapitalizeFirst() ?? ((string)meme.requiredRituals[n].precept.LabelCap)));
			}
		}
		List<string> list3 = meme.UnlockedRoles(ideo);
		if (!list3.NullOrEmpty())
		{
			stringBuilder.AppendInNewLine(("MemeUnlocksRoles".Translate() + ": ").Colorize(ColoredText.TipSectionTitleColor));
			stringBuilder.AppendInNewLine(list3.ToLineList("  - "));
			stringBuilder.AppendLine();
		}
		List<string> list4 = meme.UnlockedRituals();
		if (!list4.NullOrEmpty())
		{
			stringBuilder.AppendInNewLine(("MemeUnlocksRituals".Translate() + ": ").Colorize(ColoredText.TipSectionTitleColor));
			stringBuilder.AppendInNewLine(list4.ToLineList("  - "));
			stringBuilder.AppendLine();
		}
		if (!meme.thingStyleCategories.NullOrEmpty())
		{
			stringBuilder.AppendInNewLine(("Styles".Translate() + ": ").Colorize(ColoredText.TipSectionTitleColor));
			stringBuilder.AppendLine();
			for (int num = 0; num < meme.thingStyleCategories.Count; num++)
			{
				stringBuilder.AppendLine("  - " + meme.thingStyleCategories[num].category.LabelCap);
			}
		}
		IEnumerable<RecipeDef> source = DefDatabase<RecipeDef>.AllDefsListForReading.Where((RecipeDef r) => r.memePrerequisitesAny != null && r.memePrerequisitesAny.Contains(meme));
		if (source.Any())
		{
			stringBuilder.AppendInNewLine(("UnlockedRecipes".Translate() + ":").CapitalizeFirst().Colorize(ColoredText.TipSectionTitleColor));
			List<ThingDef> defs = source.Select((RecipeDef r) => r.ProducedThingDef).Distinct().ToList();
			stringBuilder.AppendLine(SortedLabelCaps(defs, "\n  - "));
		}
		if (!meme.addDesignators.NullOrEmpty() || !meme.addDesignatorGroups.NullOrEmpty())
		{
			stringBuilder.AppendInNewLine(("IdeoMakesBuildingBuildable".Translate() + ": ").Colorize(ColoredText.TipSectionTitleColor));
			stringBuilder.AppendLine(SortedLabelCaps(meme.addDesignators, meme.addDesignatorGroups, "\n  - "));
		}
		if (meme.startingResearchProjects.Any())
		{
			stringBuilder.AppendInNewLine(("IdeoStartWithResearch".Translate() + ": ").Colorize(ColoredText.TipSectionTitleColor));
			stringBuilder.AppendLine(SortedLabelCaps(meme.startingResearchProjects, "\n  - "));
		}
		if (!meme.agreeableTraits.NullOrEmpty())
		{
			stringBuilder.AppendInNewLine(("AgreeableTraits".Translate() + ":").Colorize(ColoredText.TipSectionTitleColor));
			stringBuilder.AppendInNewLine(meme.agreeableTraits.Select((TraitRequirement x) => (!x.degree.HasValue) ? x.def.degreeDatas.First().label : x.def.DataAtDegree(x.degree.Value).label).ToLineList("  - ", capitalizeItems: true));
			stringBuilder.AppendLine();
		}
		if (!meme.disagreeableTraits.NullOrEmpty())
		{
			stringBuilder.AppendInNewLine(("DisagreeableTraits".Translate() + ":").Colorize(ColoredText.TipSectionTitleColor));
			stringBuilder.AppendInNewLine(meme.disagreeableTraits.Select((TraitRequirement x) => (!x.degree.HasValue) ? x.def.degreeDatas.First().label : x.def.DataAtDegree(x.degree.Value).label).ToLineList("  - ", capitalizeItems: true));
			stringBuilder.AppendLine();
		}
		int num2 = 0;
		for (int num3 = 0; num3 < allDefsListForReading.Count; num3++)
		{
			if (allDefsListForReading[num3].conflictingMemes.Contains(meme))
			{
				num2++;
			}
		}
		if (num2 > 0)
		{
			stringBuilder.AppendInNewLine(("PreventsPrecepts".Translate() + ":").Colorize(ColoredText.TipSectionTitleColor));
			stringBuilder.AppendLine();
			for (int num4 = 0; num4 < allDefsListForReading.Count; num4++)
			{
				if (allDefsListForReading[num4].conflictingMemes.Contains(meme))
				{
					stringBuilder.AppendLine("  - " + allDefsListForReading[num4].issue.LabelCap + ": " + allDefsListForReading[num4].LabelCap);
				}
			}
		}
		return stringBuilder.ToString().TrimEndNewlines();
	}

	private static string SortedLabelCaps<T>(List<T> defs, string prefix) where T : Def
	{
		return SortedLabelCaps<T, Def>(defs, null, prefix);
	}

	private static string SortedLabelCaps<T1, T2>(List<T1> defs1, List<T2> defs2, string prefix) where T1 : Def where T2 : Def
	{
		tmpSortedLabelCaps.Clear();
		if (!defs1.NullOrEmpty())
		{
			foreach (T1 item in defs1)
			{
				if (item != null)
				{
					tmpSortedLabelCaps.Add(item.LabelCap);
				}
			}
		}
		if (!defs2.NullOrEmpty())
		{
			foreach (T2 item2 in defs2)
			{
				if (item2 != null)
				{
					tmpSortedLabelCaps.Add(item2.LabelCap);
				}
			}
		}
		if (tmpSortedLabelCaps.NullOrEmpty())
		{
			return "";
		}
		tmpSortedLabelCaps.Sort();
		StringBuilder stringBuilder = new StringBuilder();
		foreach (string tmpSortedLabelCap in tmpSortedLabelCaps)
		{
			stringBuilder.Append(prefix).Append(tmpSortedLabelCap);
		}
		return stringBuilder.ToString();
	}

	public static void DoPrecepts(ref float curY, float width, Ideo ideo, IdeoEditMode editMode)
	{
		DoPreceptsInt("Precepts".Translate(), "Precept".Translate(), mainPrecepts: true, ideo, editMode, ref curY, width, (PreceptDef p) => p.preceptClass == typeof(Precept), group: true);
		DoPreceptsInt("IdeoRoles".Translate(), "Role".Translate(), mainPrecepts: false, ideo, editMode, ref curY, width, (PreceptDef p) => typeof(Precept_Role).IsAssignableFrom(p.preceptClass));
		DoPreceptsInt("Rituals".Translate(), "Ritual".Translate(), mainPrecepts: false, ideo, editMode, ref curY, width, (PreceptDef p) => p.preceptClass == typeof(Precept_Ritual));
		DoPreceptsInt("IdeoBuildings".Translate(), "IdeoBuilding".Translate(), mainPrecepts: false, ideo, editMode, ref curY, width, (PreceptDef p) => p.preceptClass == typeof(Precept_Building) || p.preceptClass == typeof(Precept_RitualSeat));
		DoPreceptsInt("IdeoRelics".Translate(), "IdeoRelic".Translate(), mainPrecepts: false, ideo, editMode, ref curY, width, (PreceptDef p) => p.preceptClass == typeof(Precept_Relic));
		DoPreceptsInt("IdeoWeapons".Translate(), "IdeoWeapon".Translate(), mainPrecepts: false, ideo, editMode, ref curY, width, (PreceptDef p) => p.preceptClass == typeof(Precept_Weapon));
		DoPreceptsInt("VeneratedAnimals".Translate(), "Animal".Translate(), mainPrecepts: false, ideo, editMode, ref curY, width, (PreceptDef p) => p.preceptClass == typeof(Precept_Animal));
		if (ModsConfig.BiotechActive)
		{
			DoPreceptsInt("PreferredXenotypes".Translate(), "Xenotype".Translate().ToString().UncapitalizeFirst(), mainPrecepts: false, ideo, editMode, ref curY, width, (PreceptDef p) => p.preceptClass == typeof(Precept_Xenotype));
		}
		DoPreceptsInt("IdeoApparel".Translate(), "IdeoApparelDesire".Translate(), mainPrecepts: false, ideo, editMode, ref curY, width, (PreceptDef p) => p.preceptClass == typeof(Precept_Apparel));
	}

	private static void DoPreceptsInt(string categoryLabel, string addPreceptLabel, bool mainPrecepts, Ideo ideo, IdeoEditMode editMode, ref float curY, float width, Func<PreceptDef, bool> filter, bool group = false)
	{
		tmpPrecepts.Clear();
		List<Precept> preceptsListForReading = ideo.PreceptsListForReading;
		for (int i = 0; i < preceptsListForReading.Count; i++)
		{
			if ((showAll || preceptsListForReading[i].def.visible) && filter(preceptsListForReading[i].def))
			{
				tmpPrecepts.Add(preceptsListForReading[i]);
			}
		}
		if (!mainPrecepts && preceptsListForReading.Count == 0)
		{
			return;
		}
		tmpUsedThingDefs.Clear();
		foreach (Precept item in ideo.PreceptsListForReading)
		{
			if (item is Precept_ThingDef precept_ThingDef)
			{
				tmpUsedThingDefs.Add(precept_ThingDef.ThingDef);
			}
		}
		curY += 4f;
		float num = (width - PreceptBoxSize.x * 3f - 16f) / 2f;
		Widgets.Label(num, ref curY, width, categoryLabel);
		if (editMode == IdeoEditMode.GameStart && mainPrecepts && tmpPrecepts.Any())
		{
			DrawKnowledgeTip(ConceptDefOf.EditingPrecepts, curY, num);
		}
		if (editMode != IdeoEditMode.None)
		{
			float num2 = width - (width - PreceptBoxSize.x * 3f - 16f) / 2f;
			Rect rect = new Rect(num2 - AddPreceptButtonSize.x, curY - AddPreceptButtonSize.y, AddPreceptButtonSize.x, AddPreceptButtonSize.y);
			if (Widgets.ButtonText(rect, "AddPrecept".Translate(addPreceptLabel).CapitalizeFirst() + "...") && TutorAllowsInteraction(editMode))
			{
				AddPrecept(ideo, editMode, filter, group);
			}
			if (mainPrecepts)
			{
				Rect rect2 = rect;
				rect2.x = rect.xMin - rect.width - 10f;
				if (Widgets.ButtonText(rect2, "RandomizePrecepts".Translate()) && TutorAllowsInteraction(editMode))
				{
					if (ideo.anyPreceptEdited)
					{
						Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation("ChangesRandomizePrecepts".Translate("RandomizePrecepts".Translate()), RandomizePrecepts));
					}
					else
					{
						RandomizePrecepts();
					}
				}
			}
		}
		curY += 4f;
		PreceptImpact preceptImpact = (tmpPrecepts.Any() ? tmpPrecepts[0].def.impact : PreceptImpact.Low);
		float num3 = (width - 3f * PreceptBoxSize.x - 16f) / 2f;
		int num4 = 0;
		int num5 = 0;
		if (!tmpPrecepts.Any())
		{
			GUI.color = Color.grey;
			Widgets.Label(new Rect(num + 36f, curY + 10f, 999f, Text.LineHeight), "(" + "NoneLower".Translate() + ")");
			GUI.color = Color.white;
		}
		for (int j = 0; j < tmpPrecepts.Count; j++)
		{
			if (preceptImpact != tmpPrecepts[j].def.impact)
			{
				preceptImpact = tmpPrecepts[j].def.impact;
				num5 = 0;
				num4++;
			}
			else if (num5 >= 2)
			{
				num5 = 0;
				num4++;
			}
			else if (j > 0)
			{
				num5++;
			}
			Rect rect3 = new Rect(num3 + (float)num5 * PreceptBoxSize.x + (float)(num5 * 8), curY + (float)num4 * PreceptBoxSize.y + (float)(num4 * 8), PreceptBoxSize.x, PreceptBoxSize.y);
			if (mainPrecepts && editMode == IdeoEditMode.GameStart)
			{
				UIHighlighter.HighlightOpportunity(rect3, "PreceptBox");
			}
			tmpPrecepts[j].DrawPreceptBox(rect3, editMode, tmpMouseOverMeme != null && IsPreceptRelatedForUI(tmpMouseOverMeme, tmpPrecepts[j].def));
			if (Mouse.IsOver(rect3))
			{
				tmpMouseOverPrecept = tmpPrecepts[j].def;
			}
			GUI.color = Color.white;
		}
		curY += (float)(num4 + 1) * PreceptBoxSize.y + (float)(num4 * 8);
		curY += 17f;
		tmpPrecepts.Clear();
		void RandomizePrecepts()
		{
			ideo.foundation.RandomizePrecepts(init: true, new IdeoGenerationParms(FactionForRandomization(ideo)));
			ideo.RegenerateDescription();
			SoundDefOf.Tick_High.PlayOneShotOnCamera();
			ideo.anyPreceptEdited = false;
		}
	}

	private static void RegisterPreceptGroup(Ideo ideo, PreceptDef def, IdeoEditMode mode, ThingDef thing = null)
	{
		if (def.issue != null)
		{
			PreceptDef value2;
			if (issueGroupsTmp.TryGetValue(def.issue, out var value))
			{
				AddPreceptOption(ideo, def, mode, value);
			}
			else if (singlePreceptsTmp.Remove(def.issue, out value2))
			{
				List<FloatMenuOption> list = (issueGroupsTmp[def.issue] = new List<FloatMenuOption>());
				value = list;
				AddPreceptOption(ideo, value2, mode, value);
				AddPreceptOption(ideo, def, mode, value);
			}
			else
			{
				singlePreceptsTmp[def.issue] = def;
			}
		}
	}

	private static void PushPreceptGroups(Ideo ideo, List<FloatMenuOption> options, IdeoEditMode mode)
	{
		IssueDef key;
		foreach (KeyValuePair<IssueDef, List<FloatMenuOption>> item in issueGroupsTmp)
		{
			item.Deconstruct(out key, out var value);
			IssueDef issue = key;
			List<FloatMenuOption> list = value;
			if (!list.Empty())
			{
				options.Add(new FloatMenuOption(issue.LabelCap, delegate
				{
					Find.WindowStack.Add(new FloatMenu(list, issue.LabelCap));
				}, issue.Icon ?? ideo.Icon, Color.white));
			}
		}
		foreach (KeyValuePair<IssueDef, PreceptDef> item2 in singlePreceptsTmp)
		{
			item2.Deconstruct(out key, out var value2);
			PreceptDef def = value2;
			AddPreceptOption(ideo, def, mode, options);
		}
		singlePreceptsTmp.Clear();
		issueGroupsTmp.Clear();
	}

	private static void AddPrecept(Ideo ideo, IdeoEditMode editMode, Func<PreceptDef, bool> filter, bool group)
	{
		List<FloatMenuOption> opts = new List<FloatMenuOption>();
		List<PreceptDef> list = DefDatabase<PreceptDef>.AllDefs.Where(filter).ToList();
		bool flag = list.Any((PreceptDef preceptDef) => preceptDef.preceptClass == typeof(Precept_Ritual) && (showAll || preceptDef.visible) && (bool)CanListPrecept(ideo, preceptDef, editMode));
		int num = ideo.PreceptsListForReading.Count((Precept precept) => precept is Precept_Ritual && (showAll || precept.def.visible));
		addedPatternDefsTmp.Clear();
		thingGroupsTmp.Clear();
		AcceptanceReport acceptance;
		foreach (PreceptDef p in list)
		{
			acceptance = CanListPrecept(ideo, p, editMode);
			if (!acceptance && string.IsNullOrWhiteSpace(acceptance.Reason))
			{
				continue;
			}
			int preceptCountOfDef = ideo.GetPreceptCountOfDef(p);
			int num2 = p.maxCount;
			if (p.preceptInstanceCountCurve != null)
			{
				num2 = Mathf.Max(num2, Mathf.RoundToInt(p.preceptInstanceCountCurve.Last().y));
			}
			if (preceptCountOfDef < num2 || p.ignoreLimitsInEditMode)
			{
				if (!p.useChoicesFromBuildingDefs || p.Worker.ThingDefsForIdeo(ideo, null).EnumerableNullOrEmpty())
				{
					if (p.preceptClass == typeof(Precept_Weapon))
					{
						AddWeaponPreceptOption(p);
					}
					else if (p.preceptClass == typeof(Precept_Xenotype))
					{
						AddXenotypePreceptOption(p);
					}
					else if (group)
					{
						RegisterPreceptGroup(ideo, p, editMode);
					}
					else
					{
						AddPreceptOption(ideo, p, editMode, opts);
					}
				}
				else
				{
					tmpAllowedThingDefs.Clear();
					tmpAllThingDefs.Clear();
					tmpAllowedThingDefs.AddRange(from td in p.Worker.ThingDefsForIdeo(ideo, null)
						select td.def);
					tmpAllThingDefs.AddRange(from bd in (from bDef in p.Worker.ThingDefs
							orderby p.Worker.GetThingOrder(bDef), bDef.chance descending
							select bDef).ThenBy((Func<PreceptThingChance, string>)((PreceptThingChance bDef) => bDef.def.LabelCap))
						select bd.def);
					if (p.preceptClass == typeof(Precept_Building))
					{
						foreach (MemeDef meme in ideo.memes)
						{
							if (meme.consumableBuildings.NullOrEmpty())
							{
								continue;
							}
							foreach (ThingDef consumableBuilding in meme.consumableBuildings)
							{
								if (!tmpAllowedThingDefs.Contains(consumableBuilding))
								{
									tmpAllowedThingDefs.Add(consumableBuilding);
								}
								if (!tmpAllThingDefs.Contains(consumableBuilding))
								{
									tmpAllThingDefs.Add(consumableBuilding);
								}
							}
						}
					}
					foreach (ThingDef b in tmpAllThingDefs)
					{
						TaggedString labelCap = b.LabelCap;
						if (p.preceptClass == typeof(Precept_Apparel))
						{
							labelCap += ": " + p.LabelCap;
						}
						FloatMenuOption floatMenuOption = null;
						if ((!p.canUseAlreadyUsedThingDef && tmpUsedThingDefs.Contains(b)) || p.Worker.ShouldSkipThing(ideo, b) || !tmpAllowedThingDefs.Contains(b))
						{
							if (!p.canUseAlreadyUsedThingDef && tmpUsedThingDefs.Contains(b))
							{
								floatMenuOption = null;
							}
							else
							{
								AcceptanceReport acceptanceReport = p.Worker.CanUse(b, ideo, null);
								if (!acceptanceReport)
								{
									floatMenuOption = new FloatMenuOption(string.IsNullOrWhiteSpace(acceptanceReport.Reason) ? labelCap : (labelCap + " (" + acceptanceReport.Reason + ")"), null, b);
									floatMenuOption.thingStyle = ideo.GetStyleFor(b);
								}
							}
						}
						else
						{
							floatMenuOption = new FloatMenuOption(labelCap, delegate
							{
								Precept precept = PreceptMaker.MakePrecept(p);
								if (precept is Precept_Apparel precept_Apparel)
								{
									precept_Apparel.apparelDef = b;
								}
								ideo.AddPrecept(precept, init: true);
								ideo.anyPreceptEdited = true;
								if (precept is Precept_ThingDef precept_ThingDef)
								{
									precept_ThingDef.ThingDef = b;
									precept_ThingDef.RegenerateName();
								}
							}, b);
							floatMenuOption.thingStyle = ideo.GetStyleFor(b);
							if (p.preceptClass == typeof(Precept_Apparel))
							{
								floatMenuOption.forceThingColor = ideo.ApparelColor;
							}
						}
						if (floatMenuOption != null)
						{
							if (group)
							{
								GetOptionList(b).Add(PostProcessOption(floatMenuOption));
							}
							else
							{
								opts.Add(PostProcessOption(floatMenuOption));
							}
						}
					}
				}
				string groupTag = p.ritualPatternBase?.patternGroupTag;
				if (groupTag.NullOrEmpty())
				{
					continue;
				}
				foreach (RitualPatternDef item in DefDatabase<RitualPatternDef>.AllDefs.Where((RitualPatternDef d) => d.patternGroupTag == groupTag && d != p.ritualPatternBase))
				{
					AddPreceptOption(ideo, p, editMode, opts, item);
				}
			}
			else if (p.preceptClass == typeof(Precept_Relic))
			{
				opts.Add(new FloatMenuOption("MaxRelicCount".Translate(num2), null));
			}
			else if (preceptCountOfDef >= num2 && p.issue.allowMultiplePrecepts)
			{
				opts.Add(new FloatMenuOption(p.LabelCap + " (" + "MaxPreceptCount".Translate(num2) + ")", null, p.Icon ?? ideo.Icon, ideo.Color));
			}
			FloatMenuOption PostProcessOption(FloatMenuOption option)
			{
				if (!acceptance)
				{
					option.action = null;
					option.Label = option.Label + " (" + acceptance.Reason + ")";
				}
				return option;
			}
		}
		PushPreceptGroups(ideo, opts, editMode);
		opts = (from x in opts
			orderby x.Label.EndsWith("...") ? 1 : 0, x.Label
			select x).ToList();
		if (num < 6)
		{
			foreach (MemeDef meme2 in ideo.memes)
			{
				if (meme2.replacementPatterns.NullOrEmpty())
				{
					continue;
				}
				foreach (PreceptDef item2 in DefDatabase<PreceptDef>.AllDefs.Where(filter))
				{
					if (item2.ritualPatternBase == null || item2.ritualPatternBase.tags.NullOrEmpty() || !meme2.replaceRitualsWithTags.Any(item2.ritualPatternBase.tags.Contains) || item2.classicModeOnly)
					{
						continue;
					}
					foreach (RitualPatternDef replacementPattern in meme2.replacementPatterns)
					{
						if (CanAddRitualPattern(ideo, replacementPattern, editMode))
						{
							AddPreceptOption(ideo, item2, editMode, opts, replacementPattern);
						}
					}
				}
			}
		}
		else if (flag)
		{
			opts.Clear();
			opts.Add(new FloatMenuOption("MaxRitualCount".Translate(6), null));
		}
		if (!opts.Any())
		{
			opts.Add(new FloatMenuOption("NoChoicesAvailable".Translate(), null));
		}
		thingGroupsTmp.Clear();
		Find.WindowStack.Add(new FloatMenu(opts));
		void AddWeaponPreceptOption(PreceptDef pr)
		{
			foreach (WeaponClassPairDef allDef in DefDatabase<WeaponClassPairDef>.AllDefs)
			{
				WeaponClassPairDef w = allDef;
				if (!ideo.PreceptsListForReading.Any(ValidWeaponPrecept))
				{
					opts.Add(PostProcessOption(new FloatMenuOption(w.first.LabelCap + " / " + w.second.LabelCap, delegate
					{
						Precept_Weapon precept_Weapon = (Precept_Weapon)PreceptMaker.MakePrecept(pr);
						precept_Weapon.noble = w.first;
						precept_Weapon.despised = w.second;
						ideo.AddPrecept(precept_Weapon);
						precept_Weapon.Init(ideo);
						ideo.anyPreceptEdited = true;
					}, pr.Icon, ideo.Color)));
				}
				bool ValidWeaponPrecept(Precept x)
				{
					if (x is Precept_Weapon precept_Weapon)
					{
						if (precept_Weapon.noble != w.first || precept_Weapon.despised != w.second)
						{
							if (precept_Weapon.noble == w.second)
							{
								return precept_Weapon.despised == w.first;
							}
							return false;
						}
						return true;
					}
					return false;
				}
			}
		}
		void AddXenotypePreceptOption(PreceptDef pr)
		{
			opts.Add(PostProcessOption(new FloatMenuOption("XenotypeEditor".Translate() + "...", delegate
			{
				Find.WindowStack.Add(new Dialog_CreateXenotype(-1, delegate
				{
					CharacterCardUtility.cachedCustomXenotypes = null;
				}));
			})));
			foreach (XenotypeDef item3 in DefDatabase<XenotypeDef>.AllDefs.OrderBy((XenotypeDef x) => 0f - x.displayPriority))
			{
				XenotypeDef xenotype = item3;
				if (!ideo.PreceptsListForReading.Any((Precept y) => y is Precept_Xenotype precept_Xenotype && precept_Xenotype.xenotype == xenotype))
				{
					opts.Add(PostProcessOption(new FloatMenuOption(xenotype.LabelCap, delegate
					{
						PostProcessedXenotypeAction(pr, xenotype, null);
					}, xenotype.Icon, Color.white)));
				}
			}
			foreach (CustomXenotype item4 in CharacterCardUtility.CustomXenotypesForReading)
			{
				CustomXenotype custom = item4;
				if (!ideo.PreceptsListForReading.Any((Precept y) => y is Precept_Xenotype precept_Xenotype && precept_Xenotype.customXenotype == custom))
				{
					opts.Add(PostProcessOption(new FloatMenuOption(custom.name.CapitalizeFirst() + " (" + "Custom".Translate() + ")", delegate
					{
						PostProcessedXenotypeAction(pr, null, custom);
					}, custom.IconDef.Icon, Color.white)));
				}
			}
		}
		List<FloatMenuOption> GetOptionList(ThingDef thing)
		{
			if (!thingGroupsTmp.TryGetValue(thing, out var list2))
			{
				thingGroupsTmp[thing] = (list2 = new List<FloatMenuOption>());
				opts.Add(new FloatMenuOption(thing.LabelCap, delegate
				{
					Find.WindowStack.Add(new FloatMenu(list2, thing.LabelCap));
				}, thing.uiIcon, Color.white));
			}
			return list2;
		}
		void PostProcessedXenotypeAction(PreceptDef pr, XenotypeDef xenotypeDef, CustomXenotype customXenotype)
		{
			Precept_Xenotype precept_Xenotype = (Precept_Xenotype)PreceptMaker.MakePrecept(pr);
			precept_Xenotype.xenotype = xenotypeDef;
			precept_Xenotype.customXenotype = customXenotype;
			ideo.AddPrecept(precept_Xenotype, init: true);
			ideo.anyPreceptEdited = true;
		}
	}

	private static void AddPreceptOption(Ideo ideo, PreceptDef def, IdeoEditMode editMode, List<FloatMenuOption> options, RitualPatternDef patternDef = null)
	{
		tempRequiredMemes.Clear();
		if (!def.visibleOnAddFloatMenu && editMode != IdeoEditMode.Dev)
		{
			return;
		}
		RitualPatternDef pat = patternDef ?? def.ritualPatternBase;
		if (pat != null && (!CanAddRitualPattern(ideo, pat, editMode) || addedPatternDefsTmp.Contains(pat)))
		{
			return;
		}
		string text = GetPreceptLabel(def, patternDef);
		bool flag = IsBasicPrecept(def);
		if (flag)
		{
			text = def.issue.LabelCap + ": " + text;
		}
		Action action = AddPreceptAction;
		if (editMode != IdeoEditMode.Dev)
		{
			AcceptanceReport acceptanceReport = CanAddPrecept(def, pat, ideo);
			if (!acceptanceReport.Accepted)
			{
				action = null;
				text = text + " (" + acceptanceReport.Reason + ")";
			}
			else if (ideo.HasMaxPreceptsForIssue(def.issue))
			{
				return;
			}
		}
		options.Add(new FloatMenuOption(text, action, patternDef?.Icon ?? def.Icon ?? ideo.Icon, flag ? GetIconAndLabelColor(def.impact) : ideo.Color));
		if (pat != null)
		{
			addedPatternDefsTmp.Add(pat);
		}
		void AddPreceptAction()
		{
			Precept precept = PreceptMaker.MakePrecept(def);
			ideo.AddPrecept(precept, init: true, null, pat);
			ideo.anyPreceptEdited = true;
		}
	}

	private static bool IsBasicPrecept(PreceptDef def)
	{
		if (!(def.preceptClass == typeof(Precept)))
		{
			return def.issue.forceWriteLabelInPreceptFloatMenuOption;
		}
		return true;
	}

	private static string GetPreceptLabel(PreceptDef def, RitualPatternDef patternDef)
	{
		return patternDef?.shortDescOverride.CapitalizeFirst() ?? ((string)def.LabelCap);
	}

	private static AcceptanceReport CanAddPrecept(PreceptDef def, RitualPatternDef pat, Ideo ideo)
	{
		AcceptanceReport acceptanceReport = ideo.CanAddPreceptAllFactions(def);
		if (!acceptanceReport)
		{
			return acceptanceReport;
		}
		if (pat != null && !pat.ritualObligationTargetFilter.thingDefs.NullOrEmpty() && !pat.ignoreConsumableBuildingRequirement)
		{
			List<ThingDef> things = pat.ritualObligationTargetFilter.thingDefs;
			bool flag = false;
			foreach (MemeDef allDef in DefDatabase<MemeDef>.AllDefs)
			{
				if (!allDef.consumableBuildings.NullOrEmpty() && allDef.consumableBuildings.Any((ThingDef x) => things.Contains(x)))
				{
					if (ideo.HasMeme(allDef))
					{
						flag = true;
						break;
					}
					tempRequiredMemes.Add(allDef.label);
				}
			}
			if (tempRequiredMemes.Any() && !flag)
			{
				if (tempRequiredMemes.Count == 1)
				{
					return "RequiresMeme".Translate() + ": " + tempRequiredMemes[0].CapitalizeFirst();
				}
				return "RequiresOneOfMemes".Translate() + ": " + tempRequiredMemes.ToCommaList().CapitalizeFirst();
			}
		}
		return AcceptanceReport.WasAccepted;
	}

	public static AcceptanceReport CanListPrecept(Ideo ideo, PreceptDef precept, IdeoEditMode editMode)
	{
		if (!precept.visible && !showAll)
		{
			return false;
		}
		if (editMode == IdeoEditMode.Dev)
		{
			return true;
		}
		return ideo.CanAddPreceptAllFactions(precept);
	}

	private static bool CanAddRitualPattern(Ideo ideo, RitualPatternDef pattern, IdeoEditMode editMode)
	{
		if (editMode == IdeoEditMode.Dev)
		{
			return true;
		}
		if (!pattern.CanFactionUse(FactionForRandomization(ideo)))
		{
			return false;
		}
		return true;
	}

	public static void DoAppearanceItems(Ideo ideo, IdeoEditMode editMode, ref float curY, float width)
	{
		Widgets.Label((width - PreceptBoxSize.x * 3f - 16f) / 2f, ref curY, width, "Appearance".Translate());
		DrawAppearanceItem(4f, curY, StyleItemTab.HairAndBeard, ideo.style.DisplayedHairDef);
		DrawAppearanceItem(4f + PreceptBoxSize.x + 8f, curY, StyleItemTab.Tattoo, ideo.style.DisplayedTattooDef);
		curY += PreceptBoxSize.y + 17f;
		void DrawAppearanceItem(float xOffset, float y, StyleItemTab tab, StyleItemDef defToDisplay)
		{
			Rect rect = new Rect(xOffset, y, PreceptBoxSize.x, PreceptBoxSize.y);
			Rect butRect = rect;
			Color backgroundColor = GetBackgroundColor(PreceptImpact.Medium);
			Widgets.DrawRectFast(rect, backgroundColor);
			GUI.color = new Color(backgroundColor.r + 0.05f, backgroundColor.g + 0.05f, backgroundColor.b + 0.05f);
			Widgets.DrawBox(rect);
			GUI.color = Color.white;
			string text = ((tab == StyleItemTab.HairAndBeard) ? "HairAndBeards" : "Tattoos").Translate();
			if (Mouse.IsOver(rect))
			{
				Widgets.DrawHighlight(rect);
				TooltipHandler.TipRegion(rect, text.Colorize(ColoredText.TipSectionTitleColor) + "\n\n" + ((tab == StyleItemTab.HairAndBeard) ? "HairAndBeardsDesc" : "TattoosDesc").Translate() + ((editMode != IdeoEditMode.None) ? ("\n\n" + ClickToEdit) : string.Empty));
			}
			rect = rect.ContractedBy(4f);
			Rect rect2 = new Rect(rect.x, rect.y, 50f, 50f);
			if (defToDisplay != null)
			{
				GUI.color = PawnHairColors.ReddishBrown;
				Widgets.DefIcon(rect2, defToDisplay, null, 1.25f);
				GUI.color = Color.white;
				rect.xMin = rect2.xMax + 10f;
			}
			Text.Anchor = TextAnchor.MiddleLeft;
			GUI.color = new Color(0.8f, 0.8f, 0.8f);
			Widgets.Label(new Rect(rect.x, rect.y, rect.width, rect.height / 2f), text);
			GUI.color = Color.white;
			Widgets.Label(new Rect(rect.x, rect.y + rect.height / 2f, rect.width, rect.height / 2f), "NumAvailable".Translate(((tab == StyleItemTab.HairAndBeard) ? ideo.style.NumHairAndBeardStylesAvailable : ideo.style.NumTattooStylesAvailable).ToString()));
			Text.Anchor = TextAnchor.UpperLeft;
			if (Widgets.ButtonInvisible(butRect) && TutorAllowsInteraction(editMode))
			{
				Find.WindowStack.Add(new Dialog_EditIdeoStyleItems(ideo, tab, editMode));
			}
		}
	}

	public static FactionDef FactionForRandomization(Ideo ideo)
	{
		FactionDef result = Find.Scenario.playerFaction.factionDef;
		if (Find.World != null)
		{
			foreach (Faction item in Find.FactionManager.AllFactionsVisible)
			{
				if (item.ideos != null && item.ideos.IsPrimary(ideo))
				{
					result = item.def;
					break;
				}
			}
		}
		return result;
	}

	private static void DoDebugButtons(ref float curY, float width, Ideo ideo)
	{
		curY += 17f;
		if (Widgets.ButtonText(new Rect(0f, curY, 200f, 40f), "DEV: Single precept"))
		{
			List<FloatMenuOption> list = new List<FloatMenuOption>();
			List<PreceptDef> allDefsListForReading = DefDatabase<PreceptDef>.AllDefsListForReading;
			for (int i = 0; i < allDefsListForReading.Count; i++)
			{
				PreceptDef defLocal = allDefsListForReading[i];
				list.Add(new FloatMenuOption(defLocal.issue.LabelCap + ": " + (defLocal.LabelCap.NullOrEmpty() ? defLocal.defName : ((string)defLocal.LabelCap)), delegate
				{
					ideo.ClearPrecepts();
					ideo.AddPrecept(PreceptMaker.MakePrecept(defLocal), init: true);
				}));
			}
			Find.WindowStack.Add(new FloatMenu(list));
		}
		if (Widgets.ButtonText(new Rect(210f, curY, 200f, 40f), "DEV: test descriptions..."))
		{
			StringBuilder stringBuilder = new StringBuilder();
			for (int num = 0; num < 100; num++)
			{
				ideo.RegenerateDescription(force: true);
				stringBuilder.Append("template: " + ideo.descriptionTemplate).AppendLine();
				stringBuilder.Append("text: " + ideo.description).AppendLine().AppendLine();
			}
			Log.Message(stringBuilder.ToString());
		}
		if (Widgets.ButtonText(new Rect(420f, curY, 200f, 40f), "DEV: Test names..."))
		{
			List<FloatMenuOption> list2 = new List<FloatMenuOption>();
			list2.Add(new FloatMenuOption("Ideo names (replace)", delegate
			{
				StringBuilder stringBuilder2 = new StringBuilder();
				for (int j = 0; j < 200; j++)
				{
					ideo.foundation.GenerateTextSymbols();
					ideo.foundation.GenerateLeaderTitle();
					stringBuilder2.AppendLine(ideo.name + "\n- " + ideo.adjective + "\n- " + ideo.memberName + "\n- " + ideo.leaderTitleMale);
					stringBuilder2.AppendLine();
				}
				Log.Message(stringBuilder2.ToString());
			}));
			foreach (Precept precept in ideo.PreceptsListForReading)
			{
				PreceptDef def = precept.def;
				if (!(def.preceptClass == typeof(Precept_Ritual)) && !typeof(Precept_Role).IsAssignableFrom(def.preceptClass) && !(def.preceptClass == typeof(Precept_Building)))
				{
					continue;
				}
				list2.Add(new FloatMenuOption(def.issue.LabelCap + ": " + precept.LabelCap, delegate
				{
					StringBuilder stringBuilder2 = new StringBuilder();
					for (int j = 0; j < 200; j++)
					{
						stringBuilder2.AppendLine(precept.GenerateNameRaw());
					}
					Log.Message(stringBuilder2.ToString());
				}));
			}
			Find.WindowStack.Add(new FloatMenu(list2));
		}
		curY += 50f;
	}

	public static void DrawExtraThoughtInfoFromIdeo(Pawn pawn, ref Rect rect)
	{
		if (ModLister.IdeologyInstalled && pawn.Ideo != null && pawn.Ideo.IdeoCausesHumanMeatCravings())
		{
			string text = (Find.TickManager.TicksGame - pawn.mindState.lastHumanMeatIngestedTick).ToStringTicksToPeriod();
			TaggedString taggedString = "LastHumanMeat".Translate() + ": " + "TimeAgo".Translate(text);
			float num = Text.CalcHeight(taggedString, rect.width);
			Rect rect2 = new Rect(rect.x, rect.yMax - num, rect.width, num);
			Widgets.Label(rect2, taggedString);
			if (Mouse.IsOver(rect2))
			{
				Widgets.DrawHighlight(rect2);
				TooltipHandler.TipRegion(rect2, "LastHumanMeatDesc".Translate(pawn.Named("PAWN"), pawn.Ideo.Named("IDEO"), 8.Named("DURATION")).Resolve());
			}
			rect.yMax -= num;
		}
	}

	public static Color GetBackgroundColor(PreceptImpact impact)
	{
		return impact switch
		{
			PreceptImpact.High => new Color(0.24f, 0.24f, 0.24f), 
			PreceptImpact.Medium => new Color(0.18f, 0.18f, 0.18f), 
			PreceptImpact.Low => new Color(0.13f, 0.13f, 0.13f), 
			_ => Color.white, 
		};
	}

	public static Color GetIconAndLabelColor(PreceptImpact impact)
	{
		return impact switch
		{
			PreceptImpact.High => new Color(1f, 1f, 0.5f), 
			PreceptImpact.Medium => new Color(1f, 1f, 1f), 
			PreceptImpact.Low => new Color(0.7f, 0.7f, 0.7f), 
			_ => Color.white, 
		};
	}

	public static bool IsPreceptRelatedForUI(MemeDef meme, PreceptDef precept)
	{
		if (!precept.requiredMemes.Contains(meme) && !precept.associatedMemes.Contains(meme))
		{
			if (meme.requireOne != null)
			{
				return meme.requireOne.Any((List<PreceptDef> pl) => pl.Contains(precept));
			}
			return false;
		}
		return true;
	}

	public static void DrawImpactInfo(Rect rect, List<MemeDef> memes)
	{
		int num = ImpactOf(memes);
		if (num == 0)
		{
			return;
		}
		string text = num.ToStringCached();
		Text.Font = GameFont.Medium;
		float num2 = Mathf.Max(16f, Text.CalcSize(text).x + 2f);
		Rect rect2 = rect;
		Rect rect3 = rect.TopPartPixels(rect.height - 12f);
		rect3.SplitVertically(rect3.width - num2, out var left, out var right);
		right.y += 1f;
		if (Mouse.IsOver(rect))
		{
			Widgets.DrawHighlight(rect);
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine("IdeoImpactOverallDesc".Translate());
			foreach (MemeDef meme in memes)
			{
				if (meme.impact != 0)
				{
					stringBuilder.Append("\n  - ").Append(meme.LabelCap).Append(": ")
						.Append(meme.impact.ToStringCached());
				}
			}
			TooltipHandler.TipRegion(rect, stringBuilder.ToString());
		}
		Text.Font = GameFont.Medium;
		Text.Anchor = TextAnchor.LowerRight;
		Widgets.Label(right, text);
		Text.Font = GameFont.Tiny;
		Text.Anchor = TextAnchor.LowerRight;
		Widgets.Label(rect2, IdeoImpactUtility.OverallImpactLabel(num).CapitalizeFirst());
		Text.Font = GameFont.Small;
		Text.Anchor = TextAnchor.LowerRight;
		Widgets.Label(left, "IdeoImpactOverall".Translate() + ":");
		Text.Font = GameFont.Small;
		Text.Anchor = TextAnchor.UpperLeft;
	}

	private static int ImpactOf(List<MemeDef> memes)
	{
		int num = 0;
		foreach (MemeDef meme in memes)
		{
			num += meme.impact;
		}
		return num;
	}

	public static void DrawIdeoPlate(Rect r, Ideo ideo, Pawn pawn = null)
	{
		Widgets.DrawHighlightIfMouseover(r);
		Rect rect = new Rect(r.x, r.y, r.width, r.height);
		Rect rect2 = new Rect(r.x + 1f, r.y + 1f, 20f, 20f);
		ideo.DrawIcon(rect2);
		Widgets.Label(new Rect(rect.x + rect.height + 5f, rect.y, rect.width - 10f, rect.height), ideo.name);
		if (Widgets.ButtonInvisible(r))
		{
			OpenIdeoInfo(ideo);
		}
		if (!Mouse.IsOver(r))
		{
			return;
		}
		TaggedString taggedString = ideo.name.Colorize(ColoredText.TipSectionTitleColor);
		if (pawn != null)
		{
			taggedString += "\n" + "Certainty".Translate().CapitalizeFirst() + ": " + pawn.ideo.Certainty.ToStringPercent();
		}
		taggedString += "\n\n" + "ClickForMoreInfo".Translate().Colorize(ColoredText.SubtleGrayColor);
		if (pawn != null && pawn.ideo.PreviousIdeos.Any())
		{
			taggedString += "\n\n" + "Formerly".Translate().CapitalizeFirst() + ": \n" + pawn.ideo.PreviousIdeos.Select((Ideo x) => x.name).ToLineList("  - ");
		}
		TooltipHandler.TipRegion(r, taggedString.Resolve());
	}
}
