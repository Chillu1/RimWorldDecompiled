using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace RimWorld;

[StaticConstructorOnStartup]
public class Dialog_StylingStation : Window
{
	public enum StylingTab
	{
		Hair,
		Beard,
		TattooFace,
		TattooBody,
		ApparelColor,
		BodyType,
		HeadType
	}

	private Pawn pawn;

	private Thing stylingStation;

	private HairDef initialHairDef;

	private BeardDef initialBeardDef;

	private TattooDef initialFaceTattoo;

	private TattooDef initialBodyTattoo;

	private Color? initialSkinColor;

	private Color desiredSkinColor;

	private BodyTypeDef initialBodyType;

	private HeadTypeDef initialHeadType;

	private Color desiredHairColor;

	private StylingTab curTab;

	private Vector2 hairScrollPosition;

	private Vector2 beardScrollPosition;

	private Vector2 faceTattooScrollPosition;

	private Vector2 bodyTattooScrollPosition;

	private Vector2 apparelColorScrollPosition;

	private Vector2 bodyTypeScrollPosition;

	private Vector2 headTypeScrollPosition;

	private List<TabRecord> tabs = new List<TabRecord>();

	private Dictionary<Apparel, Color> apparelColors = new Dictionary<Apparel, Color>();

	private float viewRectHeight;

	private bool showHeadgear;

	private bool showClothes;

	private bool devEditMode;

	private List<Color> allColors;

	private List<Color> allHairColors;

	private List<Color> allSkinColors;

	private float colorsHeight;

	private static readonly Vector2 ButSize = new Vector2(200f, 40f);

	private static readonly Vector3 PortraitOffset = new Vector3(0f, 0f, 0.15f);

	private const float PortraitZoom = 1.1f;

	private const float TabMargin = 18f;

	private const float IconSize = 60f;

	private const float LeftRectPercent = 0.3f;

	private const float ApparelRowHeight = 126f;

	private const float ApparelRowButtonsHeight = 24f;

	private const float SetColorButtonWidth = 200f;

	private static readonly Texture2D FavoriteColorTex = ContentFinder<Texture2D>.Get("UI/Icons/ColorSelector/ColorFavourite");

	private static readonly Texture2D IdeoColorTex = ContentFinder<Texture2D>.Get("UI/Icons/ColorSelector/ColorIdeology");

	private static List<string> tmpUnwantedStyleNames = new List<string>();

	private static List<StyleItemDef> tmpStyleItems = new List<StyleItemDef>();

	private bool DevMode => stylingStation == null;

	public override Vector2 InitialSize => new Vector2(950f, 750f);

	private List<Color> AllColors
	{
		get
		{
			if (allColors == null)
			{
				allColors = new List<Color>();
				if (pawn.Ideo != null && !Find.IdeoManager.classicMode)
				{
					allColors.Add(pawn.Ideo.ApparelColor);
				}
				foreach (ColorDef colDef in DefDatabase<ColorDef>.AllDefs.Where((ColorDef x) => x.colorType == ColorType.Ideo || x.colorType == ColorType.Misc || (DevMode && !ModsConfig.IdeologyActive && x.colorType == ColorType.Structure)))
				{
					if (!allColors.Any((Color x) => x.IndistinguishableFrom(colDef.color)))
					{
						allColors.Add(colDef.color);
					}
				}
				allColors.SortByColor((Color x) => x);
			}
			return allColors;
		}
	}

	private List<Color> AllHairColors
	{
		get
		{
			if (allHairColors == null)
			{
				allHairColors = new List<Color>();
				foreach (ColorDef allDef in DefDatabase<ColorDef>.AllDefs)
				{
					Color color = allDef.color;
					if (allDef.displayInStylingStationUI && !allHairColors.Any((Color x) => x.WithinDiffThresholdFrom(color, 0.15f)))
					{
						allHairColors.Add(color);
					}
				}
				allHairColors.SortByColor((Color x) => x);
			}
			return allHairColors;
		}
	}

