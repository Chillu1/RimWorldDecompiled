using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using LudeonTK;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse.Sound;
using Verse.Steam;

namespace Verse;

[StaticConstructorOnStartup]
public static class Widgets
{
	public enum DraggableResult
	{
		Idle,
		Pressed,
		Dragged,
		DraggedThenPressed
	}

	private enum RangeEnd : byte
	{
		None,
		Min,
		Max
	}

	[Flags]
	public enum ColorComponents
	{
		Red = 1,
		Green = 2,
		Blue = 4,
		Hue = 8,
		Sat = 0x10,
		Value = 0x20,
		None = 0,
		All = 0x3F
	}

	public struct DropdownMenuElement<Payload>
	{
		public FloatMenuOption option;

		public Payload payload;
	}

	public static Stack<bool> mouseOverScrollViewStack;

	public static readonly GUIStyle EmptyStyle;

	[TweakValue("Input", 0f, 100f)]
	private static float DragStartDistanceSquared;

	public const int LeftMouseButton = 0;

	public static readonly Color InactiveColor;

	public static readonly Color HighlightStrongBgColor;

	public static readonly Color HighlightTextBgColor;

	private static readonly Texture2D DefaultBarBgTex;

	public static readonly Texture2D BarFullTexHor;

	public static readonly Texture2D CheckboxOnTex;

	public static readonly Texture2D CheckboxOffTex;

	public static readonly Texture2D CheckboxPartialTex;

	public const float CheckboxSize = 24f;

	public const float RadioButtonSize = 24f;

	public static readonly Texture2D RadioButOnTex;

	public static readonly Texture2D HSVColorWheelTex;

	public static readonly Texture2D ColorSelectionCircle;

	public static readonly Texture2D ColorTemperatureExp;

	public static readonly Texture2D SelectionArrow;

	private static readonly Texture2D RadioButOffTex;

	private static readonly Texture2D FillArrowTexRight;

	private static readonly Texture2D FillArrowTexLeft;

	public static readonly Texture2D PlaceholderIconTex;

	private const int FillableBarBorderWidth = 3;

	private const int MaxFillChangeArrowHeight = 16;

	private const int FillChangeArrowWidth = 8;

	public const float CloseButtonSize = 18f;

	public const float CloseButtonMargin = 4f;

	public const float BackButtonWidth = 120f;

	public const float BackButtonHeight = 40f;

	public const float BackButtonMargin = 16f;

	private const float ColorHighlightCircleFraction = 0.125f;

	private const float ColorTextfieldHeight = 30f;

	private const float SelectionArrowSize = 12f;

	private static readonly Texture2D ShadowAtlas;

	public static readonly Texture2D ButtonBGAtlas;

	private static readonly Texture2D ButtonBGAtlasMouseover;

	public static readonly Texture2D ButtonBGAtlasClick;

	private static readonly Texture2D FloatRangeSliderTex;

	public static readonly Texture2D LightHighlight;

	private static readonly Rect DefaultTexCoords;

	private static readonly Rect LinkedTexCoords;

	[TweakValue("Input", 0f, 100f)]
	private static int IntEntryButtonWidth;

	private static Texture2D LineTexAA;

	private static readonly Texture2D AltTexture;

	public static readonly Color NormalOptionColor;

	public static readonly Color MouseoverOptionColor;

	private static Dictionary<string, float> LabelCache;

	private const float TileSize = 64f;

	public static readonly Color SeparatorLabelColor;

	public static readonly Color SeparatorLineColor;

	private const float SeparatorLabelHeight = 20f;

	public const float ListSeparatorHeight = 25f;

	private static bool checkboxPainting;

	private static bool checkboxPaintingState;

	public static readonly Texture2D ButtonSubtleAtlas;

	private static readonly Texture2D SliderRailAtlas;

	private static readonly Texture2D SliderHandle;

	private static readonly Texture2D ButtonBarTex;

	public const float ButtonSubtleDefaultMarginPct = 0.15f;

	private static int buttonInvisibleDraggable_activeControl;

	private static bool buttonInvisibleDraggable_dragged;

	private static Vector3 buttonInvisibleDraggable_mouseStart;

	private static int sliderDraggingID;

	private const float SliderHandleSize = 12f;

	public const float RangeControlIdealHeight = 31f;

	public const float RangeControlCompactHeight = 32f;

	private const float RangeSliderSize = 16f;

	private static readonly Color RangeControlTextColor;

	private static int draggingId;

	private static RangeEnd curDragEnd;

	private static float lastDragSliderSoundTime;

	private static float FillableBarChangeRateDisplayRatio;

	public static int MaxFillableBarChangeRate;

	private static readonly Color WindowBGBorderColor;

	public static readonly Color WindowBGFillColor;

	public static readonly Color MenuSectionBGFillColor;

	private static readonly Color MenuSectionBGBorderColor;

	private static readonly Color TutorWindowBGFillColor;

	private static readonly Color TutorWindowBGBorderColor;

	private static readonly Color OptionUnselectedBGFillColor;

	private static readonly Color OptionUnselectedBGBorderColor;

	private static readonly Color OptionSelectedBGFillColor;

	private static readonly Color OptionSelectedBGBorderColor;

	private static readonly Rect AtlasUV_TopLeft;

	private static readonly Rect AtlasUV_TopRight;

	private static readonly Rect AtlasUV_BottomLeft;

	private static readonly Rect AtlasUV_BottomRight;

	private static readonly Rect AtlasUV_Top;

	private static readonly Rect AtlasUV_Bottom;

	private static readonly Rect AtlasUV_Left;

	private static readonly Rect AtlasUV_Right;

	private static readonly Rect AtlasUV_Center;

	private static int[] maxColorComponentValues;

	private static string[] colorComponentLabels;

	private static string[] tmpTranslatedColorComponentLabels;

	private static int[] intColorComponents;

	public const float InfoCardButtonSize = 24f;

	private static bool dropdownPainting;

	private static object dropdownPainting_Payload;

	private static Type dropdownPainting_Type;

	private static string dropdownPainting_Text;

	private static Texture2D dropdownPainting_Icon;

	public static bool Painting
	{
		get
		{
			if (!dropdownPainting)
			{
				return checkboxPainting;
			}
			return true;
		}
	}

	static Widgets()
	{
		mouseOverScrollViewStack = new Stack<bool>();
		EmptyStyle = new GUIStyle();
		DragStartDistanceSquared = 20f;
		InactiveColor = new Color(0.37f, 0.37f, 0.37f, 0.8f);
		HighlightStrongBgColor = ColorLibrary.SkyBlue;
		HighlightTextBgColor = HighlightStrongBgColor.ToTransparent(0.25f);
		DefaultBarBgTex = BaseContent.BlackTex;
		BarFullTexHor = SolidColorMaterials.NewSolidColorTexture(new Color(0.2f, 0.8f, 0.85f));
		CheckboxOnTex = ContentFinder<Texture2D>.Get("UI/Widgets/CheckOn");
		CheckboxOffTex = ContentFinder<Texture2D>.Get("UI/Widgets/CheckOff");
		CheckboxPartialTex = ContentFinder<Texture2D>.Get("UI/Widgets/CheckPartial");
		RadioButOnTex = ContentFinder<Texture2D>.Get("UI/Widgets/RadioButOn");
		HSVColorWheelTex = ContentFinder<Texture2D>.Get("UI/Widgets/HSVColorWheel");
		ColorSelectionCircle = ContentFinder<Texture2D>.Get("UI/Overlays/TargetHighlight_Square");
		ColorTemperatureExp = ContentFinder<Texture2D>.Get("UI/Widgets/ColorTemperatureExp");
		SelectionArrow = ContentFinder<Texture2D>.Get("Things/Mote/InteractionArrow");
		RadioButOffTex = ContentFinder<Texture2D>.Get("UI/Widgets/RadioButOff");
		FillArrowTexRight = ContentFinder<Texture2D>.Get("UI/Widgets/FillChangeArrowRight");
		FillArrowTexLeft = ContentFinder<Texture2D>.Get("UI/Widgets/FillChangeArrowLeft");
		PlaceholderIconTex = ContentFinder<Texture2D>.Get("UI/Icons/MenuOptionNoIcon");
		ShadowAtlas = ContentFinder<Texture2D>.Get("UI/Widgets/DropShadow");
		ButtonBGAtlas = ContentFinder<Texture2D>.Get("UI/Widgets/ButtonBG");
		ButtonBGAtlasMouseover = ContentFinder<Texture2D>.Get("UI/Widgets/ButtonBGMouseover");
		ButtonBGAtlasClick = ContentFinder<Texture2D>.Get("UI/Widgets/ButtonBGClick");
		FloatRangeSliderTex = ContentFinder<Texture2D>.Get("UI/Widgets/RangeSlider");
		LightHighlight = SolidColorMaterials.NewSolidColorTexture(new Color(1f, 1f, 1f, 0.04f));
		DefaultTexCoords = new Rect(0f, 0f, 1f, 1f);
		LinkedTexCoords = new Rect(0f, 0.5f, 0.25f, 0.25f);
		IntEntryButtonWidth = 40;
		LineTexAA = null;
		AltTexture = SolidColorMaterials.NewSolidColorTexture(new Color(1f, 1f, 1f, 0.05f));
		NormalOptionColor = new Color(0.8f, 0.85f, 1f);
		MouseoverOptionColor = Color.yellow;
		LabelCache = new Dictionary<string, float>();
		SeparatorLabelColor = new Color(0.8f, 0.8f, 0.8f, 1f);
		SeparatorLineColor = new Color(0.3f, 0.3f, 0.3f, 1f);
		checkboxPainting = false;
		checkboxPaintingState = false;
		ButtonSubtleAtlas = ContentFinder<Texture2D>.Get("UI/Widgets/ButtonSubtleAtlas");
		SliderRailAtlas = ContentFinder<Texture2D>.Get("UI/Buttons/SliderRail");
		SliderHandle = ContentFinder<Texture2D>.Get("UI/Buttons/SliderHandle");
		ButtonBarTex = SolidColorMaterials.NewSolidColorTexture(TexUI.FinishedResearchColorTransparent);
		buttonInvisibleDraggable_activeControl = 0;
		buttonInvisibleDraggable_dragged = false;
		buttonInvisibleDraggable_mouseStart = Vector3.zero;
		RangeControlTextColor = new Color(0.6f, 0.6f, 0.6f);
		draggingId = 0;
		curDragEnd = RangeEnd.None;
		lastDragSliderSoundTime = -1f;
		FillableBarChangeRateDisplayRatio = 100000000f;
		MaxFillableBarChangeRate = 3;
		WindowBGBorderColor = new ColorInt(97, 108, 122).ToColor;
		WindowBGFillColor = new ColorInt(21, 25, 29).ToColor;
		MenuSectionBGFillColor = new ColorInt(42, 43, 44).ToColor;
		MenuSectionBGBorderColor = new ColorInt(135, 135, 135).ToColor;
		TutorWindowBGFillColor = new ColorInt(133, 85, 44).ToColor;
		TutorWindowBGBorderColor = new ColorInt(176, 139, 61).ToColor;
		OptionUnselectedBGFillColor = new Color(0.21f, 0.21f, 0.21f);
		OptionUnselectedBGBorderColor = OptionUnselectedBGFillColor * 1.8f;
		OptionSelectedBGFillColor = new Color(0.32f, 0.28f, 0.21f);
		OptionSelectedBGBorderColor = OptionSelectedBGFillColor * 1.8f;
		AtlasUV_TopLeft = new Rect(0f, 0f, 0.25f, 0.25f);
		AtlasUV_TopRight = new Rect(0.75f, 0f, 0.25f, 0.25f);
		AtlasUV_BottomLeft = new Rect(0f, 0.75f, 0.25f, 0.25f);
		AtlasUV_BottomRight = new Rect(0.75f, 0.75f, 0.25f, 0.25f);
		AtlasUV_Top = new Rect(0.25f, 0f, 0.5f, 0.25f);
		AtlasUV_Bottom = new Rect(0.25f, 0.75f, 0.5f, 0.25f);
		AtlasUV_Left = new Rect(0f, 0.25f, 0.25f, 0.5f);
		AtlasUV_Right = new Rect(0.75f, 0.25f, 0.25f, 0.5f);
		AtlasUV_Center = new Rect(0.25f, 0.25f, 0.5f, 0.5f);
		maxColorComponentValues = new int[6] { 255, 255, 255, 360, 100, 100 };
		colorComponentLabels = new string[6] { "Red", "Green", "Blue", "Hue", "Saturation", "ColorValue" };
		tmpTranslatedColorComponentLabels = new string[6];
		intColorComponents = new int[6];
		dropdownPainting = false;
		dropdownPainting_Payload = null;
		dropdownPainting_Type = null;
		dropdownPainting_Text = "";
		dropdownPainting_Icon = null;
		Color color = new Color(1f, 1f, 1f, 0f);
		LineTexAA = new Texture2D(1, 3, TextureFormat.ARGB32, mipChain: false);
		LineTexAA.name = "LineTexAA";
		LineTexAA.SetPixel(0, 0, color);
		LineTexAA.SetPixel(0, 1, Color.white);
		LineTexAA.SetPixel(0, 2, color);
		LineTexAA.Apply();
	}

	public static void BeginGroup(Rect rect)
	{
		GUI.BeginGroup(rect);
		UnityGUIBugsFixer.Notify_BeginGroup();
	}

	public static void EndGroup()
	{
		GUI.EndGroup();
		UnityGUIBugsFixer.Notify_EndGroup();
	}

	public static void ClearLabelCache()
	{
		LabelCache.Clear();
	}

	public static bool CanDrawIconFor(Def def)
	{
		if (def is BuildableDef buildableDef)
		{
			return buildableDef.uiIcon != null;
		}
		if (def is FactionDef factionDef)
		{
			return factionDef.FactionIcon != null;
		}
		return false;
	}

	public static void DefIcon(Rect rect, Def def, ThingDef stuffDef = null, float scale = 1f, ThingStyleDef thingStyleDef = null, bool drawPlaceholder = false, Color? color = null, Material material = null, int? graphicIndexOverride = null, float alpha = 1f)
	{
		if (def is BuildableDef buildableDef)
		{
			rect.position += new Vector2(buildableDef.uiIconOffset.x * rect.size.x, buildableDef.uiIconOffset.y * rect.size.y);
		}
		if (def is ThingDef { IsFrame: not false, entityDefToBuild: not null } thingDef)
		{
			def = thingDef.entityDefToBuild;
		}
		if (def is ThingDef thingDef2)
		{
			ThingIcon(rect, thingDef2, stuffDef, thingStyleDef, scale, color, graphicIndexOverride, alpha);
		}
		else if (def is PawnKindDef pawnKindDef)
		{
			ThingIcon(rect, pawnKindDef.race, stuffDef, thingStyleDef, scale, color, graphicIndexOverride, alpha);
		}
		else if (def is RecipeDef recipeDef && (recipeDef.UIIconThing != null || recipeDef.UIIcon != null))
		{
			if (recipeDef.UIIconThing != null)
			{
				ThingIcon(rect, recipeDef.UIIconThing, null, thingStyleDef, scale, color, graphicIndexOverride, alpha);
			}
			else if (recipeDef.UIIcon != null)
			{
				DrawTextureFitted(rect, recipeDef.UIIcon, scale, material, alpha);
			}
		}
		else if (def is TerrainDef terrainDef && terrainDef.uiIcon != null)
		{
			GUI.color = terrainDef.uiIconColor;
			Rect texCoords = (terrainDef.cropIcon ? CroppedTerrainTextureRect(terrainDef.uiIcon) : new Rect(0f, 0f, 1f, 1f));
			DrawTextureFitted(rect, terrainDef.uiIcon, scale, Vector2.one, texCoords, 0f, material, alpha);
			GUI.color = Color.white;
		}
		else if (def is FactionDef factionDef)
		{
			if (!factionDef.colorSpectrum.NullOrEmpty())
			{
				GUI.color = factionDef.colorSpectrum.FirstOrDefault();
			}
			DrawTextureFitted(rect, factionDef.FactionIcon, scale, material, alpha);
			GUI.color = Color.white;
		}
		else if (def is StyleItemDef styleItemDef)
		{
			DrawTextureFitted(rect, styleItemDef.Icon, scale, material, alpha);
		}
		else if (def is BodyTypeDef bodyTypeDef)
		{
			DrawTextureFitted(rect, bodyTypeDef.Icon, scale, material, alpha);
		}
		else if (def is HeadTypeDef headTypeDef)
		{
			DrawTextureFitted(rect, headTypeDef.Icon, scale, material, alpha);
		}
		else if (def is GeneDef geneDef)
		{
			GUI.color = color ?? geneDef.IconColor;
			DrawTextureFitted(rect, geneDef.Icon, scale, material, alpha);
			GUI.color = Color.white;
		}
		else if (def is XenotypeDef xenotypeDef)
		{
			GUI.color = color ?? XenotypeDef.IconColor;
			DrawTextureFitted(rect, xenotypeDef.Icon, scale, material, alpha);
			GUI.color = Color.white;
		}
		else if (def is PsychicRitualDef psychicRitualDef)
		{
			DrawTextureFitted(rect, psychicRitualDef.uiIcon, scale, material, alpha);
		}
		else if (drawPlaceholder)
		{
			DrawTextureFitted(rect, PlaceholderIconTex, scale, material, alpha);
		}
	}

	public static void ThingIcon(Rect rect, Thing thing, float alpha = 1f, Rot4? rot = null, bool stackOfOne = false, float scale = 1f, bool grayscale = false)
	{
		thing = thing.GetInnerIfMinified();
		if (thing is Blueprint blueprint && blueprint.EntityToBuild() != null)
		{
			DefIcon(rect, blueprint.EntityToBuild(), blueprint.EntityToBuildStuff(), 1f, blueprint.EntityToBuildStyle(), drawPlaceholder: false, null, null, null, alpha);
			return;
		}
		float scale2;
		float angle;
		Vector2 iconProportions;
		Color color;
		Material material;
		Texture iconFor = GetIconFor(thing, new Vector2(rect.width, rect.height), rot, stackOfOne, out scale2, out angle, out iconProportions, out color, out material);
		if (thing is Frame { BuildDef: not null } frame)
		{
			iconFor = GetIconFor(frame.BuildDef, frame.Stuff, frame.StyleDef);
		}
		if (iconFor == null || iconFor == BaseContent.BadTex)
		{
			return;
		}
		GUI.color = color;
		ThingStyleDef styleDef = thing.StyleDef;
		if ((styleDef != null && styleDef.UIIcon != null) || !thing.def.uiIconPath.NullOrEmpty())
		{
			rect.position += new Vector2(thing.def.uiIconOffset.x * rect.size.x, thing.def.uiIconOffset.y * rect.size.y);
		}
		Material mat = material;
		if (grayscale)
		{
			MaterialRequest req = new MaterialRequest
			{
				shader = ShaderDatabase.GrayscaleGUI,
				color = color
			};
			if (material != null)
			{
				req.maskTex = (Texture2D)material.GetTexture(ShaderPropertyIDs.MaskTex);
				req.color = material.GetColor(ShaderPropertyIDs.Color);
				req.colorTwo = material.GetColor(ShaderPropertyIDs.ColorTwo);
			}
			else
			{
				req.maskTex = Texture2D.redTexture;
			}
			mat = MaterialPool.MatFrom(req);
		}
		ThingIconWorker(rect, thing.def, iconFor, angle, scale2 * scale, rot, mat, alpha);
		GUI.color = Color.white;
	}

	public static void ThingIcon(Rect rect, ThingDef thingDef, ThingDef stuffDef = null, ThingStyleDef thingStyleDef = null, float scale = 1f, Color? color = null, int? graphicIndexOverride = null, float alpha = 1f)
	{
		if (thingDef.uiIcon == null || thingDef.uiIcon == BaseContent.BadTex)
		{
			return;
		}
		Material material;
		Texture2D iconFor = GetIconFor(thingDef, out material, stuffDef, thingStyleDef, graphicIndexOverride);
		if (!(iconFor == null))
		{
			Color color2 = GUI.color;
			if (color.HasValue)
			{
				GUI.color = color.Value;
			}
			else if (stuffDef != null)
			{
				GUI.color = thingDef.GetColorForStuff(stuffDef);
			}
			else if (material != null)
			{
				GUI.color = Color.white;
			}
			else
			{
				GUI.color = (thingDef.MadeFromStuff ? thingDef.GetColorForStuff(GenStuff.DefaultStuffFor(thingDef)) : thingDef.uiIconColor);
			}
			scale = ((thingStyleDef == null) ? (scale * GenUI.IconDrawScale(thingDef)) : (scale * thingStyleDef.uiIconScale));
			float num = (float)iconFor.width / (float)iconFor.height;
			rect = ((num < 1f) ? rect.MiddlePart(num, 1f) : rect.MiddlePart(1f, num));
			Rect rect2 = rect;
			float uiIconAngle = thingDef.uiIconAngle;
			float scale2 = scale;
			Material mat = material;
			ThingIconWorker(rect2, thingDef, iconFor, uiIconAngle, scale2, null, mat, alpha);
			GUI.color = color2;
		}
	}

	public static Texture2D GetIconFor(ThingDef thingDef, ThingDef stuffDef = null, ThingStyleDef thingStyleDef = null, int? graphicIndexOverride = null)
	{
		Material material;
		return GetIconFor(thingDef, out material, stuffDef, thingStyleDef, graphicIndexOverride);
	}

	public static Texture2D GetIconFor(ThingDef thingDef, out Material material, ThingDef stuffDef = null, ThingStyleDef thingStyleDef = null, int? graphicIndexOverride = null)
	{
		if (thingDef.IsCorpse && thingDef.ingestible?.sourceDef != null)
		{
			thingDef = thingDef.ingestible.sourceDef;
		}
		material = null;
		Texture2D result = thingDef.GetUIIconForStuff(stuffDef);
		if (thingStyleDef != null && thingStyleDef.UIIcon != null)
		{
			result = ((!graphicIndexOverride.HasValue) ? thingStyleDef.UIIcon : thingStyleDef.IconForIndex(graphicIndexOverride.Value));
		}
		else if (thingDef.graphic is Graphic_Appearances graphic_Appearances)
		{
			result = (Texture2D)graphic_Appearances.SubGraphicFor(stuffDef ?? GenStuff.DefaultStuffFor(thingDef)).MatAt(thingDef.defaultPlacingRot).mainTexture;
		}
		else if (thingDef.uiIconMaterial != null)
		{
			material = thingDef.uiIconMaterial;
		}
		return result;
	}

	private static Color GetDrawColor(BuildableDef buildable, ThingDef stuff)
	{
		if (buildable is ThingDef thingDef)
		{
			if (stuff != null)
			{
				return thingDef.GetColorForStuff(stuff);
			}
			if (thingDef.graphicData != null)
			{
				return thingDef.graphicData.color;
			}
		}
		else if (buildable is TerrainDef terrainDef)
		{
			return terrainDef.DrawColor;
		}
		return Color.white;
	}

	public static Texture GetIconFor(Thing thing, Vector2 size, Rot4? rot, bool stackOfOne, out float scale, out float angle, out Vector2 iconProportions, out Color color, out Material material)
	{
		if (thing == null)
		{
			scale = 1f;
			angle = 0f;
			iconProportions = Vector2.one;
			color = Color.white;
			material = null;
			return null;
		}
		material = null;
		thing = thing.GetInnerIfMinified();
		if (thing is Corpse corpse)
		{
			thing = corpse.InnerPawn;
		}
		Texture result = null;
		ThingStyleDef styleDef = thing.StyleDef;
		iconProportions = thing.DrawSize;
		color = thing.DrawColor;
		scale = GenUI.IconDrawScale(thing.def);
		if (thing is Blueprint blueprint)
		{
			color = GetDrawColor(blueprint.EntityToBuild(), blueprint.EntityToBuildStuff());
		}
		if (rot.HasValue && rot.Value.IsHorizontal)
		{
			iconProportions = new Vector2(iconProportions.y, iconProportions.x);
		}
		angle = 0f;
		if (thing.UIIconOverride != null)
		{
			result = thing.UIIconOverride;
			angle = thing.def.uiIconAngle;
		}
		else if (styleDef != null && styleDef.UIIcon != null)
		{
			Rot4 valueOrDefault = rot.GetValueOrDefault();
			if (!rot.HasValue)
			{
				valueOrDefault = thing.def.defaultPlacingRot;
				rot = valueOrDefault;
			}
			result = styleDef.IconForIndex(thing.OverrideGraphicIndex ?? thing.thingIDNumber, rot);
			angle = thing.def.uiIconAngle;
		}
		else if (!thing.def.uiIconPath.NullOrEmpty())
		{
			result = thing.def.uiIcon;
			angle = thing.def.uiIconAngle;
		}
		else if (thing is Pawn pawn)
		{
			if (!pawn.RaceProps.Humanlike)
			{
				Rot4 valueOrDefault = rot.GetValueOrDefault();
				if (!rot.HasValue)
				{
					valueOrDefault = Rot4.East;
					rot = valueOrDefault;
				}
				pawn.Drawer?.renderer?.EnsureGraphicsInitialized();
				Material material2 = pawn.Drawer?.renderer?.BodyGraphic?.MatAt(rot.Value);
				if (material2 != null)
				{
					result = material2.mainTexture;
					if (ShaderDatabase.TryGetUIShader(material2.shader, out var uiShader) && MaterialPool.TryGetRequestForMat(material2, out var request))
					{
						request.shader = uiShader;
						material = MaterialPool.MatFrom(request);
						color = Color.white;
					}
					else
					{
						color = material2.color;
					}
				}
			}
			else
			{
				Rot4 valueOrDefault = rot.GetValueOrDefault();
				if (!rot.HasValue)
				{
					valueOrDefault = Rot4.South;
					rot = valueOrDefault;
				}
				Rect r = new Rect(0f, 0f, size.x, size.y).ScaledBy(1.8f);
				r = r.Rounded();
				float num = 1.8f;
				if (ChildcareUtility.CanSuckle(pawn, out var _))
				{
					num = 3f;
				}
				else
				{
					r.y += 3f;
				}
				Vector2 size2 = new Vector2(r.width, r.height);
				Rot4 value = rot.Value;
				float cameraZoom = num;
				result = PortraitsCache.Get(pawn, size2, value, default(Vector3), cameraZoom);
			}
		}
		else
		{
			Rot4 valueOrDefault = rot.GetValueOrDefault();
			if (!rot.HasValue)
			{
				valueOrDefault = thing.def.defaultPlacingRot;
				rot = valueOrDefault;
			}
			Material material3 = (stackOfOne ? ((!(thing.Graphic is Graphic_StackCount graphic_StackCount) || thing.Graphic is Graphic_MealVariants) ? thing.Graphic.ExtractInnerGraphicFor(thing).MatAt(rot.Value) : graphic_StackCount.SubGraphicForStackCount(1, thing.def).MatSingleFor(thing)) : ((!(thing.Graphic is Graphic_Linked graphic_Linked)) ? thing.Graphic.ExtractInnerGraphicFor(thing).MatAt(rot.Value) : graphic_Linked.SubGraphic.ExtractInnerGraphicFor(thing).MatAt(rot.Value)));
			result = material3.mainTexture;
			if (ShaderDatabase.TryGetUIShader(material3.shader, out var uiShader2) && MaterialPool.TryGetRequestForMat(material3, out var request2))
			{
				request2.shader = uiShader2;
				material = MaterialPool.MatFrom(request2);
				color = Color.white;
			}
		}
		return result;
	}

	private static void ThingIconWorker(Rect rect, ThingDef thingDef, Texture resolvedIcon, float resolvedIconAngle, float scale = 1f, Rot4? rot = null, Material mat = null, float alpha = 1f)
	{
		Vector2 texProportions = new Vector2(resolvedIcon.width, resolvedIcon.height);
		Rect texCoords = DefaultTexCoords;
		Rot4 valueOrDefault = rot.GetValueOrDefault();
		if (!rot.HasValue)
		{
			valueOrDefault = thingDef.defaultPlacingRot;
			rot = valueOrDefault;
		}
		if (thingDef.graphicData != null)
		{
			texProportions = (rot.Value.IsHorizontal ? thingDef.graphicData.drawSize.Rotated() : thingDef.graphicData.drawSize);
			if (thingDef.uiIconPath.NullOrEmpty() && thingDef.graphicData.linkFlags != LinkFlags.None)
			{
				texCoords = LinkedTexCoords;
			}
		}
		DrawTextureFitted(rect, resolvedIcon, scale, texProportions, texCoords, resolvedIconAngle, mat, alpha);
	}

	public static Rect CroppedTerrainTextureRect(Texture2D tex)
	{
		return new Rect(0f, 0f, 64f / (float)tex.width, 64f / (float)tex.height);
	}

	public static void DrawAltRect(Rect rect)
	{
		GUI.DrawTexture(rect, AltTexture);
	}

	public static void ListSeparator(ref RectDivider divider, string label)
	{
		RectDivider rectDivider = divider.NewRow(25f);
		GUI.BeginGroup(rectDivider);
		float curY = 0f;
		ListSeparator(ref curY, rectDivider.Rect.width, label);
		GUI.EndGroup();
	}

	public static void ListSeparator(ref float curY, float width, string label)
	{
		Color color = GUI.color;
		curY += 3f;
		GUI.color = SeparatorLabelColor;
		Rect rect = new Rect(0f, curY, width, 30f);
		Text.Anchor = TextAnchor.UpperLeft;
		Label(rect, label);
		curY += 20f;
		GUI.color = SeparatorLineColor;
		DrawLineHorizontal(0f, curY, width);
		curY += 2f;
		GUI.color = color;
	}

	public static void DrawLine(Vector2 start, Vector2 end, Color color, float width)
	{
		float num = end.x - start.x;
		float num2 = end.y - start.y;
		float num3 = Mathf.Sqrt(num * num + num2 * num2);
		if (!(num3 < 0.01f))
		{
			width *= 3f;
			float num4 = width * num2 / num3;
			float num5 = width * num / num3;
			float z = (0f - Mathf.Atan2(0f - num2, num)) * 57.29578f;
			Vector2 vector = start + new Vector2(0.5f * num4, -0.5f * num5);
			Matrix4x4 m = Matrix4x4.TRS(vector, Quaternion.Euler(0f, 0f, z), Vector3.one) * Matrix4x4.TRS(-vector, Quaternion.identity, Vector3.one);
			Rect position = new Rect(start.x, start.y - 0.5f * num5, num3, width);
			GL.PushMatrix();
			GL.MultMatrix(m);
			GUI.DrawTexture(position, LineTexAA, ScaleMode.StretchToFill, alphaBlend: true, 0f, color, 0f, 0f);
			GL.PopMatrix();
		}
	}

	public static void DrawLineHorizontal(float x, float y, float length, Color color)
	{
		DrawBoxSolid(new Rect(x, y, length, 1f), color);
	}

	public static void DrawLineHorizontal(float x, float y, float length)
	{
		GUI.DrawTexture(new Rect(x, y, length, 1f), BaseContent.WhiteTex);
	}

	public static void DrawLineVertical(float x, float y, float length)
	{
		GUI.DrawTexture(new Rect(x, y, 1f, length), BaseContent.WhiteTex);
	}

	public static void DrawBoxSolid(Rect rect, Color color)
	{
		Color color2 = GUI.color;
		GUI.color = color;
		GUI.DrawTexture(rect, BaseContent.WhiteTex);
		GUI.color = color2;
	}

	public static void DrawBoxSolidWithOutline(Rect rect, Color solidColor, Color outlineColor, int outlineThickness = 1)
	{
		DrawBoxSolid(rect, solidColor);
		Color color = GUI.color;
		GUI.color = outlineColor;
		DrawBox(rect, outlineThickness);
		GUI.color = color;
	}

	public static void DrawBox(Rect rect, int thickness = 1, Texture2D lineTexture = null)
	{
		Vector2 vector = new Vector2(rect.x, rect.y);
		Vector2 vector2 = new Vector2(rect.x + rect.width, rect.y + rect.height);
		if (vector.x > vector2.x)
		{
			ref float x = ref vector.x;
			ref float x2 = ref vector2.x;
			float x3 = vector2.x;
			float x4 = vector.x;
			x = x3;
			x2 = x4;
		}
		if (vector.y > vector2.y)
		{
			ref float x = ref vector.y;
			ref float y = ref vector2.y;
			float x4 = vector2.y;
			float x3 = vector.y;
			x = x4;
			y = x3;
		}
		Vector3 vector3 = vector2 - vector;
		GUI.DrawTexture(UIScaling.AdjustRectToUIScaling(new Rect(vector.x, vector.y, thickness, vector3.y)), lineTexture ?? BaseContent.WhiteTex);
		GUI.DrawTexture(UIScaling.AdjustRectToUIScaling(new Rect(vector2.x - (float)thickness, vector.y, thickness, vector3.y)), lineTexture ?? BaseContent.WhiteTex);
		GUI.DrawTexture(UIScaling.AdjustRectToUIScaling(new Rect(vector.x + (float)thickness, vector.y, vector3.x - (float)(thickness * 2), thickness)), lineTexture ?? BaseContent.WhiteTex);
		GUI.DrawTexture(UIScaling.AdjustRectToUIScaling(new Rect(vector.x + (float)thickness, vector2.y - (float)thickness, vector3.x - (float)(thickness * 2), thickness)), lineTexture ?? BaseContent.WhiteTex);
	}

	public static void LabelCacheHeight(ref Rect rect, string label, bool renderLabel = true, bool forceInvalidation = false)
	{
		bool flag = LabelCache.ContainsKey(label);
		float num = 0f;
		if (forceInvalidation)
		{
			flag = false;
		}
		num = ((!flag) ? Text.CalcHeight(label, rect.width) : LabelCache[label]);
		rect.height = num;
		if (renderLabel)
		{
			Label(rect, label);
		}
	}