	private List<Color> AllSkinColors
	{
		get
		{
			if (allSkinColors == null)
			{
				allSkinColors = new List<Color>();
				foreach (GeneDef item in PawnSkinColors.SkinColorGenesInOrder)
				{
					Color value = item.skinColorBase.Value;
					allSkinColors.Add(value);
				}
				allSkinColors.SortByColor((Color x) => x);
			}
			return allSkinColors;
		}
	}

	public Dialog_StylingStation(Pawn pawn, Thing stylingStation)
	{
		this.pawn = pawn;
		this.stylingStation = stylingStation;
		initialHairDef = pawn.story.hairDef;
		desiredHairColor = pawn.story.HairColor;
		initialBeardDef = pawn.style.beardDef;
		initialFaceTattoo = pawn.style.FaceTattoo;
		initialBodyTattoo = pawn.style.BodyTattoo;
		initialSkinColor = pawn.story.skinColorOverride;
		desiredSkinColor = pawn.story.SkinColor;
		initialBodyType = pawn.story.bodyType;
		initialHeadType = pawn.story.headType;
		forcePause = true;
		showClothes = true;
		closeOnAccept = false;
		closeOnCancel = false;
		foreach (Apparel item in pawn.apparel.WornApparel)
		{
			if (item.TryGetComp<CompColorable>() != null)
			{
				apparelColors.Add(item, item.DesiredColor ?? item.GetColorIgnoringTainted());
			}
		}
	}

	public override void PostOpen()
	{
		if (!ModLister.CheckIdeology("Styling station"))
		{
			Close();
		}
		else
		{
			base.PostOpen();
		}
	}

	public override void DoWindowContents(Rect inRect)
	{
		Text.Font = GameFont.Medium;
		Rect rect = new Rect(inRect);
		rect.height = Text.LineHeight * 2f;
		Widgets.Label(rect, "StylePawn".Translate().CapitalizeFirst() + ": " + Find.ActiveLanguageWorker.WithDefiniteArticle(pawn.Name.ToStringShort, pawn.gender, plural: false, name: true).ApplyTag(TagType.Name));
		Text.Font = GameFont.Small;
		inRect.yMin = rect.yMax + 4f;
		Rect rect2 = inRect;
		rect2.width *= 0.3f;
		rect2.yMax -= ButSize.y + 4f;
		DrawPawn(rect2);
		Rect rect3 = inRect;
		rect3.xMin = rect2.xMax + 10f;
		rect3.yMax -= ButSize.y + 4f;
		DrawTabs(rect3);
		DrawBottomButtons(inRect);
		if (Prefs.DevMode)
		{
			Widgets.CheckboxLabeled(new Rect(inRect.xMax - 120f, 0f, 120f, 30f), "DEV: Show all", ref devEditMode);
		}
	}

	private void DrawPawn(Rect rect)
	{
		Rect rect2 = rect;
		rect2.yMin = rect.yMax - Text.LineHeight * 2f;
		Widgets.CheckboxLabeled(new Rect(rect2.x, rect2.y, rect2.width, rect2.height / 2f), "ShowHeadgear".Translate(), ref showHeadgear);
		Widgets.CheckboxLabeled(new Rect(rect2.x, rect2.y + rect2.height / 2f, rect2.width, rect2.height / 2f), "ShowApparel".Translate(), ref showClothes);
		rect.yMax = rect2.yMin - 4f;
		Widgets.BeginGroup(rect);
		for (int i = 0; i < 3; i++)
		{
			Rect position = new Rect(0f, rect.height / 3f * (float)i, rect.width, rect.height / 3f).ContractedBy(4f);
			RenderTexture image = PortraitsCache.Get(pawn, new Vector2(position.width, position.height), new Rot4(2 - i), PortraitOffset, 1.1f, supersample: true, compensateForUIScale: true, showHeadgear, showClothes, apparelColors, desiredHairColor, stylingStation: true);
			GUI.DrawTexture(position, image);
		}
		Widgets.EndGroup();
		if (pawn.style.HasAnyUnwantedStyleItem)
		{
			string text = "PawnUnhappyWithStyleItems".Translate(pawn.Named("PAWN")) + ": ";
			tmpUnwantedStyleNames.Clear();
			if (pawn.style.HasUnwantedHairStyle)
			{
				tmpUnwantedStyleNames.Add("Hair".Translate());
			}
			if (pawn.style.HasUnwantedBeard)
			{
				tmpUnwantedStyleNames.Add("Beard".Translate());
			}
			if (pawn.style.HasUnwantedFaceTattoo)
			{
				tmpUnwantedStyleNames.Add("TattooFace".Translate());
			}
			if (pawn.style.HasUnwantedBodyTattoo)
			{
				tmpUnwantedStyleNames.Add("TattooBody".Translate());
			}
			GUI.color = ColorLibrary.RedReadable;
			Widgets.Label(new Rect(rect.x, rect.yMin - 30f, rect.width, Text.LineHeight * 2f + 10f), "Warning".Translate() + ": " + text + tmpUnwantedStyleNames.ToCommaList().CapitalizeFirst());
			GUI.color = Color.white;
		}
	}