	public static void Label(Rect rect, GUIContent content)
	{
		GUI.Label(rect, content, Text.CurFontStyle);
	}

	public static void LabelEllipses(Rect rect, string label)
	{
		label = Text.ClampTextWithEllipsis(rect, label);
		Label(rect, label);
	}

	public static void Label(Rect rect, string label)
	{
		Rect position = rect;
		float num = Prefs.UIScale / 2f;
		if (Prefs.UIScale > 1f && Math.Abs(num - Mathf.Floor(num)) > float.Epsilon)
		{
			position.xMin = UIScaling.AdjustCoordToUIScalingFloor(rect.xMin);
			position.yMin = UIScaling.AdjustCoordToUIScalingFloor(rect.yMin);
			position.xMax = UIScaling.AdjustCoordToUIScalingCeil(rect.xMax + 1E-05f);
			position.yMax = UIScaling.AdjustCoordToUIScalingCeil(rect.yMax + 1E-05f);
		}
		GUI.Label(position, label, Text.CurFontStyle);
	}

	public static void Label(Rect rect, TaggedString label)
	{
		Label(rect, label.Resolve());
	}

	public static void Label(float x, ref float curY, float width, string text, TipSignal tip = default(TipSignal))
	{
		if (!text.NullOrEmpty())
		{
			float num = Text.CalcHeight(text, width);
			Rect rect = new Rect(x, curY, width, num);
			if (!tip.text.NullOrEmpty() || tip.textGetter != null)
			{
				float x2 = Text.CalcSize(text).x;
				Rect rect2 = new Rect(rect.x, rect.y, x2, num);
				DrawHighlightIfMouseover(rect2);
				TooltipHandler.TipRegion(rect2, tip);
			}
			Label(rect, text);
			curY += num;
		}
	}

	public static void Label(Rect rect, ref float y, string text, TipSignal tip = default(TipSignal))
	{
		if (!text.NullOrEmpty())
		{
			Label(rect.x, ref y, rect.width, text, tip);
		}
	}

	public static void LongLabel(float x, float width, string label, ref float curY, bool draw = true)
	{
		if (label.Length < 2500)
		{
			if (draw)
			{
				Label(new Rect(x, curY, width, 1000f), label);
			}
			curY += Text.CalcHeight(label, width);
			return;
		}
		int num = 0;
		int num2 = -1;
		bool flag = false;
		for (int i = 0; i < label.Length; i++)
		{
			if (label[i] != '\n')
			{
				continue;
			}
			num++;
			if (num >= 50)
			{
				string text = label.Substring(num2 + 1, i - num2 - 1);
				num2 = i;
				num = 0;
				if (flag)
				{
					curY += Text.SpaceBetweenLines;
				}
				if (draw)
				{
					Label(new Rect(x, curY, width, 10000f), text);
				}
				curY += Text.CalcHeight(text, width);
				flag = true;
			}
		}
		if (num2 != label.Length - 1)
		{
			if (flag)
			{
				curY += Text.SpaceBetweenLines;
			}
			string text2 = label.Substring(num2 + 1);
			if (draw)
			{
				Label(new Rect(x, curY, width, 10000f), text2);
			}
			curY += Text.CalcHeight(text2, width);
			flag = true;
		}
	}

	public static void LabelScrollable(Rect rect, string label, ref Vector2 scrollbarPosition, bool dontConsumeScrollEventsIfNoScrollbar = false, bool takeScrollbarSpaceEvenIfNoScrollbar = true, bool longLabel = false)
	{
		int num;
		int num2;
		if (!takeScrollbarSpaceEvenIfNoScrollbar)
		{
			num = ((Text.CalcHeight(label, rect.width) > rect.height) ? 1 : 0);
			if (num == 0)
			{
				num2 = 0;
				goto IL_0045;
			}
		}
		else
		{
			num = 1;
		}
		num2 = ((!dontConsumeScrollEventsIfNoScrollbar || Text.CalcHeight(label, rect.width - 16f) > rect.height) ? 1 : 0);
		goto IL_0045;
		IL_0045:
		bool flag = (byte)num2 != 0;
		float num3 = rect.width;
		if (num != 0)
		{
			num3 -= 16f;
		}
		float curY;
		if (longLabel)
		{
			curY = 0f;
			LongLabel(0f, num3, label, ref curY, draw: false);
		}
		else
		{
			curY = Text.CalcHeight(label, num3);
		}
		Rect rect2 = new Rect(0f, 0f, num3, Mathf.Max(curY + 5f, rect.height));
		if (flag)
		{
			BeginScrollView(rect, ref scrollbarPosition, rect2);
		}
		else
		{
			BeginGroup(rect);
		}
		if (longLabel)
		{
			float curY2 = rect2.y;
			LongLabel(rect2.x, rect2.width, label, ref curY2);
		}
		else
		{
			Label(rect2, label);
		}
		if (flag)
		{
			EndScrollView();
		}
		else
		{
			EndGroup();
		}
	}

	public static void LabelWithIcon(Rect rect, string label, Texture2D labelIcon, float labelIconScale = 1f)
	{
		float num = Mathf.Min(labelIcon.width, rect.height);
		Rect outerRect = new Rect(rect.x, rect.y, num, rect.height);
		rect.xMin += num;
		DrawTextureFitted(outerRect, labelIcon, labelIconScale);
		Label(rect, label);
	}

	public static void DefLabelWithIcon(Rect rect, Def def, float iconMargin = 2f, float textOffsetX = 6f)
	{
		DrawHighlightIfMouseover(rect);
		TooltipHandler.TipRegion(rect, def.description);
		BeginGroup(rect);
		Rect rect2 = new Rect(0f, 0f, rect.height, rect.height);
		if (iconMargin != 0f)
		{
			rect2 = rect2.ContractedBy(iconMargin);
		}
		DefIcon(rect2, def, null, 1f, null, drawPlaceholder: true);
		Rect rect3 = new Rect(rect2.xMax + textOffsetX, 0f, rect.width, rect.height);
		Text.Anchor = TextAnchor.MiddleLeft;
		Text.WordWrap = false;
		Label(rect3, def.LabelCap);
		Text.Anchor = TextAnchor.UpperLeft;
		Text.WordWrap = true;
		EndGroup();
	}

	public static bool LabelFit(Rect rect, string label)
	{
		bool result = false;
		GameFont font = Text.Font;
		Text.Font = GameFont.Small;
		if (Text.CalcSize(label).x <= rect.width)
		{
			Label(rect, label);
		}
		else
		{
			Text.Font = GameFont.Tiny;
			if (Text.CalcSize(label).x <= rect.width)
			{
				Label(rect, label);
			}
			else
			{
				LabelEllipses(rect, label);
				result = true;
			}
			Text.Font = GameFont.Small;
		}
		Text.Font = font;
		return result;
	}

	public static void HyperlinkWithIcon(Rect rect, Dialog_InfoCard.Hyperlink hyperlink, string text = null, float iconMargin = 2f, float textOffsetX = 6f, Color? color = null, bool truncateLabel = false, string textSuffix = null)
	{
		string text2 = text ?? hyperlink.Label.CapitalizeFirst();
		if (textSuffix != null)
		{
			text2 += textSuffix;
		}
		BeginGroup(rect);
		Rect rect2 = new Rect(0f, 0f, rect.height, rect.height);
		if (iconMargin != 0f)
		{
			rect2 = rect2.ContractedBy(iconMargin);
		}
		if (hyperlink.IsHidden)
		{
			DrawTextureFitted(rect2, PlaceholderIconTex, 1f);
		}
		else if (hyperlink.thing != null)
		{
			ThingIcon(rect2, hyperlink.thing);
		}
		else
		{
			DefIcon(rect2, hyperlink.def, null, 1f, null, drawPlaceholder: true);
		}
		float num = rect2.xMax + textOffsetX;
		Rect rect3 = new Rect(rect2.xMax + textOffsetX, 0f, rect.width - num, rect.height);
		Text.Anchor = TextAnchor.MiddleLeft;
		Text.WordWrap = false;
		Color textColor = color ?? NormalOptionColor;
		if (hyperlink.IsHidden)
		{
			textColor = Color.gray;
		}
		ButtonText(rect3, truncateLabel ? text2.Truncate(rect3.width) : text2, drawBackground: false, doMouseoverSound: false, textColor, active: false);
		if (ButtonInvisible(rect3))
		{
			hyperlink.ActivateHyperlink();
		}
		Text.Anchor = TextAnchor.UpperLeft;
		Text.WordWrap = true;
		EndGroup();
	}

	public static void DrawNumberOnMap(Vector2 screenPos, int number, Color textColor)
	{
		Text.Anchor = TextAnchor.MiddleCenter;
		Text.Font = GameFont.Medium;
		string text = number.ToStringCached();
		float val = Text.CalcSize(text).x + 8f;
		Rect rect = new Rect(screenPos.x - 20f, screenPos.y - 15f, Math.Max(40f, val), 30f);
		GUI.DrawTexture(rect, TexUI.GrayBg);
		GUI.color = textColor;
		Label(rect, text);
		GUI.color = Color.white;
		Text.Font = GameFont.Small;
		Text.Anchor = TextAnchor.UpperLeft;
	}

	public static void DrawStringOnMap(Vector2 screenPos, string str, Color textColor)
	{
		Text.Anchor = TextAnchor.MiddleCenter;
		Text.Font = GameFont.Medium;
		float num = Text.CalcSize(str).x + 8f;
		Rect rect = new Rect(screenPos.x - num / 2f, screenPos.y - 15f, num, 30f);
		GUI.DrawTexture(rect, TexUI.GrayBg);
		GUI.color = textColor;
		Label(rect, str);
		GUI.color = Color.white;
		Text.Font = GameFont.Small;
		Text.Anchor = TextAnchor.UpperLeft;
	}

	public static void Checkbox(Vector2 topLeft, ref bool checkOn, float size = 24f, bool disabled = false, bool paintable = false, Texture2D texChecked = null, Texture2D texUnchecked = null)
	{
		Checkbox(topLeft.x, topLeft.y, ref checkOn, size, disabled, paintable, texChecked, texUnchecked);
	}

	public static void Checkbox(float x, float y, ref bool checkOn, float size = 24f, bool disabled = false, bool paintable = false, Texture2D texChecked = null, Texture2D texUnchecked = null)
	{
		if (disabled)
		{
			GUI.color = InactiveColor;
		}
		Rect rect = new Rect(x, y, size, size);
		CheckboxDraw(x, y, checkOn, disabled, size, texChecked, texUnchecked);
		if (!disabled)
		{
			ToggleInvisibleDraggable(rect, ref checkOn, doMouseoverSound: true, paintable);
		}
		if (disabled)
		{
			GUI.color = Color.white;
		}
	}

	public static void CheckboxLabeled(Rect rect, string label, ref bool checkOn, bool disabled = false, Texture2D texChecked = null, Texture2D texUnchecked = null, bool placeCheckboxNearText = false, bool paintable = false)
	{
		TextAnchor anchor = Text.Anchor;
		Text.Anchor = TextAnchor.MiddleLeft;
		if (placeCheckboxNearText)
		{
			rect.width = Mathf.Min(rect.width, Text.CalcSize(label).x + 24f + 10f);
		}
		Rect rect2 = rect;
		rect2.xMax -= 24f;
		Label(rect2, label);
		if (!disabled)
		{
			ToggleInvisibleDraggable(rect, ref checkOn, doMouseoverSound: true, paintable);
		}
		CheckboxDraw(rect.x + rect.width - 24f, rect.y + (rect.height - 24f) / 2f, checkOn, disabled, 24f, texChecked, texUnchecked);
		Text.Anchor = anchor;
	}

	public static void ToggleInvisibleDraggable(Rect rect, ref bool checkOn, bool doMouseoverSound = false, bool paintable = false)
	{
		DraggableResult draggableResult = ButtonInvisibleDraggable(rect, doMouseoverSound);
		bool flag = false;
		if (draggableResult == DraggableResult.Pressed)
		{
			checkOn = !checkOn;
			flag = true;
		}
		else if (draggableResult == DraggableResult.Dragged && paintable)
		{
			checkOn = !checkOn;
			flag = true;
			checkboxPainting = true;
			checkboxPaintingState = checkOn;
		}
		if (paintable && Mouse.IsOver(rect) && checkboxPainting && Input.GetMouseButton(0) && checkOn != checkboxPaintingState)
		{
			checkOn = checkboxPaintingState;
			flag = true;
		}
		if (doMouseoverSound && flag)
		{
			if (checkOn)
			{
				SoundDefOf.Checkbox_TurnedOn.PlayOneShotOnCamera();
			}
			else
			{
				SoundDefOf.Checkbox_TurnedOff.PlayOneShotOnCamera();
			}
		}
	}

	public static bool CheckboxLabeledSelectable(Rect rect, string label, ref bool selected, ref bool checkOn, Texture2D labelIcon = null, float labelIconScale = 1f)
	{
		if (selected)
		{
			DrawHighlight(rect);
		}
		TextAnchor anchor = Text.Anchor;
		Text.Anchor = TextAnchor.MiddleLeft;
		if (labelIcon != null)
		{
			Rect outerRect = new Rect(rect.x, rect.y, labelIcon.width, rect.height);
			rect.xMin += labelIcon.width;
			DrawTextureFitted(outerRect, labelIcon, labelIconScale);
		}
		Label(rect, label);
		Text.Anchor = anchor;
		bool flag = selected;
		Rect butRect = rect;
		butRect.width -= 24f;
		if (!selected && ButtonInvisible(butRect))
		{
			SoundDefOf.Tick_Tiny.PlayOneShotOnCamera();
			selected = true;
		}
		Color color = GUI.color;
		GUI.color = Color.white;
		CheckboxDraw(rect.xMax - 24f, rect.y, checkOn, disabled: false);
		GUI.color = color;
		if (ButtonInvisible(new Rect(rect.xMax - 24f, rect.y, 24f, 24f)))
		{
			checkOn = !checkOn;
			if (checkOn)
			{
				SoundDefOf.Checkbox_TurnedOn.PlayOneShotOnCamera();
			}
			else
			{
				SoundDefOf.Checkbox_TurnedOff.PlayOneShotOnCamera();
			}
		}
		if (selected)
		{
			return !flag;
		}
		return false;
	}

	public static Texture2D GetCheckboxTexture(bool state)
	{
		if (state)
		{
			return CheckboxOnTex;
		}
		return CheckboxOffTex;
	}

	public static void CheckboxDraw(float x, float y, bool active, bool disabled, float size = 24f, Texture2D texChecked = null, Texture2D texUnchecked = null)
	{
		Color color = GUI.color;
		if (disabled)
		{
			GUI.color = InactiveColor;
		}
		GUI.DrawTexture(image: (!active) ? ((texUnchecked != null) ? texUnchecked : CheckboxOffTex) : ((texChecked != null) ? texChecked : CheckboxOnTex), position: new Rect(x, y, size, size));
		if (disabled)
		{
			GUI.color = color;
		}
	}

	public static MultiCheckboxState CheckboxMulti(Rect rect, MultiCheckboxState state, bool paintable = false)
	{
		Texture2D tex = state switch
		{
			MultiCheckboxState.On => CheckboxOnTex, 
			MultiCheckboxState.Off => CheckboxOffTex, 
			_ => CheckboxPartialTex, 
		};
		MouseoverSounds.DoRegion(rect);
		MultiCheckboxState multiCheckboxState = ((state != MultiCheckboxState.Off) ? MultiCheckboxState.Off : MultiCheckboxState.On);
		bool flag = false;
		DraggableResult draggableResult = ButtonImageDraggable(rect, tex);
		if (paintable && draggableResult == DraggableResult.Dragged)
		{
			checkboxPainting = true;
			checkboxPaintingState = multiCheckboxState == MultiCheckboxState.On;
			flag = true;
		}
		else if (draggableResult.AnyPressed())
		{
			flag = true;
		}
		else if (paintable && checkboxPainting && Mouse.IsOver(rect))
		{
			multiCheckboxState = ((!checkboxPaintingState) ? MultiCheckboxState.Off : MultiCheckboxState.On);
			if (state != multiCheckboxState)
			{
				flag = true;
			}
		}
		if (flag)
		{
			if (multiCheckboxState == MultiCheckboxState.On)
			{
				SoundDefOf.Checkbox_TurnedOn.PlayOneShotOnCamera();
			}
			else
			{
				SoundDefOf.Checkbox_TurnedOff.PlayOneShotOnCamera();
			}
			return multiCheckboxState;
		}
		return state;
	}

	public static bool RadioButton(Vector2 topLeft, bool chosen, bool disabled = false)
	{
		return RadioButton(topLeft.x, topLeft.y, chosen, disabled);
	}