	private void DrawTabs(Rect rect)
	{
		tabs.Clear();
		tabs.Add(new TabRecord("Hair".Translate().CapitalizeFirst(), delegate
		{
			curTab = StylingTab.Hair;
		}, curTab == StylingTab.Hair));
		if (pawn.style.CanWantBeard || devEditMode)
		{
			tabs.Add(new TabRecord("Beard".Translate().CapitalizeFirst(), delegate
			{
				curTab = StylingTab.Beard;
			}, curTab == StylingTab.Beard));
		}
		tabs.Add(new TabRecord("TattooFace".Translate().CapitalizeFirst(), delegate
		{
			curTab = StylingTab.TattooFace;
		}, curTab == StylingTab.TattooFace));
		tabs.Add(new TabRecord("TattooBody".Translate().CapitalizeFirst(), delegate
		{
			curTab = StylingTab.TattooBody;
		}, curTab == StylingTab.TattooBody));
		tabs.Add(new TabRecord("ApparelColor".Translate().CapitalizeFirst(), delegate
		{
			curTab = StylingTab.ApparelColor;
		}, curTab == StylingTab.ApparelColor));
		if (devEditMode)
		{
			tabs.Add(new TabRecord("Body type", delegate
			{
				curTab = StylingTab.BodyType;
			}, curTab == StylingTab.BodyType));
			tabs.Add(new TabRecord("Head type", delegate
			{
				curTab = StylingTab.HeadType;
			}, curTab == StylingTab.HeadType));
		}
		Widgets.DrawMenuSection(rect);
		TabDrawer.DrawTabs(rect, tabs);
		rect = rect.ContractedBy(18f);
		switch (curTab)
		{
		case StylingTab.Hair:
			rect.yMax -= colorsHeight;
			DrawStylingItemType(rect, ref hairScrollPosition, delegate(Rect r, HairDef h)
			{
				GUI.color = desiredHairColor;
				Widgets.DefIcon(r, h, null, 1.25f);
				GUI.color = Color.white;
			}, delegate(HairDef h)
			{
				pawn.story.hairDef = h;
			}, (StyleItemDef h) => pawn.story.hairDef == h, (StyleItemDef h) => initialHairDef == h, null, doColors: true);
			break;
		case StylingTab.Beard:
			DrawStylingItemType(rect, ref beardScrollPosition, delegate(Rect r, BeardDef b)
			{
				GUI.color = desiredHairColor;
				Widgets.DefIcon(r, b, null, 1.25f);
				GUI.color = Color.white;
			}, delegate(BeardDef b)
			{
				pawn.style.beardDef = b;
			}, (StyleItemDef b) => pawn.style.beardDef == b, (StyleItemDef b) => initialBeardDef == b);
			break;
		case StylingTab.TattooFace:
			DrawStylingItemType(rect, ref faceTattooScrollPosition, delegate(Rect r, TattooDef t)
			{
				Widgets.DefIcon(r, t);
			}, delegate(TattooDef t)
			{
				pawn.style.FaceTattoo = t;
			}, (StyleItemDef t) => pawn.style.FaceTattoo == t, (StyleItemDef t) => initialFaceTattoo == t, (StyleItemDef t) => ((TattooDef)t).tattooType == TattooType.Face);
			break;
		case StylingTab.TattooBody:
			DrawStylingItemType(rect, ref bodyTattooScrollPosition, delegate(Rect r, TattooDef t)
			{
				Widgets.DefIcon(r, t);
			}, delegate(TattooDef t)
			{
				pawn.style.BodyTattoo = t;
			}, (StyleItemDef t) => pawn.style.BodyTattoo == t, (StyleItemDef t) => initialBodyTattoo == t, (StyleItemDef t) => ((TattooDef)t).tattooType == TattooType.Body);
			break;
		case StylingTab.BodyType:
			rect.yMax -= colorsHeight;
			DevDrawBodyType(rect, ref bodyTypeScrollPosition, delegate(Rect r, BodyTypeDef b)
			{
				GUI.color = pawn.story.SkinColor;
				Widgets.DefIcon(r, b, null, 1.25f);
				GUI.color = Color.white;
			}, delegate(BodyTypeDef t)
			{
				pawn.story.bodyType = t;
			}, (BodyTypeDef t) => pawn.story.bodyType == t);
			break;
		case StylingTab.HeadType:
			rect.yMax -= colorsHeight;
			DevDrawHeadType(rect, ref headTypeScrollPosition, delegate(Rect r, HeadTypeDef b)
			{
				GUI.color = pawn.story.SkinColor;
				Widgets.DefIcon(r, b, null, 1.25f);
				GUI.color = Color.white;
			}, delegate(HeadTypeDef t)
			{
				pawn.story.headType = t;
			}, (HeadTypeDef t) => pawn.story.headType == t);
			break;
		case StylingTab.ApparelColor:
			DrawApparelColor(rect);
			break;
		}
	}