	public static bool RadioButton(float x, float y, bool chosen, bool disabled = false)
	{
		Rect butRect = new Rect(x, y, 24f, 24f);
		RadioButtonDraw(x, y, chosen, disabled);
		bool num = ButtonInvisible(butRect);
		if (num && !chosen)
		{
			SoundDefOf.Tick_Tiny.PlayOneShotOnCamera();
		}
		return num;
	}

	public static bool RadioButtonLabeled(Rect rect, string labelText, bool chosen, bool disabled = false)
	{
		TextBlock textBlock = new TextBlock(TextAnchor.MiddleLeft, disabled ? ColoredText.SubtleGrayColor : Color.white);
		try
		{
			Label(rect, labelText);
		}
		finally
		{
			((IDisposable)textBlock/*cast due to .constrained prefix*/).Dispose();
		}
		bool num = ButtonInvisible(rect);
		if (num && !chosen && !disabled)
		{
			SoundDefOf.Tick_Tiny.PlayOneShotOnCamera();
		}
		RadioButtonDraw(rect.x + rect.width - 24f, rect.y + rect.height / 2f - 12f, chosen, disabled);
		return num;
	}

	private static void RadioButtonDraw(float x, float y, bool chosen, bool disabled)
	{
		Color color = GUI.color;
		GUI.color = Color.white;
		Texture2D image = ((!chosen) ? RadioButOffTex : RadioButOnTex);
		Rect position = new Rect(x, y, 24f, 24f);
		if (disabled)
		{
			GUI.color = Color.gray;
		}
		GUI.DrawTexture(position, image);
		GUI.color = color;
	}

	public static bool ButtonText(Rect rect, string label, bool drawBackground = true, bool doMouseoverSound = true, bool active = true, TextAnchor? overrideTextAnchor = null)
	{
		return ButtonText(rect, label, drawBackground, doMouseoverSound, NormalOptionColor, active, overrideTextAnchor);
	}

	public static bool ButtonText(Rect rect, string label, bool drawBackground, bool doMouseoverSound, Color textColor, bool active = true, TextAnchor? overrideTextAnchor = null)
	{
		return ButtonTextWorker(rect, label, drawBackground, doMouseoverSound, textColor, active, draggable: false, overrideTextAnchor).AnyPressed();
	}

	public static DraggableResult ButtonTextDraggable(Rect rect, string label, bool drawBackground = true, bool doMouseoverSound = false, bool active = true, TextAnchor? overrideTextAnchor = null)
	{
		return ButtonTextDraggable(rect, label, drawBackground, doMouseoverSound, NormalOptionColor, active, overrideTextAnchor);
	}

	public static DraggableResult ButtonTextDraggable(Rect rect, string label, bool drawBackground, bool doMouseoverSound, Color textColor, bool active = true, TextAnchor? overrideTextAnchor = null)
	{
		return ButtonTextWorker(rect, label, drawBackground, doMouseoverSound, NormalOptionColor, active, draggable: true, overrideTextAnchor);
	}

	public static void DrawButtonGraphic(Rect rect)
	{
		Texture2D atlas = ButtonBGAtlas;
		if (Mouse.IsOver(rect))
		{
			atlas = ButtonBGAtlasMouseover;
			if (Input.GetMouseButton(0))
			{
				atlas = ButtonBGAtlasClick;
			}
		}
		DrawAtlas(rect, atlas);
	}

	private static DraggableResult ButtonTextWorker(Rect rect, string label, bool drawBackground, bool doMouseoverSound, Color textColor, bool active, bool draggable, TextAnchor? overrideTextAnchor = null)
	{
		TextAnchor anchor = Text.Anchor;
		Color color = GUI.color;
		if (drawBackground)
		{
			DrawButtonGraphic(rect);
		}
		if (doMouseoverSound)
		{
			MouseoverSounds.DoRegion(rect);
		}
		if (!drawBackground)
		{
			GUI.color = textColor;
			if (Mouse.IsOver(rect))
			{
				GUI.color = MouseoverOptionColor;
			}
		}
		if (overrideTextAnchor.HasValue)
		{
			Text.Anchor = overrideTextAnchor.Value;
		}
		else if (drawBackground)
		{
			Text.Anchor = TextAnchor.MiddleCenter;
		}
		else
		{
			Text.Anchor = TextAnchor.MiddleLeft;
		}
		bool wordWrap = Text.WordWrap;
		if (rect.height < Text.LineHeight * 2f)
		{
			Text.WordWrap = false;
		}
		Label(rect, label);
		Text.Anchor = anchor;
		GUI.color = color;
		Text.WordWrap = wordWrap;
		if (active && draggable)
		{
			return ButtonInvisibleDraggable(rect);
		}
		if (active)
		{
			if (!ButtonInvisible(rect, doMouseoverSound: false))
			{
				return DraggableResult.Idle;
			}
			return DraggableResult.Pressed;
		}
		return DraggableResult.Idle;
	}

	public static void DrawRectFast(Rect position, Color color, GUIContent content = null)
	{
		Color backgroundColor = GUI.backgroundColor;
		GUI.backgroundColor = color;
		GUI.Box(position, content ?? GUIContent.none, TexUI.FastFillStyle);
		GUI.backgroundColor = backgroundColor;
	}

	public static bool CustomButtonText(ref Rect rect, string label, Color bgColor, Color textColor, Color borderColor, Color unfilledBgColor = default(Color), bool cacheHeight = false, float borderSize = 1f, bool doMouseoverSound = true, bool active = true, float fillPercent = 1f)
	{
		if (cacheHeight)
		{
			LabelCacheHeight(ref rect, label, renderLabel: false);
		}
		Rect position = new Rect(rect);
		position.x += borderSize;
		position.y += borderSize;
		position.width -= borderSize * 2f;
		position.height -= borderSize * 2f;
		DrawRectFast(rect, borderColor);
		if (unfilledBgColor != default(Color))
		{
			DrawRectFast(position, unfilledBgColor);
		}
		position.width *= fillPercent;
		DrawRectFast(position, bgColor);
		TextAnchor anchor = Text.Anchor;
		Color color = GUI.color;
		if (doMouseoverSound)
		{
			MouseoverSounds.DoRegion(rect);
		}
		GUI.color = textColor;
		if (Mouse.IsOver(rect))
		{
			GUI.color = MouseoverOptionColor;
		}
		Text.Anchor = TextAnchor.MiddleCenter;
		Label(rect, label);
		Text.Anchor = anchor;
		GUI.color = color;
		if (active)
		{
			return ButtonInvisible(rect, doMouseoverSound: false);
		}
		return false;
	}

	public static bool ButtonTextSubtle(Rect rect, string label, float barPercent = 0f, float textLeftMargin = -1f, SoundDef mouseoverSound = null, Vector2 functionalSizeOffset = default(Vector2), Color? labelColor = null, bool highlight = false)
	{
		Rect rect2 = rect;
		rect2.width += functionalSizeOffset.x;
		rect2.height += functionalSizeOffset.y;
		bool flag = false;
		if (Mouse.IsOver(rect2))
		{
			flag = true;
			GUI.color = GenUI.MouseoverColor;
		}
		if (mouseoverSound != null)
		{
			MouseoverSounds.DoRegion(rect2, mouseoverSound);
		}
		DrawAtlas(rect, ButtonSubtleAtlas);
		if (highlight)
		{
			GUI.color = Color.grey;
			DrawBox(rect, 2);
		}
		GUI.color = Color.white;
		if (barPercent > 0.001f)
		{
			FillableBar(rect.ContractedBy(1f), barPercent, ButtonBarTex, null, doBorder: false);
		}
		Rect rect3 = new Rect(rect);
		if (textLeftMargin < 0f)
		{
			textLeftMargin = rect.width * 0.15f;
		}
		rect3.x += textLeftMargin;
		if (flag)
		{
			rect3.x += 2f;
			rect3.y -= 2f;
		}
		Text.Anchor = TextAnchor.MiddleLeft;
		Text.WordWrap = false;
		Text.Font = GameFont.Small;
		GUI.color = labelColor ?? Color.white;
		Label(rect3, label);
		Text.Anchor = TextAnchor.UpperLeft;
		Text.WordWrap = true;
		GUI.color = Color.white;
		return ButtonInvisible(rect2, doMouseoverSound: false);
	}

	public static bool ButtonImage(Rect butRect, Texture2D tex, bool doMouseoverSound = true, string tooltip = null)
	{
		return ButtonImage(butRect, tex, Color.white, doMouseoverSound, tooltip);
	}

	public static bool ButtonImage(Rect butRect, Texture2D tex, Color baseColor, bool doMouseoverSound = true, string tooltip = null)
	{
		return ButtonImage(butRect, tex, baseColor, GenUI.MouseoverColor, doMouseoverSound, tooltip);
	}

	public static bool ButtonImage(Rect butRect, Texture2D tex, Color baseColor, Color mouseoverColor, bool doMouseoverSound = true, string tooltip = null)
	{
		GUI.color = (Mouse.IsOver(butRect) ? mouseoverColor : baseColor);
		GUI.DrawTexture(butRect, tex);
		GUI.color = baseColor;
		if (!tooltip.NullOrEmpty())
		{
			TooltipHandler.TipRegion(butRect, tooltip);
		}
		bool result = ButtonInvisible(butRect, doMouseoverSound);
		GUI.color = Color.white;
		return result;
	}

	public static DraggableResult ButtonImageDraggable(Rect butRect, Texture2D tex)
	{
		return ButtonImageDraggable(butRect, tex, Color.white);
	}

	public static DraggableResult ButtonImageDraggable(Rect butRect, Texture2D tex, Color baseColor)
	{
		return ButtonImageDraggable(butRect, tex, baseColor, GenUI.MouseoverColor);
	}

	public static DraggableResult ButtonImageDraggable(Rect butRect, Texture2D tex, Color baseColor, Color mouseoverColor)
	{
		if (Mouse.IsOver(butRect))
		{
			GUI.color = mouseoverColor;
		}
		else
		{
			GUI.color = baseColor;
		}
		GUI.DrawTexture(butRect, tex);
		GUI.color = baseColor;
		return ButtonInvisibleDraggable(butRect);
	}

	public static bool ButtonImageFitted(Rect butRect, Texture2D tex)
	{
		return ButtonImageFitted(butRect, tex, Color.white);
	}

	public static bool ButtonImageFitted(Rect butRect, Texture2D tex, Color baseColor)
	{
		return ButtonImageFitted(butRect, tex, baseColor, GenUI.MouseoverColor);
	}

	public static bool ButtonImageFitted(Rect butRect, Texture2D tex, Color baseColor, Color mouseoverColor)
	{
		if (Mouse.IsOver(butRect))
		{
			GUI.color = mouseoverColor;
		}
		else
		{
			GUI.color = baseColor;
		}
		DrawTextureFitted(butRect, tex, 1f);
		GUI.color = baseColor;
		return ButtonInvisible(butRect);
	}

	public static bool ButtonImageWithBG(Rect butRect, Texture2D image, Vector2? imageSize = null)
	{
		bool result = ButtonText(butRect, "");
		Rect position = ((!imageSize.HasValue) ? butRect : new Rect(Mathf.Floor(butRect.x + butRect.width / 2f - imageSize.Value.x / 2f), Mathf.Floor(butRect.y + butRect.height / 2f - imageSize.Value.y / 2f), imageSize.Value.x, imageSize.Value.y));
		GUI.DrawTexture(position, image);
		return result;
	}

	public static bool CloseButtonFor(Rect rectToClose)
	{
		return ButtonImage(new Rect(rectToClose.x + rectToClose.width - 18f - 4f, rectToClose.y + 4f, 18f, 18f), TexButton.CloseXSmall);
	}

	public static bool BackButtonFor(Rect rectToBack)
	{
		return ButtonText(new Rect(rectToBack.x + rectToBack.width - 18f - 4f - 120f - 16f, rectToBack.y + 18f, 120f, 40f), "Back".Translate());
	}

	public static bool ButtonInvisible(Rect butRect, bool doMouseoverSound = true)
	{
		if (doMouseoverSound)
		{
			MouseoverSounds.DoRegion(butRect);
		}
		return GUI.Button(butRect, "", EmptyStyle);
	}

	public static DraggableResult ButtonInvisibleDraggable(Rect butRect, bool doMouseoverSound = false)
	{
		if (doMouseoverSound)
		{
			MouseoverSounds.DoRegion(butRect);
		}
		int controlID = GUIUtility.GetControlID(FocusType.Passive, butRect);
		if (Input.GetMouseButtonDown(0) && Mouse.IsOver(butRect))
		{
			GUIUtility.keyboardControl = 0;
			buttonInvisibleDraggable_activeControl = controlID;
			buttonInvisibleDraggable_mouseStart = Input.mousePosition;
			buttonInvisibleDraggable_dragged = false;
		}
		if (buttonInvisibleDraggable_activeControl == controlID)
		{
			if (Input.GetMouseButtonUp(0))
			{
				buttonInvisibleDraggable_activeControl = 0;
				if (Mouse.IsOver(butRect))
				{
					if (!buttonInvisibleDraggable_dragged)
					{
						return DraggableResult.Pressed;
					}
					return DraggableResult.DraggedThenPressed;
				}
				return DraggableResult.Idle;
			}
			if (!Input.GetMouseButton(0))
			{
				buttonInvisibleDraggable_activeControl = 0;
				return DraggableResult.Idle;
			}
			if (!buttonInvisibleDraggable_dragged && (buttonInvisibleDraggable_mouseStart - Input.mousePosition).sqrMagnitude > DragStartDistanceSquared)
			{
				buttonInvisibleDraggable_dragged = true;
				return DraggableResult.Dragged;
			}
		}
		return DraggableResult.Idle;
	}

	public static string TextField(Rect rect, string text)
	{
		if (text == null)
		{
			text = "";
		}
		return GUI.TextField(rect, text, Text.CurTextFieldStyle);
	}

	public static string TextField(Rect rect, string text, int maxLength, Regex inputValidator = null)
	{
		string text2 = TextField(rect, text);
		if (text2.Length <= maxLength && (inputValidator == null || inputValidator.IsMatch(text2)))
		{
			return text2;
		}
		return text;
	}

	public static string TextArea(Rect rect, string text, bool readOnly = false)
	{
		if (text == null)
		{
			text = "";
		}
		return GUI.TextArea(rect, text, readOnly ? Text.CurTextAreaReadOnlyStyle : Text.CurTextAreaStyle);
	}

	public static string TextEntryLabeled(Rect rect, string label, string text)
	{
		Rect rect2 = rect.LeftHalf().Rounded();
		Rect rect3 = rect.RightHalf().Rounded();
		TextAnchor anchor = Text.Anchor;
		Text.Anchor = TextAnchor.MiddleRight;
		Label(rect2, label);
		Text.Anchor = anchor;
		if (rect.height <= 30f)
		{
			return TextField(rect3, text);
		}
		return TextArea(rect3, text);
	}

	public static string DelayedTextField(Rect rect, string text, ref string buffer, string previousFocusedControlName, string controlName = null)
	{
		controlName = controlName ?? $"TextField{rect.x},{rect.y}";
		bool num = previousFocusedControlName == controlName;
		bool flag = GUI.GetNameOfFocusedControl() == controlName;
		string text2 = controlName + "_unfocused";
		GUI.SetNextControlName(text2);
		GUI.Label(rect, "");
		GUI.SetNextControlName(controlName);
		bool flag2 = false;
		if (flag && Event.current.type == EventType.KeyDown && (Event.current.keyCode == KeyCode.Return || Event.current.keyCode == KeyCode.KeypadEnter))
		{
			Event.current.Use();
			flag2 = true;
		}
		bool flag3 = false;
		if (Event.current.type == EventType.MouseDown && !rect.Contains(Event.current.mousePosition))
		{
			flag3 = true;
		}
		if (num)
		{
			buffer = TextField(rect, buffer);
			if (!flag)
			{
				return buffer;
			}
			if (flag3 || flag2)
			{
				GUI.FocusControl(text2);
				return buffer;
			}
			return text;
		}
		buffer = TextField(rect, text);
		return buffer;
	}

	public static void TextFieldVector(Rect rect, ref Vector3 vector, ref string[] buffer, float min = 0f, float max = 1E+09f)
	{
		if (buffer == null)
		{
			buffer = new string[3];
		}
		float width = rect.width / 3f - 4f;
		Rect rect2 = rect.LeftPartPixels(width);
		Rect rect3 = rect2;
		Rect rect4 = rect3;
		rect3.x = rect2.xMax + 4f;
		rect4.x = rect3.xMax + 4f;
		TextFieldNumeric(rect2, ref vector.x, ref buffer[0], min, max);
		TextFieldNumeric(rect3, ref vector.y, ref buffer[1], min, max);
		TextFieldNumeric(rect4, ref vector.z, ref buffer[2], min, max);
	}

	public static void TextFieldNumeric<T>(Rect rect, ref T val, ref string buffer, float min = 0f, float max = 1E+09f) where T : struct
	{
		if (buffer == null)
		{
			buffer = val.ToString();
		}
		GUI.SetNextControlName("TextField" + rect.y.ToString("F0") + rect.x.ToString("F0"));
		string text = TextField(rect, buffer);
		if (text != buffer && IsPartiallyOrFullyTypedNumber(ref val, text, min, max))
		{
			buffer = text;
			if (text.IsFullyTypedNumber<T>())
			{
				ResolveParseNow(text, ref val, ref buffer, min, max, force: false);
			}
		}
	}

	private static void ResolveParseNow<T>(string edited, ref T val, ref string buffer, float min, float max, bool force)
	{
		if (typeof(T) == typeof(int))
		{
			int result;
			if (edited.NullOrEmpty())
			{
				ResetValue(edited, ref val, ref buffer, min, max);
			}
			else if (int.TryParse(edited, out result))
			{
				val = (T)(object)Mathf.RoundToInt(Mathf.Clamp(result, min, max));
				buffer = ToStringTypedIn(val);
			}
			else if (force)
			{
				ResetValue(edited, ref val, ref buffer, min, max);
			}
		}
		else if (typeof(T) == typeof(float))
		{
			if (float.TryParse(edited, out var result2))
			{
				val = (T)(object)Mathf.Clamp(result2, min, max);
				buffer = ToStringTypedIn(val);
			}
			else if (force)
			{
				ResetValue(edited, ref val, ref buffer, min, max);
			}
		}
		else
		{
			Log.Error("TextField<T> does not support " + typeof(T));
		}
	}

	private static void ResetValue<T>(string edited, ref T val, ref string buffer, float min, float max)
	{
		val = default(T);
		if (min > 0f)
		{
			val = (T)(object)Mathf.RoundToInt(min);
		}
		if (max < 0f)
		{
			val = (T)(object)Mathf.RoundToInt(max);
		}
		buffer = ToStringTypedIn(val);
	}

	private static string ToStringTypedIn<T>(T val)
	{
		if (typeof(T) == typeof(float))
		{
			return ((float)(object)val).ToString("0.##########");
		}
		return val.ToString();
	}

	private static bool IsPartiallyOrFullyTypedNumber<T>(ref T val, string s, float min, float max)
	{
		if (s == "")
		{
			return true;
		}
		if (s[0] == '-' && min >= 0f)
		{
			return false;
		}
		if (s.Length > 1 && s[s.Length - 1] == '-')
		{
			return false;
		}
		if (s == "00")
		{
			return false;
		}
		if (s.Length > 12)
		{
			return false;
		}
		if (typeof(T) == typeof(float) && s.CharacterCount('.') <= 1 && s.ContainsOnlyCharacters("-.0123456789"))
		{
			return true;
		}
		if (s.IsFullyTypedNumber<T>())
		{
			return true;
		}
		return false;
	}

	private static bool IsFullyTypedNumber<T>(this string s)
	{
		if (s == "")
		{
			return false;
		}
		if (typeof(T) == typeof(float))
		{
			string[] array = s.Split('.');
			if (array.Length > 2 || array.Length < 1)
			{
				return false;
			}
			if (!array[0].ContainsOnlyCharacters("-0123456789"))
			{
				return false;
			}
			if (array.Length == 2 && (array[1].Length == 0 || !array[1].ContainsOnlyCharacters("0123456789")))
			{
				return false;
			}
		}
		if (typeof(T) == typeof(int) && !s.ContainsOnlyCharacters("-0123456789"))
		{
			return false;
		}
		return true;
	}

	private static bool ContainsOnlyCharacters(this string s, string allowedChars)
	{
		for (int i = 0; i < s.Length; i++)
		{
			if (!allowedChars.Contains(s[i]))
			{
				return false;
			}
		}
		return true;
	}

	private static int CharacterCount(this string s, char c)
	{
		int num = 0;
		for (int i = 0; i < s.Length; i++)
		{
			if (s[i] == c)
			{
				num++;
			}
		}
		return num;
	}

	public static void TextFieldNumericLabeled<T>(Rect rect, string label, ref T val, ref string buffer, float min = 0f, float max = 1E+09f) where T : struct
	{
		Rect rect2 = rect.LeftHalf().Rounded();
		Rect rect3 = rect.RightHalf().Rounded();
		TextAnchor anchor = Text.Anchor;
		Text.Anchor = TextAnchor.MiddleRight;
		Label(rect2, label);
		Text.Anchor = anchor;
		TextFieldNumeric(rect3, ref val, ref buffer, min, max);
	}

	public static void TextFieldPercent(Rect rect, ref float val, ref string buffer, float min = 0f, float max = 1f)
	{
		Rect rect2 = new Rect(rect.x, rect.y, rect.width - 25f, rect.height);
		Label(new Rect(rect2.xMax, rect.y, 25f, rect2.height), "%");
		float val2 = val * 100f;
		TextFieldNumeric(rect2, ref val2, ref buffer, min * 100f, max * 100f);
		val = val2 / 100f;
		if (val > max)
		{
			val = max;
			buffer = val.ToString();
		}
	}

	public static T ChangeType<T>(this object obj)
	{
		CultureInfo invariantCulture = CultureInfo.InvariantCulture;
		return (T)Convert.ChangeType(obj, typeof(T), invariantCulture);
	}

	public static void ResetSliderDraggingIDs()
	{
		sliderDraggingID = 0;
		draggingId = 0;
		curDragEnd = RangeEnd.None;
	}

	public static void HorizontalSlider(Rect rect, ref float value, FloatRange range, string label = null, float roundTo = -1f)
	{
		float trueMin = range.TrueMin;
		float trueMax = range.TrueMax;
		value = HorizontalSlider(rect, value, trueMin, trueMax, middleAlignment: false, label, trueMin.ToString(), trueMax.ToString(), roundTo);
	}

	public static float HorizontalSlider(Rect rect, float value, float min, float max, bool middleAlignment = false, string label = null, string leftAlignedLabel = null, string rightAlignedLabel = null, float roundTo = -1f)
	{
		float num = value;
		if (middleAlignment || !label.NullOrEmpty())
		{
			rect.y += Mathf.Round((rect.height - 10f) / 2f);
		}
		if (!label.NullOrEmpty())
		{
			rect.y += 5f;
		}
		int hashCode = UI.GUIToScreenPoint(new Vector2(rect.x, rect.y)).GetHashCode();
		hashCode = Gen.HashCombine(hashCode, rect.width);
		hashCode = Gen.HashCombine(hashCode, rect.height);
		hashCode = Gen.HashCombine(hashCode, min);
		hashCode = Gen.HashCombine(hashCode, max);
		Rect rect2 = rect;
		rect2.xMin += 6f;
		rect2.xMax -= 6f;
		GUI.color = RangeControlTextColor;
		Rect rect3 = new Rect(rect2.x, rect2.y + 2f, rect2.width, 8f);
		DrawAtlas(rect3, SliderRailAtlas);
		GUI.color = Color.white;
		float x = Mathf.Clamp(rect2.x - 6f + rect2.width * Mathf.InverseLerp(min, max, num), rect2.xMin - 6f, rect2.xMax - 6f);
		GUI.DrawTexture(new Rect(x, rect3.center.y - 6f, 12f, 12f), SliderHandle);
		if (Event.current.type == EventType.MouseDown && Mouse.IsOver(rect) && sliderDraggingID != hashCode)
		{
			sliderDraggingID = hashCode;
			SoundDefOf.DragSlider.PlayOneShotOnCamera();
			Event.current.Use();
		}
		if (sliderDraggingID == hashCode && UnityGUIBugsFixer.MouseDrag())
		{
			num = Mathf.Clamp((Event.current.mousePosition.x - rect2.x) / rect2.width * (max - min) + min, min, max);
			if (Event.current.type == EventType.MouseDrag)
			{
				Event.current.Use();
			}
		}
		if (!label.NullOrEmpty() || !leftAlignedLabel.NullOrEmpty() || !rightAlignedLabel.NullOrEmpty())
		{
			TextAnchor anchor = Text.Anchor;
			GameFont font = Text.Font;
			Text.Font = GameFont.Small;
			float num2 = (label.NullOrEmpty() ? 18f : Text.CalcSize(label).y);
			rect.y = rect.y - num2 + 3f;
			if (!leftAlignedLabel.NullOrEmpty())
			{
				Text.Anchor = TextAnchor.UpperLeft;
				Label(rect, leftAlignedLabel);
			}
			if (!rightAlignedLabel.NullOrEmpty())
			{
				Text.Anchor = TextAnchor.UpperRight;
				Label(rect, rightAlignedLabel);
			}
			if (!label.NullOrEmpty())
			{
				Text.Anchor = TextAnchor.UpperCenter;
				Label(rect, label);
			}
			Text.Anchor = anchor;
			Text.Font = font;
		}
		if (roundTo > 0f)
		{
			num = (float)Mathf.RoundToInt(num / roundTo) * roundTo;
		}
		if (value != num)
		{
			CheckPlayDragSliderSound();
		}
		return num;
	}

	public static float FrequencyHorizontalSlider(Rect rect, float freq, float minFreq, float maxFreq, bool roundToInt = false)
	{
		float num;
		if (freq < 1f)
		{
			float x = 1f / freq;
			num = GenMath.LerpDouble(1f, 1f / minFreq, 0.5f, 1f, x);
		}
		else
		{
			num = GenMath.LerpDouble(maxFreq, 1f, 0f, 0.5f, freq);
		}
		string label = ((freq == 1f) ? ((string)"EveryDay".Translate()) : ((!(freq < 1f)) ? ((string)"EveryDays".Translate(freq.ToString("0.##"))) : ((string)"TimesPerDay".Translate((1f / freq).ToString("0.##")))));
		float num2 = HorizontalSlider(rect, num, 0f, 1f, middleAlignment: true, label);
		if (num != num2)
		{
			float num3;
			if (num2 < 0.5f)
			{
				num3 = GenMath.LerpDouble(0.5f, 0f, 1f, maxFreq, num2);
				if (roundToInt)
				{
					num3 = Mathf.Round(num3);
				}
			}
			else
			{
				float num4 = GenMath.LerpDouble(1f, 0.5f, 1f / minFreq, 1f, num2);
				if (roundToInt)
				{
					num4 = Mathf.Round(num4);
				}
				num3 = 1f / num4;
			}
			freq = num3;
		}
		return freq;
	}

	public static void IntEntry(Rect rect, ref int value, ref string editBuffer, int multiplier = 1)
	{
		int num = Mathf.Min(IntEntryButtonWidth, (int)rect.width / 5);
		if (ButtonText(new Rect(rect.xMin, rect.yMin, num, rect.height), (-10 * multiplier).ToStringCached()))
		{
			value -= 10 * multiplier * GenUI.CurrentAdjustmentMultiplier();
			editBuffer = value.ToStringCached();
			SoundDefOf.Checkbox_TurnedOff.PlayOneShotOnCamera();
		}
		if (ButtonText(new Rect(rect.xMin + (float)num, rect.yMin, num, rect.height), (-1 * multiplier).ToStringCached()))
		{
			value -= multiplier * GenUI.CurrentAdjustmentMultiplier();
			editBuffer = value.ToStringCached();
			SoundDefOf.Checkbox_TurnedOff.PlayOneShotOnCamera();
		}
		if (ButtonText(new Rect(rect.xMax - (float)num, rect.yMin, num, rect.height), "+" + (10 * multiplier).ToStringCached()))
		{
			value += 10 * multiplier * GenUI.CurrentAdjustmentMultiplier();
			editBuffer = value.ToStringCached();
			SoundDefOf.Checkbox_TurnedOn.PlayOneShotOnCamera();
		}
		if (ButtonText(new Rect(rect.xMax - (float)(num * 2), rect.yMin, num, rect.height), "+" + multiplier.ToStringCached()))
		{
			value += multiplier * GenUI.CurrentAdjustmentMultiplier();
			editBuffer = value.ToStringCached();
			SoundDefOf.Checkbox_TurnedOn.PlayOneShotOnCamera();
		}
		TextFieldNumeric(new Rect(rect.xMin + (float)(num * 2), rect.yMin, rect.width - (float)(num * 4), rect.height), ref value, ref editBuffer);
	}

	public static void FloatRange(Rect rect, int id, ref FloatRange range, float min = 0f, float max = 1f, string labelKey = null, ToStringStyle valueStyle = ToStringStyle.FloatTwo, float gap = 0f, GameFont sliderLabelFont = GameFont.Small, Color? sliderLabelColor = null, float roundTo = 0f)
	{
		Rect rect2 = rect;
		rect2.xMin += 8f;
		rect2.xMax -= 8f;
		GUI.color = sliderLabelColor ?? RangeControlTextColor;
		string text = range.min.ToStringByStyle(valueStyle) + " - " + range.max.ToStringByStyle(valueStyle);
		if (labelKey != null)
		{
			text = labelKey.Translate(text);
		}
		GameFont font = Text.Font;
		Text.Font = sliderLabelFont;
		Text.Anchor = TextAnchor.UpperCenter;
		Rect rect3 = rect2;
		rect3.yMin -= 2f;
		rect3.height = Mathf.Max(rect3.height, Text.CalcHeight(text, rect3.width));
		LabelFit(rect3, text);
		Text.Anchor = TextAnchor.UpperLeft;
		Rect position = new Rect(rect2.x, rect2.yMax - 8f - 1f, rect2.width, 2f);
		GUI.DrawTexture(position, BaseContent.WhiteTex);
		float num = rect2.x + rect2.width * Mathf.InverseLerp(min, max, range.min);
		float num2 = rect2.x + rect2.width * Mathf.InverseLerp(min, max, range.max);
		GUI.color = Color.white;
		GUI.DrawTexture(new Rect(num, rect2.yMax - 8f - 2f, num2 - num, 4f), BaseContent.WhiteTex);
		float num3 = num;
		float num4 = num2;
		Rect position2 = new Rect(num3 - 16f, position.center.y - 8f, 16f, 16f);
		GUI.DrawTexture(position2, FloatRangeSliderTex);
		Rect position3 = new Rect(num4 + 16f, position.center.y - 8f, -16f, 16f);
		GUI.DrawTexture(position3, FloatRangeSliderTex);
		if (curDragEnd != RangeEnd.None && (Event.current.type == EventType.MouseUp || Event.current.rawType == EventType.MouseDown))
		{
			draggingId = 0;
			curDragEnd = RangeEnd.None;
			SoundDefOf.DragSlider.PlayOneShotOnCamera();
			Event.current.Use();
		}
		bool flag = false;
		if (Mouse.IsOver(rect) || draggingId == id)
		{
			if (Event.current.type == EventType.MouseDown && Event.current.button == 0 && id != draggingId)
			{
				draggingId = id;
				float x = Event.current.mousePosition.x;
				if (x < position2.xMax)
				{
					curDragEnd = RangeEnd.Min;
				}
				else if (x > position3.xMin)
				{
					curDragEnd = RangeEnd.Max;
				}
				else
				{
					float num5 = Mathf.Abs(x - position2.xMax);
					float num6 = Mathf.Abs(x - (position3.x - 16f));
					curDragEnd = ((num5 < num6) ? RangeEnd.Min : RangeEnd.Max);
				}
				flag = true;
				Event.current.Use();
				SoundDefOf.DragSlider.PlayOneShotOnCamera();
			}
			if (flag || (curDragEnd != RangeEnd.None && UnityGUIBugsFixer.MouseDrag()))
			{
				float value = (Event.current.mousePosition.x - rect2.x) / rect2.width * (max - min) + min;
				value = Mathf.Clamp(value, min, max);
				if (curDragEnd == RangeEnd.Min)
				{
					if (value != range.min)
					{
						range.min = Mathf.Min(value, max - gap);
						if (range.max < range.min + gap)
						{
							range.max = range.min + gap;
						}
						CheckPlayDragSliderSound();
					}
				}
				else if (curDragEnd == RangeEnd.Max && value != range.max)
				{
					range.max = Mathf.Max(value, min + gap);
					if (range.min > range.max - gap)
					{
						range.min = range.max - gap;
					}
					CheckPlayDragSliderSound();
				}
				if (roundTo != 0f)
				{
					range.min = Mathf.Round(range.min / roundTo) * roundTo;
					range.max = Mathf.Round(range.max / roundTo) * roundTo;
				}
				if (Event.current.type == EventType.MouseDrag)
				{
					Event.current.Use();
				}
			}
		}
		Text.Font = font;
	}