	private void DrawDyeRequirement(Rect rect, ref float curY, int requiredDye)
	{
		Widgets.ThingIcon(new Rect(rect.x, curY, Text.LineHeight, Text.LineHeight), ThingDefOf.Dye, null, null, 1.1f);
		string text = string.Concat("Required".Translate() + ": ", requiredDye.ToString(), " ", ThingDefOf.Dye.label);
		float x = Text.CalcSize(text).x;
		Widgets.Label(new Rect(rect.x + Text.LineHeight + 4f, curY, x, Text.LineHeight), text);
		Rect rect2 = new Rect(rect.x, curY, x + Text.LineHeight + 8f, Text.LineHeight);
		if (Mouse.IsOver(rect2))
		{
			Widgets.DrawHighlight(rect2);
			TooltipHandler.TipRegionByKey(rect2, "TooltipDyeExplanation");
		}
		curY += Text.LineHeight;
	}

	private void DrawHairColors(Rect rect)
	{
		float y = rect.y;
		Widgets.ColorSelector(new Rect(rect.x, y, rect.width, colorsHeight), ref desiredHairColor, AllHairColors, out colorsHeight);
		y += colorsHeight;
		if (pawn.Spawned && desiredHairColor != pawn.story.HairColor)
		{
			Color value = desiredHairColor;
			Color? nextHairColor = pawn.style.nextHairColor;
			if (value != nextHairColor)
			{
				DrawDyeRequirement(rect, ref y, 1);
				if (pawn.Map.resourceCounter.GetCount(ThingDefOf.Dye) < 1)
				{
					Rect rect2 = new Rect(rect.x, y, rect.width, Text.LineHeight);
					Color color = GUI.color;
					GUI.color = ColorLibrary.RedReadable;
					Widgets.Label(rect2, "NotEnoughDye".Translate() + " " + "NotEnoughDyeWillRecolorHair".Translate());
					GUI.color = color;
				}
			}
		}
		colorsHeight += Text.LineHeight * 2f;
	}

	private void DrawApparelColor(Rect rect)
	{
		Rect viewRect = new Rect(rect.x, rect.y, rect.width - 16f, viewRectHeight);
		Widgets.BeginScrollView(rect, ref apparelColorScrollPosition, viewRect);
		int num = 0;
		float curY = rect.y;
		foreach (Apparel item in pawn.apparel.WornApparel)
		{
			if (!apparelColors.TryGetValue(item, out var value))
			{
				continue;
			}
			Rect rect2 = new Rect(rect.x, curY, viewRect.width, 92f);
			curY += rect2.height + 10f;
			if (!pawn.apparel.IsLocked(item) || DevMode)
			{
				Widgets.ColorSelector(rect2, ref value, AllColors, out var _, Widgets.GetIconFor(item.def, item.Stuff, item.StyleDef), 22, 2, ColorSelecterExtraOnGUI);
				float num2 = rect2.x;
				if (pawn.Ideo != null && !Find.IdeoManager.classicMode)
				{
					rect2 = new Rect(num2, curY, 200f, 24f);
					if (Widgets.ButtonText(rect2, "SetIdeoColor".Translate()))
					{
						value = pawn.Ideo.ApparelColor;
						SoundDefOf.Tick_Low.PlayOneShotOnCamera();
					}
					num2 += 210f;
				}
				if (pawn.story?.favoriteColor != null)
				{
					rect2 = new Rect(num2, curY, 200f, 24f);
					if (Widgets.ButtonText(rect2, "SetFavoriteColor".Translate()))
					{
						value = pawn.story.favoriteColor.color;
						SoundDefOf.Tick_Low.PlayOneShotOnCamera();
					}
				}
				if (!value.IndistinguishableFrom(item.GetColorIgnoringTainted()))
				{
					num++;
				}
				apparelColors[item] = value;
			}
			else
			{
				Widgets.ColorSelectorIcon(new Rect(rect2.x, rect2.y, 88f, 88f), item.def.uiIcon, value);
				Text.Anchor = TextAnchor.MiddleLeft;
				Rect rect3 = rect2;
				rect3.x += 100f;
				Widgets.Label(rect3, ((string)"ApparelLockedCannotRecolor".Translate(pawn.Named("PAWN"), item.Named("APPAREL"))).Colorize(ColorLibrary.RedReadable));
				Text.Anchor = TextAnchor.UpperLeft;
			}
			curY += 34f;
		}
		if (pawn.Spawned)
		{
			if (num > 0)
			{
				DrawDyeRequirement(rect, ref curY, num);
			}
			if (pawn.Map.resourceCounter.GetCount(ThingDefOf.Dye) < num)
			{
				Rect rect4 = new Rect(rect.x, curY, rect.width - 16f - 10f, 60f);
				Color color = GUI.color;
				GUI.color = ColorLibrary.RedReadable;
				Widgets.Label(rect4, "NotEnoughDye".Translate() + " " + "NotEnoughDyeWillRecolorApparel".Translate());
				GUI.color = color;
				curY += rect4.height;
			}
		}
		if (Event.current.type == EventType.Layout)
		{
			viewRectHeight = curY - rect.y;
		}
		Widgets.EndScrollView();
	}

	private void ColorSelecterExtraOnGUI(Color color, Rect boxRect)
	{
		Texture2D texture2D = null;
		TaggedString taggedString = null;
		bool flag = Mouse.IsOver(boxRect);
		if (pawn.story?.favoriteColor != null && color.IndistinguishableFrom(pawn.story.favoriteColor.color))
		{
			texture2D = FavoriteColorTex;
			if (flag)
			{
				taggedString = "FavoriteColorPickerTip".Translate(pawn.Named("PAWN"));
			}
		}
		else if (pawn.Ideo != null && !Find.IdeoManager.classicMode && color.IndistinguishableFrom(pawn.Ideo.ApparelColor))
		{
			texture2D = IdeoColorTex;
			if (flag)
			{
				taggedString = "IdeoColorPickerTip".Translate(pawn.Named("PAWN"));
			}
		}
		if (texture2D != null)
		{
			Rect position = boxRect.ContractedBy(4f);
			GUI.color = Color.black.ToTransparent(0.2f);
			GUI.DrawTexture(new Rect(position.x + 2f, position.y + 2f, position.width, position.height), texture2D);
			GUI.color = Color.white.ToTransparent(0.8f);
			GUI.DrawTexture(position, texture2D);
			GUI.color = Color.white;
		}
		if (!taggedString.NullOrEmpty())
		{
			TooltipHandler.TipRegion(boxRect, taggedString);
		}
	}