	public static void IntRange(Rect rect, int id, ref IntRange range, int min = 0, int max = 100, string labelKey = null, int minWidth = 0)
	{
		Rect rect2 = rect;
		rect2.xMin += 8f;
		rect2.xMax -= 8f;
		GUI.color = RangeControlTextColor;
		string text = range.min.ToStringCached() + " - " + range.max.ToStringCached();
		if (labelKey != null)
		{
			text = labelKey.Translate(text);
		}
		GameFont font = Text.Font;
		Text.Font = GameFont.Small;
		Text.Anchor = TextAnchor.UpperCenter;
		Rect rect3 = rect2;
		rect3.yMin -= 2f;
		Label(rect3, text);
		Text.Anchor = TextAnchor.UpperLeft;
		Rect position = new Rect(rect2.x, rect2.yMax - 8f - 1f, rect2.width, 2f);
		GUI.DrawTexture(position, BaseContent.WhiteTex);
		float num = rect2.x + rect2.width * (float)(range.min - min) / (float)(max - min);
		float num2 = rect2.x + rect2.width * (float)(range.max - min) / (float)(max - min);
		GUI.color = Color.white;
		GUI.DrawTexture(new Rect(num, rect2.yMax - 8f - 2f, num2 - num, 4f), BaseContent.WhiteTex);
		float num3 = num;
		float num4 = num2;
		Rect position2 = new Rect(num3 - 16f, position.center.y - 8f, 16f, 16f);
		GUI.DrawTexture(position2, FloatRangeSliderTex);
		Rect position3 = new Rect(num4 + 16f, position.center.y - 8f, -16f, 16f);
		GUI.DrawTexture(position3, FloatRangeSliderTex);
		if (curDragEnd != RangeEnd.None && (Event.current.type == EventType.MouseUp || Event.current.rawType == EventType.MouseDown))
		{
			draggingId = 0;
			curDragEnd = RangeEnd.None;
			SoundDefOf.DragSlider.PlayOneShotOnCamera();
		}
		bool flag = false;
		if (Mouse.IsOver(rect) || draggingId == id)
		{
			if (Event.current.type == EventType.MouseDown && Event.current.button == 0 && id != draggingId)
			{
				draggingId = id;
				float x = Event.current.mousePosition.x;
				if (x < position2.xMax)
				{
					curDragEnd = RangeEnd.Min;
				}
				else if (x > position3.xMin)
				{
					curDragEnd = RangeEnd.Max;
				}
				else
				{
					float num5 = Mathf.Abs(x - position2.xMax);
					float num6 = Mathf.Abs(x - (position3.x - 16f));
					curDragEnd = ((num5 < num6) ? RangeEnd.Min : RangeEnd.Max);
				}
				flag = true;
				Event.current.Use();
				SoundDefOf.DragSlider.PlayOneShotOnCamera();
			}
			if (flag || (curDragEnd != RangeEnd.None && UnityGUIBugsFixer.MouseDrag()))
			{
				int num7 = Mathf.RoundToInt(Mathf.Clamp((Event.current.mousePosition.x - rect2.x) / rect2.width * (float)(max - min) + (float)min, min, max));
				if (curDragEnd == RangeEnd.Min)
				{
					if (num7 != range.min)
					{
						range.min = num7;
						if (range.min > max - minWidth)
						{
							range.min = max - minWidth;
						}
						int num8 = Mathf.Max(min, range.min + minWidth);
						if (range.max < num8)
						{
							range.max = num8;
						}
						CheckPlayDragSliderSound();
					}
				}
				else if (curDragEnd == RangeEnd.Max && num7 != range.max)
				{
					range.max = num7;
					if (range.max < min + minWidth)
					{
						range.max = min + minWidth;
					}
					int num9 = Mathf.Min(max, range.max - minWidth);
					if (range.min > num9)
					{
						range.min = num9;
					}
					CheckPlayDragSliderSound();
				}
				if (Event.current.type == EventType.MouseDrag)
				{
					Event.current.Use();
				}
			}
		}
		Text.Font = font;
	}

	private static void CheckPlayDragSliderSound()
	{
		if (Time.realtimeSinceStartup > lastDragSliderSoundTime + 0.075f)
		{
			SoundDefOf.DragSlider.PlayOneShotOnCamera();
			lastDragSliderSoundTime = Time.realtimeSinceStartup;
		}
	}

	public static void QualityRange(Rect rect, int id, ref QualityRange range)
	{
		Rect rect2 = rect;
		rect2.xMin += 8f;
		rect2.xMax -= 8f;
		GUI.color = RangeControlTextColor;
		string label = ((range == RimWorld.QualityRange.All) ? ((string)"AnyQuality".Translate()) : ((range.max != range.min) ? (range.min.GetLabel() + " - " + range.max.GetLabel()) : ((string)"OnlyQuality".Translate(range.min.GetLabel()))));
		GameFont font = Text.Font;
		Text.Font = GameFont.Small;
		Text.Anchor = TextAnchor.UpperCenter;
		Rect rect3 = rect2;
		rect3.yMin -= 2f;
		Label(rect3, label);
		Text.Anchor = TextAnchor.UpperLeft;
		Rect position = new Rect(rect2.x, rect2.yMax - 8f - 1f, rect2.width, 2f);
		GUI.DrawTexture(position, BaseContent.WhiteTex);
		int qualityCount = QualityUtility.QualityCount;
		float num = rect2.x + rect2.width / (float)(qualityCount - 1) * (float)(int)range.min;
		float num2 = rect2.x + rect2.width / (float)(qualityCount - 1) * (float)(int)range.max;
		GUI.color = Color.white;
		GUI.DrawTexture(new Rect(num, rect2.yMax - 8f - 2f, num2 - num, 4f), BaseContent.WhiteTex);
		float num3 = num;
		float num4 = num2;
		Rect position2 = new Rect(num3 - 16f, position.center.y - 8f, 16f, 16f);
		GUI.DrawTexture(position2, FloatRangeSliderTex);
		Rect position3 = new Rect(num4 + 16f, position.center.y - 8f, -16f, 16f);
		GUI.DrawTexture(position3, FloatRangeSliderTex);
		if (curDragEnd != RangeEnd.None && (Event.current.type == EventType.MouseUp || Event.current.type == EventType.MouseDown))
		{
			draggingId = 0;
			curDragEnd = RangeEnd.None;
			SoundDefOf.DragSlider.PlayOneShotOnCamera();
		}
		bool flag = false;
		if (Mouse.IsOver(rect) || id == draggingId)
		{
			if (Event.current.type == EventType.MouseDown && Event.current.button == 0 && id != draggingId)
			{
				draggingId = id;
				float x = Event.current.mousePosition.x;
				if (x < position2.xMax)
				{
					curDragEnd = RangeEnd.Min;
				}
				else if (x > position3.xMin)
				{
					curDragEnd = RangeEnd.Max;
				}
				else
				{
					float num5 = Mathf.Abs(x - position2.xMax);
					float num6 = Mathf.Abs(x - (position3.x - 16f));
					curDragEnd = ((num5 < num6) ? RangeEnd.Min : RangeEnd.Max);
				}
				flag = true;
				Event.current.Use();
				SoundDefOf.DragSlider.PlayOneShotOnCamera();
			}
			if (flag || (curDragEnd != RangeEnd.None && UnityGUIBugsFixer.MouseDrag()))
			{
				int value = Mathf.RoundToInt((Event.current.mousePosition.x - rect2.x) / rect2.width * (float)(qualityCount - 1));
				value = Mathf.Clamp(value, 0, qualityCount - 1);
				if (curDragEnd == RangeEnd.Min)
				{
					if ((uint)range.min != (byte)value)
					{
						range.min = (QualityCategory)value;
						if ((int)range.max < (int)range.min)
						{
							range.max = range.min;
						}
						SoundDefOf.DragSlider.PlayOneShotOnCamera();
					}
				}
				else if (curDragEnd == RangeEnd.Max && (uint)range.max != (byte)value)
				{
					range.max = (QualityCategory)value;
					if ((int)range.min > (int)range.max)
					{
						range.min = range.max;
					}
					SoundDefOf.DragSlider.PlayOneShotOnCamera();
				}
				if (Event.current.type == EventType.MouseDrag)
				{
					Event.current.Use();
				}
			}
		}
		Text.Font = font;
	}

	public static void FloatRangeWithTypeIn(Rect rect, int id, ref FloatRange fRange, float sliderMin = 0f, float sliderMax = 1f, ToStringStyle valueStyle = ToStringStyle.FloatTwo, string labelKey = null)
	{
		Rect rect2 = new Rect(rect);
		rect2.width = rect.width / 4f;
		Rect rect3 = new Rect(rect);
		rect3.width = rect.width / 2f;
		rect3.x = rect.x + rect.width / 4f;
		rect3.height = rect.height / 2f;
		rect3.width -= rect.height;
		Rect butRect = new Rect(rect3);
		butRect.x = rect3.xMax;
		butRect.height = rect.height;
		butRect.width = rect.height;
		Rect rect4 = new Rect(rect);
		rect4.x = rect.x + rect.width * 0.75f;
		rect4.width = rect.width / 4f;
		rect3.y += 4f;
		rect3.height += 4f;
		FloatRange(rect3, id, ref fRange, sliderMin, sliderMax, labelKey, valueStyle);
		if (ButtonImage(butRect, TexButton.RangeMatch))
		{
			fRange.max = fRange.min;
		}
		float.TryParse(TextField(rect2, fRange.min.ToString()), out fRange.min);
		float.TryParse(TextField(rect4, fRange.max.ToString()), out fRange.max);
	}

	public static Rect FillableBar(Rect rect, float fillPercent)
	{
		return FillableBar(rect, fillPercent, BarFullTexHor);
	}

	public static Rect FillableBar(Rect rect, float fillPercent, Texture2D fillTex)
	{
		bool doBorder = rect.height > 15f && rect.width > 20f;
		return FillableBar(rect, fillPercent, fillTex, DefaultBarBgTex, doBorder);
	}

	public static Rect FillableBar(Rect rect, float fillPercent, Texture2D fillTex, Texture2D bgTex, bool doBorder)
	{
		if (doBorder)
		{
			GUI.DrawTexture(rect, BaseContent.BlackTex);
			rect = rect.ContractedBy(3f);
		}
		if (bgTex != null)
		{
			GUI.DrawTexture(rect, bgTex);
		}
		Rect result = rect;
		rect.width *= fillPercent;
		GUI.DrawTexture(rect, fillTex);
		return result;
	}

	public static void FillableBarLabeled(Rect rect, float fillPercent, int labelWidth, string label)
	{
		if (fillPercent < 0f)
		{
			fillPercent = 0f;
		}
		if (fillPercent > 1f)
		{
			fillPercent = 1f;
		}
		Rect rect2 = rect;
		rect2.width = labelWidth;
		Label(rect2, label);
		Rect rect3 = rect;
		rect3.x += labelWidth;
		rect3.width -= labelWidth;
		FillableBar(rect3, fillPercent);
	}

	public static void FillableBarChangeArrows(Rect barRect, float changeRate)
	{
		int changeRate2 = (int)(changeRate * FillableBarChangeRateDisplayRatio);
		FillableBarChangeArrows(barRect, changeRate2);
	}

	public static void FillableBarChangeArrows(Rect barRect, int changeRate)
	{
		if (changeRate != 0)
		{
			if (changeRate > MaxFillableBarChangeRate)
			{
				changeRate = MaxFillableBarChangeRate;
			}
			if (changeRate < -MaxFillableBarChangeRate)
			{
				changeRate = -MaxFillableBarChangeRate;
			}
			float num = barRect.height;
			if (num > 16f)
			{
				num = 16f;
			}
			int num2 = Mathf.Abs(changeRate);
			float y = barRect.y + barRect.height / 2f - num / 2f;
			float num3;
			float num4;
			Texture2D image;
			if (changeRate > 0)
			{
				num3 = barRect.x + barRect.width + 2f;
				num4 = 8f;
				image = FillArrowTexRight;
			}
			else
			{
				num3 = barRect.x - 8f - 2f;
				num4 = -8f;
				image = FillArrowTexLeft;
			}
			for (int i = 0; i < num2; i++)
			{
				GUI.DrawTexture(new Rect(num3, y, 8f, num), image);
				num3 += num4;
			}
		}
	}

	public static void DrawWindowBackground(Rect rect)
	{
		GUI.color = WindowBGFillColor;
		GUI.DrawTexture(rect, BaseContent.WhiteTex);
		GUI.color = WindowBGBorderColor;
		DrawBox(rect);
		GUI.color = Color.white;
	}

	public static void DrawWindowBackground(Rect rect, Color colorFactor)
	{
		Color color = GUI.color;
		GUI.color = WindowBGFillColor * colorFactor;
		GUI.DrawTexture(rect, BaseContent.WhiteTex);
		GUI.color = WindowBGBorderColor * colorFactor;
		DrawBox(rect);
		GUI.color = color;
	}

	public static void DrawMenuSection(Rect rect)
	{
		GUI.color = MenuSectionBGFillColor;
		GUI.DrawTexture(rect, BaseContent.WhiteTex);
		GUI.color = MenuSectionBGBorderColor;
		DrawBox(rect);
		GUI.color = Color.white;
	}

	public static void DrawWindowBackgroundTutor(Rect rect)
	{
		GUI.color = TutorWindowBGFillColor;
		GUI.DrawTexture(rect, BaseContent.WhiteTex);
		GUI.color = TutorWindowBGBorderColor;
		DrawBox(rect);
		GUI.color = Color.white;
	}

	public static void DrawOptionUnselected(Rect rect)
	{
		GUI.color = OptionUnselectedBGFillColor;
		GUI.DrawTexture(rect, BaseContent.WhiteTex);
		GUI.color = OptionUnselectedBGBorderColor;
		DrawBox(rect);
		GUI.color = Color.white;
	}

	public static void DrawOptionSelected(Rect rect)
	{
		GUI.color = OptionSelectedBGFillColor;
		GUI.DrawTexture(rect, BaseContent.WhiteTex);
		GUI.color = OptionSelectedBGBorderColor;
		DrawBox(rect.ExpandedBy(3f), 3);
		GUI.color = Color.white;
	}

	public static void DrawOptionBackground(Rect rect, bool selected)
	{
		if (selected)
		{
			DrawOptionSelected(rect);
		}
		else
		{
			DrawOptionUnselected(rect);
		}
		DrawHighlightIfMouseover(rect);
	}

	public static void DrawShadowAround(Rect rect)
	{
		Rect rect2 = rect.ContractedBy(-9f);
		rect2.x += 2f;
		rect2.y += 2f;
		DrawAtlas(rect2, ShadowAtlas);
	}

	public static void DrawAtlas(Rect rect, Texture2D atlas)
	{
		DrawAtlas(rect, atlas, drawTop: true);
	}

	public static void DrawAtlas(Rect rect, Texture2D atlas, bool drawTop)
	{
		if (Event.current.type == EventType.Repaint)
		{
			rect.x = Mathf.Round(rect.x);
			rect.y = Mathf.Round(rect.y);
			rect.width = Mathf.Round(rect.width);
			rect.height = Mathf.Round(rect.height);
			rect = UIScaling.AdjustRectToUIScaling(rect);
			float a = (float)atlas.width * 0.25f;
			a = UIScaling.AdjustCoordToUIScalingCeil(GenMath.Min(a, rect.height / 2f, rect.width / 2f));
			Rect drawRect;
			if (drawTop)
			{
				drawRect = new Rect(rect.x, rect.y, a, a);
				DrawTexturePart(drawRect, AtlasUV_TopLeft, atlas);
				drawRect = new Rect(rect.x + rect.width - a, rect.y, a, a);
				DrawTexturePart(drawRect, AtlasUV_TopRight, atlas);
			}
			drawRect = new Rect(rect.x, rect.y + rect.height - a, a, a);
			DrawTexturePart(drawRect, AtlasUV_BottomLeft, atlas);
			drawRect = new Rect(rect.x + rect.width - a, rect.y + rect.height - a, a, a);
			DrawTexturePart(drawRect, AtlasUV_BottomRight, atlas);
			drawRect = new Rect(rect.x + a, rect.y + a, rect.width - a * 2f, rect.height - a * 2f);
			if (!drawTop)
			{
				drawRect.height += a;
				drawRect.y -= a;
			}
			DrawTexturePart(drawRect, AtlasUV_Center, atlas);
			if (drawTop)
			{
				drawRect = new Rect(rect.x + a, rect.y, rect.width - a * 2f, a);
				DrawTexturePart(drawRect, AtlasUV_Top, atlas);
			}
			drawRect = new Rect(rect.x + a, rect.y + rect.height - a, rect.width - a * 2f, a);
			DrawTexturePart(drawRect, AtlasUV_Bottom, atlas);
			drawRect = new Rect(rect.x, rect.y + a, a, rect.height - a * 2f);
			if (!drawTop)
			{
				drawRect.height += a;
				drawRect.y -= a;
			}
			DrawTexturePart(drawRect, AtlasUV_Left, atlas);
			drawRect = new Rect(rect.x + rect.width - a, rect.y + a, a, rect.height - a * 2f);
			if (!drawTop)
			{
				drawRect.height += a;
				drawRect.y -= a;
			}
			DrawTexturePart(drawRect, AtlasUV_Right, atlas);
		}
	}