	private void DrawStylingItemType<T>(Rect rect, ref Vector2 scrollPosition, Action<Rect, T> drawAction, Action<T> selectAction, Func<StyleItemDef, bool> hasStyleItem, Func<StyleItemDef, bool> hadStyleItem, Func<StyleItemDef, bool> extraValidator = null, bool doColors = false) where T : StyleItemDef
	{
		Rect viewRect = new Rect(rect.x, rect.y, rect.width - 16f, viewRectHeight);
		int num = Mathf.FloorToInt(viewRect.width / 60f) - 1;
		float num2 = (viewRect.width - (float)num * 60f - (float)(num - 1) * 10f) / 2f;
		int num3 = 0;
		int num4 = 0;
		int num5 = 0;
		tmpStyleItems.Clear();
		tmpStyleItems.AddRange(DefDatabase<T>.AllDefs.Where((T x) => (devEditMode || PawnStyleItemChooser.WantsToUseStyle(pawn, x) || hadStyleItem(x)) && (extraValidator == null || extraValidator(x))));
		tmpStyleItems.SortBy((StyleItemDef x) => 0f - PawnStyleItemChooser.FrequencyFromGender(x, pawn));
		if (tmpStyleItems.NullOrEmpty())
		{
			Widgets.NoneLabelCenteredVertically(rect, "(" + "NoneUsableForPawn".Translate(pawn.Named("PAWN")) + ")");
		}
		else
		{
			Widgets.BeginScrollView(rect, ref scrollPosition, viewRect);
			foreach (StyleItemDef tmpStyleItem in tmpStyleItems)
			{
				if (num5 >= num - 1)
				{
					num5 = 0;
					num4++;
				}
				else if (num3 > 0)
				{
					num5++;
				}
				Rect rect2 = new Rect(rect.x + num2 + (float)num5 * 60f + (float)num5 * 10f, rect.y + (float)num4 * 60f + (float)num4 * 10f, 60f, 60f);
				Widgets.DrawHighlight(rect2);
				if (Mouse.IsOver(rect2))
				{
					Widgets.DrawHighlight(rect2);
					TooltipHandler.TipRegion(rect2, tmpStyleItem.LabelCap);
				}
				drawAction?.Invoke(rect2, tmpStyleItem as T);
				if (hasStyleItem(tmpStyleItem))
				{
					Widgets.DrawBox(rect2, 2);
				}
				if (Widgets.ButtonInvisible(rect2))
				{
					selectAction?.Invoke(tmpStyleItem as T);
					SoundDefOf.Tick_High.PlayOneShotOnCamera();
					pawn.Drawer.renderer.SetAllGraphicsDirty();
				}
				num3++;
			}
			if (Event.current.type == EventType.Layout)
			{
				viewRectHeight = (float)(num4 + 1) * 60f + (float)num4 * 10f + 10f;
			}
			Widgets.EndScrollView();
		}
		if (doColors)
		{
			DrawHairColors(new Rect(rect.x, rect.yMax + 10f, rect.width, colorsHeight));
		}
	}