	public static void DrawAtlasWithMaterial(Rect rect, Texture2D atlas, Material mat, bool drawTop = true)
	{
		rect.x = Mathf.Round(rect.x);
		rect.y = Mathf.Round(rect.y);
		rect.width = Mathf.Round(rect.width);
		rect.height = Mathf.Round(rect.height);
		rect = UIScaling.AdjustRectToUIScaling(rect);
		float a = (float)atlas.width * 0.25f;
		a = UIScaling.AdjustCoordToUIScalingCeil(GenMath.Min(a, rect.height / 2f, rect.width / 2f));
		BeginGroup(rect);
		Rect rect2;
		if (drawTop)
		{
			rect2 = new Rect(0f, 0f, a, a);
			GenUI.DrawTexturePartWithMaterial(rect2, atlas, mat, AtlasUV_TopLeft);
			rect2 = new Rect(rect.width - a, 0f, a, a);
			GenUI.DrawTexturePartWithMaterial(rect2, atlas, mat, AtlasUV_TopRight);
		}
		rect2 = new Rect(0f, rect.height - a, a, a);
		GenUI.DrawTexturePartWithMaterial(rect2, atlas, mat, AtlasUV_BottomLeft);
		rect2 = new Rect(rect.width - a, rect.height - a, a, a);
		GenUI.DrawTexturePartWithMaterial(rect2, atlas, mat, AtlasUV_BottomRight);
		rect2 = new Rect(a, a, rect.width - a * 2f, rect.height - a * 2f);
		if (!drawTop)
		{
			rect2.height += a;
			rect2.y -= a;
		}
		GenUI.DrawTexturePartWithMaterial(rect2, atlas, mat, AtlasUV_Center);
		if (drawTop)
		{
			rect2 = new Rect(a, 0f, rect.width - a * 2f, a);
			GenUI.DrawTexturePartWithMaterial(rect2, atlas, mat, AtlasUV_Top);
		}
		rect2 = new Rect(a, rect.height - a, rect.width - a * 2f, a);
		GenUI.DrawTexturePartWithMaterial(rect2, atlas, mat, AtlasUV_Bottom);
		rect2 = new Rect(0f, a, a, rect.height - a * 2f);
		if (!drawTop)
		{
			rect2.height += a;
			rect2.y -= a;
		}
		GenUI.DrawTexturePartWithMaterial(rect2, atlas, mat, AtlasUV_Left);
		rect2 = new Rect(rect.width - a, a, a, rect.height - a * 2f);
		if (!drawTop)
		{
			rect2.height += a;
			rect2.y -= a;
		}
		GenUI.DrawTexturePartWithMaterial(rect2, atlas, mat, AtlasUV_Right);
		EndGroup();
	}

	public static Rect ToUVRect(this Rect r, Vector2 texSize)
	{
		return new Rect(r.x / texSize.x, r.y / texSize.y, r.width / texSize.x, r.height / texSize.y);
	}

	public static void DrawTexturePart(Rect drawRect, Rect uvRect, Texture2D tex)
	{
		uvRect.y = 1f - uvRect.y - uvRect.height;
		GUI.DrawTextureWithTexCoords(drawRect, tex, uvRect);
	}

	public static void ScrollHorizontal(Rect outRect, ref Vector2 scrollPosition, Rect viewRect, float ScrollWheelSpeed = 20f)
	{
		if (Event.current.type == EventType.ScrollWheel && Mouse.IsOver(outRect))
		{
			scrollPosition.x += Event.current.delta.y * ScrollWheelSpeed;
			float num = 0f;
			float num2 = viewRect.width - outRect.width + 16f;
			if (scrollPosition.x < num)
			{
				scrollPosition.x = num;
			}
			if (scrollPosition.x > num2)
			{
				scrollPosition.x = num2;
			}
			Event.current.Use();
		}
	}

	public static void BeginScrollView(Rect outRect, ref Vector2 scrollPosition, Rect viewRect, bool showScrollbars = true)
	{
		if (mouseOverScrollViewStack.Count > 0)
		{
			mouseOverScrollViewStack.Push(mouseOverScrollViewStack.Peek() && outRect.Contains(Event.current.mousePosition));
		}
		else
		{
			mouseOverScrollViewStack.Push(outRect.Contains(Event.current.mousePosition));
		}
		SteamDeck.HandleTouchScreenScrollViewScroll(outRect, ref scrollPosition);
		if (showScrollbars)
		{
			scrollPosition = GUI.BeginScrollView(outRect, scrollPosition, viewRect);
		}
		else
		{
			scrollPosition = GUI.BeginScrollView(outRect, scrollPosition, viewRect, GUIStyle.none, GUIStyle.none);
		}
		UnityGUIBugsFixer.Notify_BeginScrollView();
	}

	public static void AdjustRectsForScrollView(Rect parentRect, ref Rect outRect, ref Rect viewRect)
	{
		if (viewRect.height >= outRect.height)
		{
			viewRect.width -= 20f;
			outRect.xMax -= 4f;
			outRect.yMin = Mathf.Max(parentRect.yMin + 6f, outRect.yMin);
			outRect.yMax = Mathf.Min(parentRect.yMax - 6f, outRect.yMax);
		}
	}

	public static void EndScrollView()
	{
		mouseOverScrollViewStack.Pop();
		GUI.EndScrollView();
	}

	public static void EnsureMousePositionStackEmpty()
	{
		if (mouseOverScrollViewStack.Count > 0)
		{
			Log.Error("Mouse position stack is not empty. There were more calls to BeginScrollView than EndScrollView. Fixing.");
			mouseOverScrollViewStack.Clear();
		}
	}

	public static void ColorSelectorIcon(Rect rect, Texture icon, Color color, bool drawColor = false)
	{
		if (icon != null)
		{
			GUI.color = color;
			GUI.DrawTexture(rect, icon);
			GUI.color = Color.white;
		}
		else if (drawColor)
		{
			DrawBoxSolid(rect, color);
		}
	}

	public static bool ColorBox(Rect rect, ref Color color, Color boxColor, int colorSize = 22, int colorPadding = 2, Action<Color, Rect> extraOnGUI = null)
	{
		DrawLightHighlight(rect);
		DrawHighlightIfMouseover(rect);
		if (color.IndistinguishableFrom(boxColor))
		{
			DrawBox(rect);
		}
		Rect rect2 = new Rect(rect.x + (float)colorPadding, rect.y + (float)colorPadding, colorSize, colorSize);
		DrawBoxSolid(rect2, boxColor);
		extraOnGUI?.Invoke(boxColor, rect2);
		bool result = false;
		if (ButtonInvisible(rect))
		{
			result = true;
			color = boxColor;
			SoundDefOf.Tick_High.PlayOneShotOnCamera();
		}
		return result;
	}

	public static bool ColorSelector(Rect rect, ref Color color, List<Color> colors, out float height, Texture icon = null, int colorSize = 22, int colorPadding = 2, Action<Color, Rect> extraOnGUI = null)
	{
		height = 0f;
		bool result = false;
		int num = colorSize + colorPadding * 2;
		float num2 = ((icon != null) ? ((float)(colorSize * 4) + 10f) : 0f);
		int num3 = Mathf.FloorToInt((rect.width - num2 + (float)colorPadding) / (float)(num + colorPadding));
		int num4 = Mathf.CeilToInt((float)colors.Count / (float)num3);
		BeginGroup(rect);
		ColorSelectorIcon(new Rect(5f, 5f, colorSize * 4, colorSize * 4), icon, color);
		for (int i = 0; i < colors.Count; i++)
		{
			int num5 = i / num3;
			int num6 = i % num3;
			float num7 = ((icon != null) ? ((num2 - (float)(num * num4) - (float)colorPadding) / 2f) : 0f);
			Rect rect2 = new Rect(num2 + (float)(num6 * num) + (float)(num6 * colorPadding), num7 + (float)(num5 * num) + (float)(num5 * colorPadding), num, num);
			if (ColorBox(rect2, ref color, colors[i], colorSize, colorPadding, extraOnGUI))
			{
				result = true;
			}
			height = Mathf.Max(height, rect2.yMax);
		}
		EndGroup();
		return result;
	}

	private static void DrawColorSelectionCircle(Rect hsvColorWheelRect, Vector2Int center, Color color)
	{
		int num = (int)Mathf.Round(hsvColorWheelRect.width * 0.125f);
		GUI.DrawTexture(new Rect(center.x - num / 2, center.y - num / 2, num, num), ColorSelectionCircle, ScaleMode.ScaleToFit, alphaBlend: true, 1f, color, 0f, 0f);
	}

	private static bool ClickedInsideRect(Rect rect)
	{
		if (Event.current.type == EventType.MouseDown)
		{
			return rect.Contains(Event.current.mousePosition);
		}
		return false;
	}

	public static void HSVColorWheel(Rect rect, ref Color color, ref bool currentlyDragging, float? colorValueOverride = null, string controlName = null)
	{
		if (rect.width != rect.height)
		{
			throw new ArgumentException("HSV color wheel must be drawn in a square rect.");
		}
		Color.RGBToHSV(color, out var H, out var S, out var V);
		float num = colorValueOverride ?? V;
		GUI.DrawTexture(rect, HSVColorWheelTex, ScaleMode.ScaleToFit, alphaBlend: true, 1f, Color.HSVToRGB(0f, 0f, num), 0f, 0f);
		H = (H + 0.25f) * 2f * MathF.PI;
		Vector2 vector = new Vector2(Mathf.Cos(H), 0f - Mathf.Sin(H)) * S * rect.width / 2f;
		DrawColorSelectionCircle(rect, Vector2Int.RoundToInt(vector + rect.center), (num > 0.5f) ? Color.black : Color.white);
		if (!currentlyDragging)
		{
			MouseoverSounds.DoRegion(rect);
		}
		if (Event.current.isMouse && Event.current.button == 0)
		{
			if (currentlyDragging && Event.current.type == EventType.MouseUp)
			{
				currentlyDragging = false;
			}
			else if (ClickedInsideRect(rect) | currentlyDragging)
			{
				GUI.FocusControl(controlName);
				currentlyDragging = true;
				Vector2 vector2 = (Event.current.mousePosition - rect.center) / (rect.size / 2f);
				float num2 = Mathf.Atan2(0f - vector2.y, vector2.x) / (MathF.PI * 2f);
				num2 += 1.75f;
				num2 %= 1f;
				float s = Mathf.Clamp01(vector2.magnitude);
				color = Color.HSVToRGB(num2, s, num);
				Event.current.Use();
			}
		}
	}

	public static void ColorTemperatureBar(Rect rect, ref Color color, ref bool dragging, float? colorValueOverride = null)
	{
		float num = colorValueOverride ?? Mathf.Max(color.r, color.g, color.b);
		float? num2 = color.ColorTemperature();
		string label = num2?.ToString("0.K") ?? "";
		RectDivider rectDivider = new RectDivider(rect, 661493905, new Vector2(17f, 0f));
		using (new TextBlock(TextAnchor.MiddleLeft))
		{
			string text = "ColorTemperature".Translate().CapitalizeFirst();
			Label(rectDivider.NewCol(Text.CalcSize(text).x), text);
			Label(rectDivider.NewCol(Text.CalcSize("XXXXXK").x), label);
		}
		if (!dragging)
		{
			TooltipHandler.TipRegion(rect, "ColorTemperatureTooltip".Translate());
			MouseoverSounds.DoRegion(rect);
		}
		if (Event.current.button == 0)
		{
			if (dragging && Event.current.type == EventType.MouseUp)
			{
				dragging = false;
			}
			else if (ClickedInsideRect(rectDivider) || (dragging && UnityGUIBugsFixer.MouseDrag()))
			{
				dragging = true;
				if (Event.current.type == EventType.MouseDrag)
				{
					Event.current.Use();
				}
				float fraction = Mathf.Clamp01((Event.current.mousePosition.x - rectDivider.Rect.xMin) / rectDivider.Rect.width);
				num2 = GenMath.ExponentialWarpInterpolation(1000f, 40000f, fraction, new Vector2(0.5f, 6600f));
				color = GenColor.FromColorTemperature(num2.Value);
				color *= num;
			}
		}
		rectDivider.NewRow(6f);
		rectDivider.NewRow(6f, VerticalJustification.Bottom);
		GUI.DrawTexture(rectDivider, ColorTemperatureExp, ScaleMode.StretchToFill, alphaBlend: true, 1f, Color.HSVToRGB(0f, 0f, num), 0f, 0f);
		if (num2.HasValue)
		{
			float num3 = rectDivider.Rect.width * GenMath.InverseExponentialWarpInterpolation(1000f, 40000f, num2.Value, new Vector2(0.5f, 6600f));
			Rect position = new Rect(rectDivider.Rect.x + num3 - 6f, rectDivider.Rect.y - 6f, 12f, 12f);
			Rect position2 = new Rect(rectDivider.Rect.x + num3 - 6f, rectDivider.Rect.yMax - 6f, 12f, 12f);
			GUI.DrawTextureWithTexCoords(position, SelectionArrow, new Rect(0f, 1f, 1f, -1f), alphaBlend: true);
			GUI.DrawTextureWithTexCoords(position2, SelectionArrow, new Rect(0f, 0f, 1f, 1f), alphaBlend: true);
		}
	}

	private static int ToIntegerRange(float fraction, int min, int max)
	{
		return Mathf.Clamp(Mathf.RoundToInt(fraction * (float)max), min, max);
	}

	public static bool ColorTextfields(ref RectAggregator aggregator, ref Color color, ref string[] buffers, ref Color colorBuffer, string previousFocusedControlName, string controlName = null, ColorComponents editable = ColorComponents.All, ColorComponents visible = ColorComponents.All)
	{
		if (visible == ColorComponents.None)
		{
			return false;
		}
		if ((~visible & editable) != ColorComponents.None)
		{
			throw new ArgumentException($"Cannot have editable but invisible components {~visible & editable}.");
		}
		controlName = controlName ?? $"ColorTextfields{aggregator.Rect.x}{aggregator.Rect.y}";
		bool flag = previousFocusedControlName?.StartsWith(controlName) ?? false;
		bool flag2 = GUI.GetNameOfFocusedControl().StartsWith(controlName);
		using (new TextBlock(TextAnchor.MiddleLeft))
		{
			float num = 30f;
			float num2 = 0f;
			for (int i = 0; i < colorComponentLabels.Length; i++)
			{
				tmpTranslatedColorComponentLabels[i] = colorComponentLabels[i].Translate().CapitalizeFirst();
				num = Mathf.Max(num, tmpTranslatedColorComponentLabels[i].GetHeightCached());
				num2 = Mathf.Max(num2, tmpTranslatedColorComponentLabels[i].GetWidthCached());
			}
			Color.RGBToHSV(colorBuffer, out var H, out var S, out var V);
			intColorComponents[0] = ToIntegerRange(colorBuffer.r, 0, maxColorComponentValues[0]);
			intColorComponents[1] = ToIntegerRange(colorBuffer.g, 0, maxColorComponentValues[1]);
			intColorComponents[2] = ToIntegerRange(colorBuffer.b, 0, maxColorComponentValues[2]);
			intColorComponents[3] = ToIntegerRange(H, 0, maxColorComponentValues[3]);
			intColorComponents[4] = ToIntegerRange(S, 0, maxColorComponentValues[4]);
			intColorComponents[5] = ToIntegerRange(V, 0, maxColorComponentValues[5]);
			for (int j = 0; j <= 5; j++)
			{
				ColorComponents colorComponents = (ColorComponents)(1 << j);
				if ((visible & colorComponents) == 0)
				{
					continue;
				}
				RectDivider rectDivider = aggregator.NewRow(num);
				Label(rectDivider.NewCol(num2), tmpTranslatedColorComponentLabels[j]);
				if ((editable & colorComponents) == 0)
				{
					Label(rectDivider, intColorComponents[j].ToString());
					continue;
				}
				string text = intColorComponents[j].ToString();
				string text2 = DelayedTextField(rectDivider, text, ref buffers[j], previousFocusedControlName, $"{controlName}_{j}");
				if (text != text2 && int.TryParse(text2, out var result))
				{
					intColorComponents[j] = result;
					if (j < 3)
					{
						colorBuffer = new ColorInt(intColorComponents[0], intColorComponents[1], intColorComponents[2]).ToColor;
					}
					else
					{
						colorBuffer = Color.HSVToRGB((float)intColorComponents[3] / 360f, (float)intColorComponents[4] / 100f, (float)intColorComponents[5] / 100f);
					}
				}
			}
		}
		if (flag)
		{
			if (!flag2)
			{
				color = colorBuffer;
				return true;
			}
		}
		else
		{
			colorBuffer = color;
		}
		return false;
	}

	public static void DrawHighlightSelected(Rect rect)
	{
		GUI.DrawTexture(rect, TexUI.HighlightSelectedTex);
	}

	public static void DrawHighlightIfMouseover(Rect rect)
	{
		if (Mouse.IsOver(rect))
		{
			DrawHighlight(rect);
		}
	}

	public static void DrawHighlight(Rect rect)
	{
		GUI.DrawTexture(rect, TexUI.HighlightTex);
	}