	private void DevDrawBodyType(Rect rect, ref Vector2 scrollPosition, Action<Rect, BodyTypeDef> drawAction, Action<BodyTypeDef> selectAction, Func<BodyTypeDef, bool> hasStyleItem)
	{
		Rect viewRect = new Rect(rect.x, rect.y, rect.width - 16f, viewRectHeight);
		int num = Mathf.FloorToInt(viewRect.width / 60f) - 1;
		float num2 = (viewRect.width - (float)num * 60f - (float)(num - 1) * 10f) / 2f;
		int num3 = 0;
		int num4 = 0;
		int num5 = 0;
		Widgets.BeginScrollView(rect, ref scrollPosition, viewRect);
		foreach (BodyTypeDef allDef in DefDatabase<BodyTypeDef>.AllDefs)
		{
			if (allDef != BodyTypeDefOf.Child && allDef != BodyTypeDefOf.Baby)
			{
				if (num5 >= num - 1)
				{
					num5 = 0;
					num4++;
				}
				else if (num3 > 0)
				{
					num5++;
				}
				Rect rect2 = new Rect(rect.x + num2 + (float)num5 * 60f + (float)num5 * 10f, rect.y + (float)num4 * 60f + (float)num4 * 10f, 60f, 60f);
				Widgets.DrawHighlight(rect2);
				if (Mouse.IsOver(rect2))
				{
					Widgets.DrawHighlight(rect2);
					TooltipHandler.TipRegion(rect2, allDef.LabelCap);
				}
				drawAction?.Invoke(rect2, allDef);
				if (hasStyleItem(allDef))
				{
					Widgets.DrawBox(rect2, 2);
				}
				if (Widgets.ButtonInvisible(rect2))
				{
					selectAction?.Invoke(allDef);
					SoundDefOf.Tick_High.PlayOneShotOnCamera();
					pawn.Drawer.renderer.SetAllGraphicsDirty();
				}
				num3++;
			}
		}
		if (Event.current.type == EventType.Layout)
		{
			viewRectHeight = (float)(num4 + 1) * 60f + (float)num4 * 10f + 10f;
		}
		Widgets.EndScrollView();
		Rect rect3 = new Rect(rect.x, rect.yMax + 10f, rect.width, colorsHeight);
		float y = rect3.y;
		Widgets.ColorSelector(new Rect(rect3.x, y, rect3.width, colorsHeight), ref desiredSkinColor, AllSkinColors, out colorsHeight);
		pawn.story.skinColorOverride = desiredSkinColor;
		colorsHeight += Text.LineHeight * 2f;
	}

	private void DevDrawHeadType(Rect rect, ref Vector2 scrollPosition, Action<Rect, HeadTypeDef> drawAction, Action<HeadTypeDef> selectAction, Func<HeadTypeDef, bool> hasStyleItem)
	{
		Rect viewRect = new Rect(rect.x, rect.y, rect.width - 16f, viewRectHeight);
		int num = Mathf.FloorToInt(viewRect.width / 60f) - 1;
		float num2 = (viewRect.width - (float)num * 60f - (float)(num - 1) * 10f) / 2f;
		int num3 = 0;
		int num4 = 0;
		int num5 = 0;
		Widgets.BeginScrollView(rect, ref scrollPosition, viewRect);
		foreach (HeadTypeDef allDef in DefDatabase<HeadTypeDef>.AllDefs)
		{
			if (allDef != HeadTypeDefOf.Skull && allDef != HeadTypeDefOf.Stump)
			{
				if (num5 >= num - 1)
				{
					num5 = 0;
					num4++;
				}
				else if (num3 > 0)
				{
					num5++;
				}
				Rect rect2 = new Rect(rect.x + num2 + (float)num5 * 60f + (float)num5 * 10f, rect.y + (float)num4 * 60f + (float)num4 * 10f, 60f, 60f);
				Widgets.DrawHighlight(rect2);
				if (Mouse.IsOver(rect2))
				{
					Widgets.DrawHighlight(rect2);
					TooltipHandler.TipRegion(rect2, allDef.LabelCap);
				}
				drawAction?.Invoke(rect2, allDef);
				if (hasStyleItem(allDef))
				{
					Widgets.DrawBox(rect2, 2);
				}
				if (Widgets.ButtonInvisible(rect2))
				{
					selectAction?.Invoke(allDef);
					SoundDefOf.Tick_High.PlayOneShotOnCamera();
					pawn.Drawer.renderer.SetAllGraphicsDirty();
				}
				num3++;
			}
		}
		if (Event.current.type == EventType.Layout)
		{
			viewRectHeight = (float)(num4 + 1) * 60f + (float)num4 * 10f + 10f;
		}
		Widgets.EndScrollView();
		Rect rect3 = new Rect(rect.x, rect.yMax + 10f, rect.width, colorsHeight);
		float y = rect3.y;
		Widgets.ColorSelector(new Rect(rect3.x, y, rect3.width, colorsHeight), ref desiredSkinColor, AllSkinColors, out colorsHeight);
		pawn.story.skinColorOverride = desiredSkinColor;
		colorsHeight += Text.LineHeight * 2f;
	}

	private void DrawBottomButtons(Rect inRect)
	{
		if (Widgets.ButtonText(new Rect(inRect.x, inRect.yMax - ButSize.y, ButSize.x, ButSize.y), "Cancel".Translate()))
		{
			Reset();
			Close();
		}
		if (Widgets.ButtonText(new Rect(inRect.xMin + inRect.width / 2f - ButSize.x / 2f, inRect.yMax - ButSize.y, ButSize.x, ButSize.y), "Reset".Translate()))
		{
			Reset();
			SoundDefOf.Tick_Low.PlayOneShotOnCamera();
		}
		if (!Widgets.ButtonText(new Rect(inRect.xMax - ButSize.x, inRect.yMax - ButSize.y, ButSize.x, ButSize.y), "Accept".Translate()))
		{
			return;
		}
		if (pawn.story.hairDef == initialHairDef && pawn.style.beardDef == initialBeardDef && pawn.style.FaceTattoo == initialFaceTattoo && pawn.style.BodyTattoo == initialBodyTattoo && !(pawn.story.HairColor != desiredHairColor))
		{
			Color? skinColorOverride = pawn.story.skinColorOverride;
			Color? color = initialSkinColor;
			if (skinColorOverride.HasValue == color.HasValue && (!skinColorOverride.HasValue || !(skinColorOverride.GetValueOrDefault() != color.GetValueOrDefault())) && pawn.story.bodyType == initialBodyType && pawn.story.headType == initialHeadType)
			{
				goto IL_0313;
			}
		}
		if (!DevMode)
		{
			pawn.style.SetupNextLookChangeData(pawn.story.hairDef, pawn.style.beardDef, pawn.style.FaceTattoo, pawn.style.BodyTattoo, desiredHairColor);
			Reset(resetColors: false);
			pawn.jobs.TryTakeOrderedJob(JobMaker.MakeJob(JobDefOf.UseStylingStation, stylingStation), JobTag.Misc);
		}
		else
		{
			pawn.story.HairColor = desiredHairColor;
			pawn.style.Notify_StyleItemChanged();
		}
		goto IL_0313;
		IL_0313:
		ApplyApparelColors();
		Close();
	}

	private void ApplyApparelColors()
	{
		foreach (KeyValuePair<Apparel, Color> apparelColor in apparelColors)
		{
			if (!DevMode)
			{
				if (apparelColor.Key.GetColorIgnoringTainted() != apparelColor.Value)
				{
					apparelColor.Key.DesiredColor = apparelColor.Value;
				}
			}
			else if (apparelColor.Key.GetColorIgnoringTainted() != apparelColor.Value)
			{
				apparelColor.Key.DrawColor = apparelColor.Value;
				apparelColor.Key.Notify_ColorChanged();
				apparelColor.Key.DesiredColor = null;
			}
		}
		pawn.mindState.Notify_OutfitChanged();
	}

	private void Reset(bool resetColors = true)
	{
		if (resetColors)
		{
			apparelColors.Clear();
			foreach (Apparel item in pawn.apparel.WornApparel)
			{
				if (item.TryGetComp<CompColorable>() != null)
				{
					apparelColors.Add(item, item.DesiredColor ?? item.GetColorIgnoringTainted());
				}
			}
			desiredHairColor = pawn.story.HairColor;
		}
		pawn.story.hairDef = initialHairDef;
		pawn.style.beardDef = initialBeardDef;
		pawn.style.FaceTattoo = initialFaceTattoo;
		pawn.style.BodyTattoo = initialBodyTattoo;
		pawn.story.bodyType = initialBodyType;
		pawn.story.headType = initialHeadType;
		pawn.story.skinColorOverride = initialSkinColor;
		pawn.Drawer.renderer.SetAllGraphicsDirty();
	}
}