	public static void DrawHighlight(Rect rect, float opacity)
	{
		GUI.color = Color.white.ToTransparent(opacity);
		GUI.DrawTexture(rect, TexUI.HighlightTex);
		GUI.color = Color.white;
	}

	public static void DrawLightHighlight(Rect rect)
	{
		GUI.DrawTexture(rect, LightHighlight);
	}

	public static void DrawStrongHighlight(Rect rect, Color? color = null)
	{
		Color color2 = GUI.color;
		GUI.color = color.GetValueOrDefault(HighlightStrongBgColor);
		DrawAtlas(rect, TexUI.RectHighlight);
		GUI.color = color2;
	}

	public static void DrawTextHighlight(Rect rect, float expandBy = 4f, Color? color = null)
	{
		rect.y -= expandBy;
		rect.height += expandBy * 2f;
		Color color2 = GUI.color;
		GUI.color = color.GetValueOrDefault(HighlightTextBgColor);
		DrawAtlas(rect, TexUI.RectHighlight);
		GUI.color = color2;
	}

	public static void DrawTitleBG(Rect rect)
	{
		GUI.DrawTexture(rect, TexUI.TitleBGTex);
	}

	public static bool InfoCardButton(float x, float y, Thing thing)
	{
		if (thing is IConstructible constructible)
		{
			if (thing.def.entityDefToBuild is ThingDef thingDef)
			{
				return InfoCardButton(x, y, thingDef, constructible.EntityToBuildStuff());
			}
			return InfoCardButton(x, y, thing.def.entityDefToBuild);
		}
		if (InfoCardButtonWorker(x, y))
		{
			Find.WindowStack.Add(new Dialog_InfoCard(thing));
			return true;
		}
		return false;
	}

	public static bool InfoCardButton(float x, float y, Def def)
	{
		if (InfoCardButtonWorker(x, y))
		{
			Find.WindowStack.Add(new Dialog_InfoCard(def));
			return true;
		}
		return false;
	}

	public static bool InfoCardButton(Rect rect, Def def)
	{
		if (InfoCardButtonWorker(rect))
		{
			Find.WindowStack.Add(new Dialog_InfoCard(def));
			return true;
		}
		return false;
	}

	public static bool InfoCardButton(float x, float y, Def def, Precept_ThingStyle precept)
	{
		if (InfoCardButtonWorker(x, y))
		{
			Find.WindowStack.Add(new Dialog_InfoCard(def, precept));
			return true;
		}
		return false;
	}

	public static bool InfoCardButton(float x, float y, ThingDef thingDef, ThingDef stuffDef)
	{
		if (InfoCardButtonWorker(x, y))
		{
			Find.WindowStack.Add(new Dialog_InfoCard(thingDef, stuffDef));
			return true;
		}
		return false;
	}

	public static bool InfoCardButton(float x, float y, WorldObject worldObject)
	{
		if (InfoCardButtonWorker(x, y))
		{
			Find.WindowStack.Add(new Dialog_InfoCard(worldObject));
			return true;
		}
		return false;
	}

	public static bool InfoCardButton(Rect rect, Hediff hediff)
	{
		if (InfoCardButtonWorker(rect))
		{
			Find.WindowStack.Add(new Dialog_InfoCard(hediff));
			return true;
		}
		return false;
	}

	public static bool InfoCardButton(float x, float y, Faction faction)
	{
		if (InfoCardButtonWorker(x, y))
		{
			Find.WindowStack.Add(new Dialog_InfoCard(faction));
			return true;
		}
		return false;
	}

	public static bool InfoCardButtonCentered(Rect rect, Thing thing)
	{
		return InfoCardButton(rect.center.x - 12f, rect.center.y - 12f, thing);
	}

	public static bool InfoCardButtonCentered(Rect rect, Faction faction)
	{
		return InfoCardButton(rect.center.x - 12f, rect.center.y - 12f, faction);
	}

	private static bool InfoCardButtonWorker(float x, float y)
	{
		return InfoCardButtonWorker(new Rect(x, y, 24f, 24f));
	}

	private static bool InfoCardButtonWorker(Rect rect)
	{
		MouseoverSounds.DoRegion(rect);
		TooltipHandler.TipRegionByKey(rect, "DefInfoTip");
		bool result = ButtonImage(rect, TexButton.Info, GUI.color);
		UIHighlighter.HighlightOpportunity(rect, "InfoCard");
		return result;
	}

	public static void DrawTextureFitted(Rect outerRect, Texture tex, float scale, float alpha = 1f)
	{
		DrawTextureFitted(outerRect, tex, scale, new Vector2(tex.width, tex.height), new Rect(0f, 0f, 1f, 1f), 0f, null, alpha);
	}

	public static void DrawTextureFitted(Rect outerRect, Texture tex, float scale, Material mat, float alpha = 1f)
	{
		DrawTextureFitted(outerRect, tex, scale, new Vector2(tex.width, tex.height), new Rect(0f, 0f, 1f, 1f), 0f, mat, alpha);
	}

	public static void DrawTextureFitted(Rect outerRect, Texture tex, float scale, Vector2 texProportions, Rect texCoords, float angle = 0f, Material mat = null, float alpha = 1f)
	{
		if (Event.current.type == EventType.Repaint)
		{
			Rect rect = new Rect(0f, 0f, texProportions.x, texProportions.y);
			float num = ((!(rect.width / rect.height < outerRect.width / outerRect.height)) ? (outerRect.width / rect.width) : (outerRect.height / rect.height));
			num *= scale;
			rect.width *= num;
			rect.height *= num;
			rect.x = outerRect.x + outerRect.width / 2f - rect.width / 2f;
			rect.y = outerRect.y + outerRect.height / 2f - rect.height / 2f;
			Matrix4x4 matrix = Matrix4x4.identity;
			if (angle != 0f)
			{
				matrix = GUI.matrix;
				UI.RotateAroundPivot(angle, rect.center);
			}
			Color color = Color.white;
			if (!Mathf.Approximately(alpha, 1f))
			{
				Color color2 = (color = GUI.color);
				color2.a *= alpha;
				GUI.color = color2;
			}
			GenUI.DrawTextureWithMaterial(rect, tex, mat, texCoords);
			if (angle != 0f)
			{
				GUI.matrix = matrix;
			}
			if (!Mathf.Approximately(alpha, 1f))
			{
				GUI.color = color;
			}
		}
	}

	public static void DrawTextureRotated(Vector2 center, Texture tex, float angle, float scale = 1f, Material material = null)
	{
		float num = (float)tex.width * scale;
		float num2 = (float)tex.height * scale;
		DrawTextureRotated(new Rect(center.x - num / 2f, center.y - num2 / 2f, num, num2), tex, angle, material);
	}

	public static void DrawTextureRotated(Rect rect, Texture tex, float angle, Material material = null)
	{
		if (Event.current.type != EventType.Repaint)
		{
			return;
		}
		if (angle == 0f)
		{
			if (material == null)
			{
				GUI.DrawTexture(rect, tex);
			}
			else
			{
				GenUI.DrawTextureWithMaterial(rect, tex, material);
			}
			return;
		}
		Matrix4x4 matrix = GUI.matrix;
		UI.RotateAroundPivot(angle, rect.center);
		if (material == null)
		{
			GUI.DrawTexture(rect, tex);
		}
		else
		{
			GenUI.DrawTextureWithMaterial(rect, tex, material);
		}
		GUI.matrix = matrix;
	}

	public static void NoneLabel(float y, float width, string customLabel = null)
	{
		NoneLabel(ref y, width, customLabel);
	}

	public static void NoneLabel(ref float curY, float width, string customLabel = null)
	{
		GUI.color = Color.gray;
		Text.Anchor = TextAnchor.UpperCenter;
		Label(new Rect(0f, curY, width, 30f), customLabel ?? ((string)"NoneBrackets".Translate()));
		Text.Anchor = TextAnchor.UpperLeft;
		curY += 25f;
		GUI.color = Color.white;
	}

	public static void NoneLabelCenteredVertically(Rect rect, string customLabel = null)
	{
		GUI.color = Color.gray;
		Text.Anchor = TextAnchor.MiddleCenter;
		Label(rect, customLabel ?? ((string)"NoneBrackets".Translate()));
		Text.Anchor = TextAnchor.UpperLeft;
		GUI.color = Color.white;
	}

	public static void DraggableBar(Rect barRect, Texture2D barTexture, Texture2D barHighlightTexture, Texture2D emptyBarTex, Texture2D dragBarTex, ref bool draggingBar, float barValue, ref float targetValue, IEnumerable<float> bandPercentages = null, int increments = 20, float min = 0f, float max = 1f)
	{
		bool flag = Mouse.IsOver(barRect);
		FillableBar(barRect, Mathf.Min(barValue, 1f), flag ? barHighlightTexture : barTexture, emptyBarTex, doBorder: true);
		if (bandPercentages != null)
		{
			foreach (float bandPercentage in bandPercentages)
			{
				DrawDraggableBarThreshold(barRect, bandPercentage, barValue);
			}
		}
		float num = Mathf.Clamp(Mathf.Round((Event.current.mousePosition.x - barRect.x) / barRect.width * (float)increments) / (float)increments, min, max);
		Event current2 = Event.current;
		if (current2.type == EventType.MouseDown && current2.button == 0 && flag)
		{
			targetValue = num;
			draggingBar = true;
			current2.Use();
		}
		if ((UnityGUIBugsFixer.MouseDrag() & draggingBar) && flag)
		{
			if (Math.Abs(num - targetValue) > float.Epsilon)
			{
				SoundDefOf.DragSlider.PlayOneShotOnCamera();
			}
			targetValue = num;
			if (Event.current.type == EventType.MouseDrag)
			{
				current2.Use();
			}
		}
		if ((current2.type == EventType.MouseUp && current2.button == 0) & draggingBar)
		{
			draggingBar = false;
			current2.Use();
		}
		DrawDraggableBarTarget(barRect, draggingBar ? num : targetValue, dragBarTex);
		GUI.color = Color.white;
	}

	private static void DrawDraggableBarThreshold(Rect rect, float percent, float curValue)
	{
		Rect position = new Rect
		{
			x = rect.x + 3f + (rect.width - 8f) * percent,
			y = rect.y + rect.height - 9f,
			width = 2f,
			height = 6f
		};
		if (curValue < percent)
		{
			GUI.DrawTexture(position, BaseContent.GreyTex);
		}
		else
		{
			GUI.DrawTexture(position, BaseContent.BlackTex);
		}
	}

	private static void DrawDraggableBarTarget(Rect rect, float percent, Texture2D targetTex)
	{
		float num = Mathf.Round((rect.width - 8f) * percent);
		GUI.DrawTexture(new Rect
		{
			x = rect.x + 3f + num,
			y = rect.y,
			width = 2f,
			height = rect.height
		}, targetTex);
		float num2 = UIScaling.AdjustCoordToUIScalingFloor(rect.x + 2f + num);
		float xMax = UIScaling.AdjustCoordToUIScalingCeil(num2 + 4f);
		Rect obj = new Rect
		{
			y = rect.y - 3f,
			height = 5f,
			xMin = num2,
			xMax = xMax
		};
		GUI.DrawTexture(obj, targetTex);
		Rect position = obj;
		position.y = rect.yMax - 2f;
		GUI.DrawTexture(position, targetTex);
	}

	public static void Dropdown<Target, Payload>(Rect rect, Target target, Func<Target, Payload> getPayload, Func<Target, IEnumerable<DropdownMenuElement<Payload>>> menuGenerator, string buttonLabel = null, Texture2D buttonIcon = null, string dragLabel = null, Texture2D dragIcon = null, Action dropdownOpened = null, bool paintable = false)
	{
		Dropdown(rect, target, Color.white, getPayload, menuGenerator, buttonLabel, buttonIcon, dragLabel, dragIcon, dropdownOpened, paintable);
	}

	public static void Dropdown<Target, Payload>(Rect rect, Target target, Color iconColor, Func<Target, Payload> getPayload, Func<Target, IEnumerable<DropdownMenuElement<Payload>>> menuGenerator, string buttonLabel = null, Texture2D buttonIcon = null, string dragLabel = null, Texture2D dragIcon = null, Action dropdownOpened = null, bool paintable = false, float? contractButtonIcon = null)
	{
		MouseoverSounds.DoRegion(rect);
		DraggableResult draggableResult;
		if (buttonIcon != null)
		{
			DrawHighlightIfMouseover(rect);
			GUI.color = iconColor;
			Rect rect2 = rect;
			if (contractButtonIcon.HasValue)
			{
				rect2 = rect2.ContractedBy(contractButtonIcon.Value);
			}
			DrawTextureFitted(rect2, buttonIcon, 1f);
			GUI.color = Color.white;
			draggableResult = ButtonInvisibleDraggable(rect);
		}
		else
		{
			draggableResult = ButtonTextDraggable(rect, buttonLabel);
		}
		if (draggableResult == DraggableResult.Pressed)
		{
			List<FloatMenuOption> options = (from opt in menuGenerator(target)
				select opt.option).ToList();
			Find.WindowStack.Add(new FloatMenu(options));
			dropdownOpened?.Invoke();
		}
		else if (paintable && draggableResult == DraggableResult.Dragged)
		{
			dropdownPainting = true;
			dropdownPainting_Payload = getPayload(target);
			dropdownPainting_Type = typeof(Payload);
			dropdownPainting_Text = ((dragLabel != null) ? dragLabel : buttonLabel);
			dropdownPainting_Icon = ((dragIcon != null) ? dragIcon : buttonIcon);
		}
		else
		{
			if (!paintable || !dropdownPainting || !Mouse.IsOver(rect) || !(dropdownPainting_Type == typeof(Payload)))
			{
				return;
			}
			FloatMenuOption floatMenuOption = (from opt in menuGenerator(target)
				where object.Equals(opt.payload, dropdownPainting_Payload)
				select opt.option).FirstOrDefault();
			if (floatMenuOption != null && !floatMenuOption.Disabled)
			{
				Payload x = getPayload(target);
				floatMenuOption.action();
				Payload y = getPayload(target);
				if (!EqualityComparer<Payload>.Default.Equals(x, y))
				{
					SoundDefOf.Click.PlayOneShotOnCamera();
				}
			}
		}
	}

	public static void MouseAttachedLabel(string label, float xOffset = 0f, float yOffset = 0f, Color? colorOverride = null)
	{
		Rect rect = CreateMouseAttachedLabelRect(label, xOffset, yOffset);
		if (colorOverride.HasValue)
		{
			GUI.color = colorOverride.Value;
		}
		Label(rect, label);
		GUI.color = Color.white;
	}

	public static void MouseAttachedLabel(TaggedString label, float xOffset = 0f, float yOffset = 0f, Color? colorOverride = null)
	{
		Rect rect = CreateMouseAttachedLabelRect(label, xOffset, yOffset);
		if (colorOverride.HasValue)
		{
			GUI.color = colorOverride.Value;
		}
		Label(rect, label);
		GUI.color = Color.white;
	}

	public static void WorldAttachedLabel(Vector3 worldPos, string label, float xOffset = 0f, float yOffset = 0f, Color? colorOverride = null)
	{
		Vector3 vector = Find.WorldCamera.WorldToScreenPoint(worldPos);
		vector.y = (float)Screen.height - vector.y;
		vector /= Prefs.UIScale;
		Rect rect = CreateAttachedLabelRect(vector, label, xOffset, yOffset);
		if (colorOverride.HasValue)
		{
			GUI.color = colorOverride.Value;
		}
		Label(rect, label);
		GUI.color = Color.white;
	}

	private static Rect CreateMouseAttachedLabelRect(string label, float xOffset, float yOffset)
	{
		return CreateAttachedLabelRect(Event.current.mousePosition, label, xOffset, yOffset);
	}

	private static Rect CreateAttachedLabelRect(Vector2 screenPosition, string label, float xOffset, float yOffset)
	{
		Rect rect = new Rect(screenPosition.x + 8f + xOffset, screenPosition.y + 8f + yOffset, 32f, 32f);
		GUI.color = Color.white;
		Text.Font = GameFont.Small;
		Rect result = new Rect(rect.xMax, rect.y, 9999f, 100f);
		Vector2 vector = Text.CalcSize(label);
		result.height = Mathf.Max(result.height, vector.y);
		GUI.DrawTexture(new Rect(result.x - vector.x * 0.1f, result.y, vector.x * 1.2f, vector.y), TexUI.GrayTextBG);
		return result;
	}

	public static void WidgetsOnGUI()
	{
		if (Event.current.rawType == EventType.MouseUp || Input.GetMouseButtonUp(0))
		{
			checkboxPainting = false;
			dropdownPainting = false;
		}
		if (checkboxPainting)
		{
			GenUI.DrawMouseAttachment(checkboxPaintingState ? CheckboxOnTex : CheckboxOffTex);
		}
		if (dropdownPainting)
		{
			GenUI.DrawMouseAttachment(dropdownPainting_Icon, dropdownPainting_Text);
		}
	}
}
