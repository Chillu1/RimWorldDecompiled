using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEngine;
using Verse.Sound;

namespace Verse
{
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

		public struct DropdownMenuElement<Payload>
		{
			public FloatMenuOption option;

			public Payload payload;
		}

		public static Stack<bool> mouseOverScrollViewStack;

		public static readonly GUIStyle EmptyStyle;

		[TweakValue("Input", 0f, 100f)]
		private static float DragStartDistanceSquared;

		private static readonly Color InactiveColor;

		private static readonly Texture2D DefaultBarBgTex;

		private static readonly Texture2D BarFullTexHor;

		public static readonly Texture2D CheckboxOnTex;

		public static readonly Texture2D CheckboxOffTex;

		public static readonly Texture2D CheckboxPartialTex;

		public const float CheckboxSize = 24f;

		public const float RadioButtonSize = 24f;

		private static readonly Texture2D RadioButOnTex;

		private static readonly Texture2D RadioButOffTex;

		private static readonly Texture2D FillArrowTexRight;

		private static readonly Texture2D FillArrowTexLeft;

		private static readonly Texture2D PlaceholderIconTex;

		private const int FillableBarBorderWidth = 3;

		private const int MaxFillChangeArrowHeight = 16;

		private const int FillChangeArrowWidth = 8;

		public const float CloseButtonSize = 18f;

		public const float CloseButtonMargin = 4f;

		public const float BackButtonWidth = 120f;

		public const float BackButtonHeight = 40f;

		public const float BackButtonMargin = 16f;

		private static readonly Texture2D ShadowAtlas;

		private static readonly Texture2D ButtonBGAtlas;

		private static readonly Texture2D ButtonBGAtlasMouseover;

		private static readonly Texture2D ButtonBGAtlasClick;

		private static readonly Texture2D FloatRangeSliderTex;

		public static readonly Texture2D LightHighlight;

		private static readonly Rect DefaultTexCoords;

		private static readonly Rect LinkedTexCoords;

		[TweakValue("Input", 0f, 100f)]
		private static int IntEntryButtonWidth;

		private static Texture2D LineTexAA;

		private static readonly Rect LineRect;

		private static readonly Material LineMat;

		private static readonly Texture2D AltTexture;

		public static readonly Color NormalOptionColor;

		public static readonly Color MouseoverOptionColor;

		private static Dictionary<string, float> LabelCache;

		private const float TileSize = 64f;

		public static readonly Color SeparatorLabelColor;

		private static readonly Color SeparatorLineColor;

		private const float SeparatorLabelHeight = 20f;

		public const float ListSeparatorHeight = 25f;

		private static bool checkboxPainting;

		private static bool checkboxPaintingState;

		public static readonly Texture2D ButtonSubtleAtlas;

		private static readonly Texture2D ButtonBarTex;

		public const float ButtonSubtleDefaultMarginPct = 0.15f;

		private static int buttonInvisibleDraggable_activeControl;

		private static bool buttonInvisibleDraggable_dragged;

		private static Vector3 buttonInvisibleDraggable_mouseStart;

		public const float RangeControlIdealHeight = 31f;

		public const float RangeControlCompactHeight = 28f;

		private const float RangeSliderSize = 16f;

		private static readonly Color RangeControlTextColor;

		private static int draggingId;

		private static RangeEnd curDragEnd;

		private static float lastDragSliderSoundTime;

		private static float FillableBarChangeRateDisplayRatio;

		public static int MaxFillableBarChangeRate;

		private static readonly Color WindowBGBorderColor;

		public static readonly Color WindowBGFillColor;

		private static readonly Color MenuSectionBGFillColor;

		private static readonly Color MenuSectionBGBorderColor;

		private static readonly Color TutorWindowBGFillColor;

		private static readonly Color TutorWindowBGBorderColor;

		private static readonly Color OptionUnselectedBGFillColor;

		private static readonly Color OptionUnselectedBGBorderColor;

		private static readonly Color OptionSelectedBGFillColor;

		private static readonly Color OptionSelectedBGBorderColor;

		public const float InfoCardButtonSize = 24f;

		private static bool dropdownPainting;

		private static object dropdownPainting_Payload;

		private static Type dropdownPainting_Type;

		private static string dropdownPainting_Text;

		private static Texture2D dropdownPainting_Icon;

		static Widgets()
		{
			mouseOverScrollViewStack = new Stack<bool>();
			EmptyStyle = new GUIStyle();
			DragStartDistanceSquared = 20f;
			InactiveColor = new Color(0.37f, 0.37f, 0.37f, 0.8f);
			DefaultBarBgTex = BaseContent.BlackTex;
			BarFullTexHor = SolidColorMaterials.NewSolidColorTexture(new Color(0.2f, 0.8f, 0.85f));
			CheckboxOnTex = ContentFinder<Texture2D>.Get("UI/Widgets/CheckOn");
			CheckboxOffTex = ContentFinder<Texture2D>.Get("UI/Widgets/CheckOff");
			CheckboxPartialTex = ContentFinder<Texture2D>.Get("UI/Widgets/CheckPartial");
			RadioButOnTex = ContentFinder<Texture2D>.Get("UI/Widgets/RadioButOn");
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
			LineRect = new Rect(0f, 0f, 1f, 1f);
			LineMat = null;
			AltTexture = SolidColorMaterials.NewSolidColorTexture(new Color(1f, 1f, 1f, 0.05f));
			NormalOptionColor = new Color(0.8f, 0.85f, 1f);
			MouseoverOptionColor = Color.yellow;
			LabelCache = new Dictionary<string, float>();
			SeparatorLabelColor = new Color(0.8f, 0.8f, 0.8f, 1f);
			SeparatorLineColor = new Color(0.3f, 0.3f, 0.3f, 1f);
			checkboxPainting = false;
			checkboxPaintingState = false;
			ButtonSubtleAtlas = ContentFinder<Texture2D>.Get("UI/Widgets/ButtonSubtleAtlas");
			ButtonBarTex = SolidColorMaterials.NewSolidColorTexture(new ColorInt(78, 109, 129, 130).ToColor);
			buttonInvisibleDraggable_activeControl = 0;
			buttonInvisibleDraggable_dragged = false;
			buttonInvisibleDraggable_mouseStart = Vector3.zero;
			RangeControlTextColor = new Color(0.6f, 0.6f, 0.6f);
			draggingId = 0;
			curDragEnd = RangeEnd.None;
			lastDragSliderSoundTime = -1f;
			FillableBarChangeRateDisplayRatio = 1E+08f;
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
			LineMat = (Material)typeof(GUI).GetMethod("get_blendMaterial", BindingFlags.Static | BindingFlags.NonPublic).Invoke(null, null);
		}

		public static bool CanDrawIconFor(Def def)
		{
			BuildableDef buildableDef;
			if ((buildableDef = (def as BuildableDef)) != null)
			{
				return buildableDef.uiIcon != null;
			}
			FactionDef factionDef;
			if ((factionDef = (def as FactionDef)) != null)
			{
				return factionDef.FactionIcon != null;
			}
			return false;
		}

		public static void DefIcon(Rect rect, Def def, ThingDef stuffDef = null, float scale = 1f, bool drawPlaceholder = false)
		{
			BuildableDef buildableDef;
			if ((buildableDef = (def as BuildableDef)) != null)
			{
				rect.position += new Vector2(buildableDef.uiIconOffset.x * rect.size.x, buildableDef.uiIconOffset.y * rect.size.y);
			}
			ThingDef thingDef;
			RecipeDef recipeDef;
			TerrainDef terrainDef;
			FactionDef factionDef;
			if ((thingDef = (def as ThingDef)) != null)
			{
				ThingIcon(rect, thingDef, stuffDef, scale);
			}
			else if ((recipeDef = (def as RecipeDef)) != null && recipeDef.ProducedThingDef != null)
			{
				ThingIcon(rect, recipeDef.ProducedThingDef, null, scale);
			}
			else if ((terrainDef = (def as TerrainDef)) != null && terrainDef.uiIcon != null)
			{
				GUI.color = terrainDef.uiIconColor;
				DrawTextureFitted(rect, terrainDef.uiIcon, scale, Vector2.one, CroppedTerrainTextureRect(terrainDef.uiIcon));
				GUI.color = Color.white;
			}
			else if ((factionDef = (def as FactionDef)) != null)
			{
				if (!factionDef.colorSpectrum.NullOrEmpty())
				{
					GUI.color = factionDef.colorSpectrum.FirstOrDefault();
				}
				DrawTextureFitted(rect, factionDef.FactionIcon, scale);
				GUI.color = Color.white;
			}
			else if (drawPlaceholder)
			{
				DrawTextureFitted(rect, PlaceholderIconTex, scale);
			}
		}

		public static void ThingIcon(Rect rect, Thing thing, float alpha = 1f)
		{
			thing = thing.GetInnerIfMinified();
			GUI.color = thing.DrawColor;
			float resolvedIconAngle = 0f;
			Texture resolvedIcon;
			if (!thing.def.uiIconPath.NullOrEmpty())
			{
				resolvedIcon = thing.def.uiIcon;
				resolvedIconAngle = thing.def.uiIconAngle;
				rect.position += new Vector2(thing.def.uiIconOffset.x * rect.size.x, thing.def.uiIconOffset.y * rect.size.y);
			}
			else if (thing is Pawn || thing is Corpse)
			{
				Pawn pawn = thing as Pawn;
				if (pawn == null)
				{
					pawn = ((Corpse)thing).InnerPawn;
				}
				if (!pawn.RaceProps.Humanlike)
				{
					if (!pawn.Drawer.renderer.graphics.AllResolved)
					{
						pawn.Drawer.renderer.graphics.ResolveAllGraphics();
					}
					Material material = pawn.Drawer.renderer.graphics.nakedGraphic.MatAt(Rot4.East);
					resolvedIcon = material.mainTexture;
					GUI.color = material.color;
				}
				else
				{
					rect = rect.ScaledBy(1.8f);
					rect.y += 3f;
					rect = rect.Rounded();
					resolvedIcon = PortraitsCache.Get(pawn, new Vector2(rect.width, rect.height));
				}
			}
			else
			{
				resolvedIcon = thing.Graphic.ExtractInnerGraphicFor(thing).MatAt(thing.def.defaultPlacingRot).mainTexture;
			}
			if (alpha != 1f)
			{
				Color color = GUI.color;
				color.a *= alpha;
				GUI.color = color;
			}
			ThingIconWorker(rect, thing.def, resolvedIcon, resolvedIconAngle);
			GUI.color = Color.white;
		}

		public static void ThingIcon(Rect rect, ThingDef thingDef, ThingDef stuffDef = null, float scale = 1f)
		{
			if (!(thingDef.uiIcon == null) && !(thingDef.uiIcon == BaseContent.BadTex))
			{
				Texture2D resolvedIcon = thingDef.uiIcon;
				Graphic_Appearances graphic_Appearances;
				if ((graphic_Appearances = (thingDef.graphic as Graphic_Appearances)) != null)
				{
					resolvedIcon = (Texture2D)graphic_Appearances.SubGraphicFor(stuffDef).MatAt(thingDef.defaultPlacingRot).mainTexture;
				}
				if (stuffDef != null)
				{
					GUI.color = thingDef.GetColorForStuff(stuffDef);
				}
				else
				{
					GUI.color = (thingDef.MadeFromStuff ? thingDef.GetColorForStuff(GenStuff.DefaultStuffFor(thingDef)) : thingDef.uiIconColor);
				}
				ThingIconWorker(rect, thingDef, resolvedIcon, thingDef.uiIconAngle, scale);
				GUI.color = Color.white;
			}
		}

		private static void ThingIconWorker(Rect rect, ThingDef thingDef, Texture resolvedIcon, float resolvedIconAngle, float scale = 1f)
		{
			Vector2 texProportions = new Vector2(resolvedIcon.width, resolvedIcon.height);
			Rect texCoords = DefaultTexCoords;
			if (thingDef.graphicData != null)
			{
				texProportions = thingDef.graphicData.drawSize.RotatedBy(thingDef.defaultPlacingRot);
				if (thingDef.uiIconPath.NullOrEmpty() && thingDef.graphicData.linkFlags != 0)
				{
					texCoords = LinkedTexCoords;
				}
			}
			DrawTextureFitted(rect, resolvedIcon, GenUI.IconDrawScale(thingDef) * scale, texProportions, texCoords, resolvedIconAngle);
		}

		public static Rect CroppedTerrainTextureRect(Texture2D tex)
		{
			return new Rect(0f, 0f, 64f / (float)tex.width, 64f / (float)tex.height);
		}

		public static void DrawAltRect(Rect rect)
		{
			GUI.DrawTexture(rect, AltTexture);
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
				Rect screenRect = new Rect(start.x, start.y - 0.5f * num5, num3, width);
				GL.PushMatrix();
				GL.MultMatrix(m);
				Graphics.DrawTexture(screenRect, LineTexAA, LineRect, 0, 0, 0, 0, color, LineMat);
				GL.PopMatrix();
			}
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

		public static void DrawBox(Rect rect, int thickness = 1)
		{
			Vector2 b = new Vector2(rect.x, rect.y);
			Vector2 a = new Vector2(rect.x + rect.width, rect.y + rect.height);
			if (b.x > a.x)
			{
				float x = b.x;
				b.x = a.x;
				a.x = x;
			}
			if (b.y > a.y)
			{
				float y = b.y;
				b.y = a.y;
				a.y = y;
			}
			Vector3 vector = a - b;
			GUI.DrawTexture(new Rect(b.x, b.y, thickness, vector.y), BaseContent.WhiteTex);
			GUI.DrawTexture(new Rect(a.x - (float)thickness, b.y, thickness, vector.y), BaseContent.WhiteTex);
			GUI.DrawTexture(new Rect(b.x + (float)thickness, b.y, vector.x - (float)(thickness * 2), thickness), BaseContent.WhiteTex);
			GUI.DrawTexture(new Rect(b.x + (float)thickness, a.y - (float)thickness, vector.x - (float)(thickness * 2), thickness), BaseContent.WhiteTex);
		}

		public static void LabelCacheHeight(ref Rect rect, string label, bool renderLabel = true, bool forceInvalidation = false)
		{
			bool flag = LabelCache.ContainsKey(label);
			float num = 0f;
			if (forceInvalidation)
			{
				flag = false;
			}
			num = (rect.height = ((!flag) ? Text.CalcHeight(label, rect.width) : LabelCache[label]));
			if (renderLabel)
			{
				Label(rect, label);
			}
		}

		public static void Label(Rect rect, GUIContent content)
		{
			GUI.Label(rect, content, Text.CurFontStyle);
		}

		public static void Label(Rect rect, string label)
		{
			Rect position = rect;
			float num = Prefs.UIScale / 2f;
			if (Prefs.UIScale > 1f && Math.Abs(num - Mathf.Floor(num)) > float.Epsilon)
			{
				position.xMin = AdjustCoordToUIScalingFloor(rect.xMin);
				position.yMin = AdjustCoordToUIScalingFloor(rect.yMin);
				position.xMax = AdjustCoordToUIScalingCeil(rect.xMax + 1E-05f);
				position.yMax = AdjustCoordToUIScalingCeil(rect.yMax + 1E-05f);
			}
			GUI.Label(position, label, Text.CurFontStyle);
		}

		public static void Label(Rect rect, TaggedString label)
		{
			Label(rect, label.Resolve());
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
			bool num = takeScrollbarSpaceEvenIfNoScrollbar || Text.CalcHeight(label, rect.width) > rect.height;
			bool flag = num && (!dontConsumeScrollEventsIfNoScrollbar || Text.CalcHeight(label, rect.width - 16f) > rect.height);
			float num2 = rect.width;
			if (num)
			{
				num2 -= 16f;
			}
			float curY;
			if (longLabel)
			{
				curY = 0f;
				LongLabel(0f, num2, label, ref curY, draw: false);
			}
			else
			{
				curY = Text.CalcHeight(label, num2);
			}
			Rect rect2 = new Rect(0f, 0f, num2, Mathf.Max(curY + 5f, rect.height));
			if (flag)
			{
				BeginScrollView(rect, ref scrollbarPosition, rect2);
			}
			else
			{
				GUI.BeginGroup(rect);
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
				GUI.EndGroup();
			}
		}

		public static void DefLabelWithIcon(Rect rect, Def def, float iconMargin = 2f, float textOffsetX = 6f)
		{
			DrawHighlightIfMouseover(rect);
			TooltipHandler.TipRegion(rect, def.description);
			GUI.BeginGroup(rect);
			Rect rect2 = new Rect(0f, 0f, rect.height, rect.height);
			if (iconMargin != 0f)
			{
				rect2 = rect2.ContractedBy(iconMargin);
			}
			DefIcon(rect2, def, null, 1f, drawPlaceholder: true);
			Rect rect3 = new Rect(rect2.xMax + textOffsetX, 0f, rect.width, rect.height);
			Text.Anchor = TextAnchor.MiddleLeft;
			Text.WordWrap = false;
			Label(rect3, def.LabelCap);
			Text.Anchor = TextAnchor.UpperLeft;
			Text.WordWrap = true;
			GUI.EndGroup();
		}

		public static void HyperlinkWithIcon(Rect rect, Dialog_InfoCard.Hyperlink hyperlink, string text = null, float iconMargin = 2f, float textOffsetX = 6f)
		{
			string label = text ?? hyperlink.Label.CapitalizeFirst();
			GUI.BeginGroup(rect);
			Rect rect2 = new Rect(0f, 0f, rect.height, rect.height);
			if (iconMargin != 0f)
			{
				rect2 = rect2.ContractedBy(iconMargin);
			}
			if (hyperlink.thing != null)
			{
				ThingIcon(rect2, hyperlink.thing);
			}
			else
			{
				DefIcon(rect2, hyperlink.def, null, 1f, drawPlaceholder: true);
			}
			Rect rect3 = new Rect(rect2.xMax + textOffsetX, 0f, rect.width, rect.height);
			Text.Anchor = TextAnchor.MiddleLeft;
			Text.WordWrap = false;
			ButtonText(rect3, label, drawBackground: false, doMouseoverSound: false, active: false);
			if (ButtonInvisible(rect3))
			{
				hyperlink.OpenDialog();
			}
			Text.Anchor = TextAnchor.UpperLeft;
			Text.WordWrap = true;
			GUI.EndGroup();
		}

		public static void DrawNumberOnMap(Vector2 screenPos, int number, Color textColor)
		{
			Text.Anchor = TextAnchor.MiddleCenter;
			Text.Font = GameFont.Medium;
			Rect rect = new Rect(screenPos.x - 20f, screenPos.y - 15f, 40f, 30f);
			GUI.DrawTexture(rect, TexUI.GrayBg);
			GUI.color = textColor;
			Label(rect, number.ToStringCached());
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
				MouseoverSounds.DoRegion(rect);
				bool flag = false;
				DraggableResult draggableResult = ButtonInvisibleDraggable(rect);
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
				if (flag)
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
			if (disabled)
			{
				GUI.color = Color.white;
			}
		}

		public static void CheckboxLabeled(Rect rect, string label, ref bool checkOn, bool disabled = false, Texture2D texChecked = null, Texture2D texUnchecked = null, bool placeCheckboxNearText = false)
		{
			TextAnchor anchor = Text.Anchor;
			Text.Anchor = TextAnchor.MiddleLeft;
			if (placeCheckboxNearText)
			{
				rect.width = Mathf.Min(rect.width, Text.CalcSize(label).x + 24f + 10f);
			}
			Label(rect, label);
			if (!disabled && ButtonInvisible(rect))
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
			CheckboxDraw(rect.x + rect.width - 24f, rect.y, checkOn, disabled);
			Text.Anchor = anchor;
		}

		public static bool CheckboxLabeledSelectable(Rect rect, string label, ref bool selected, ref bool checkOn)
		{
			if (selected)
			{
				DrawHighlight(rect);
			}
			TextAnchor anchor = Text.Anchor;
			Text.Anchor = TextAnchor.MiddleLeft;
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

		private static void CheckboxDraw(float x, float y, bool active, bool disabled, float size = 24f, Texture2D texChecked = null, Texture2D texUnchecked = null)
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
			Texture2D tex;
			switch (state)
			{
			case MultiCheckboxState.On:
				tex = CheckboxOnTex;
				break;
			case MultiCheckboxState.Off:
				tex = CheckboxOffTex;
				break;
			default:
				tex = CheckboxPartialTex;
				break;
			}
			MouseoverSounds.DoRegion(rect);
			MultiCheckboxState multiCheckboxState = (state != MultiCheckboxState.Off) ? MultiCheckboxState.Off : MultiCheckboxState.On;
			bool flag = false;
			DraggableResult draggableResult = ButtonImageDraggable(rect, tex);
			if (paintable && draggableResult == DraggableResult.Dragged)
			{
				checkboxPainting = true;
				checkboxPaintingState = (multiCheckboxState == MultiCheckboxState.On);
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

		public static bool RadioButton(Vector2 topLeft, bool chosen)
		{
			return RadioButton(topLeft.x, topLeft.y, chosen);
		}

		public static bool RadioButton(float x, float y, bool chosen)
		{
			Rect butRect = new Rect(x, y, 24f, 24f);
			RadioButtonDraw(x, y, chosen);
			bool num = ButtonInvisible(butRect);
			if (num && !chosen)
			{
				SoundDefOf.Tick_Tiny.PlayOneShotOnCamera();
			}
			return num;
		}

		public static bool RadioButtonLabeled(Rect rect, string labelText, bool chosen)
		{
			TextAnchor anchor = Text.Anchor;
			Text.Anchor = TextAnchor.MiddleLeft;
			Label(rect, labelText);
			Text.Anchor = anchor;
			bool num = ButtonInvisible(rect);
			if (num && !chosen)
			{
				SoundDefOf.Tick_Tiny.PlayOneShotOnCamera();
			}
			RadioButtonDraw(rect.x + rect.width - 24f, rect.y + rect.height / 2f - 12f, chosen);
			return num;
		}

		private static void RadioButtonDraw(float x, float y, bool chosen)
		{
			Color color = GUI.color;
			GUI.color = Color.white;
			GUI.DrawTexture(image: (!chosen) ? RadioButOffTex : RadioButOnTex, position: new Rect(x, y, 24f, 24f));
			GUI.color = color;
		}

		public static bool ButtonText(Rect rect, string label, bool drawBackground = true, bool doMouseoverSound = true, bool active = true)
		{
			return ButtonText(rect, label, drawBackground, doMouseoverSound, NormalOptionColor, active);
		}

		public static bool ButtonText(Rect rect, string label, bool drawBackground, bool doMouseoverSound, Color textColor, bool active = true)
		{
			return ButtonTextWorker(rect, label, drawBackground, doMouseoverSound, textColor, active, draggable: false).AnyPressed();
		}

		public static DraggableResult ButtonTextDraggable(Rect rect, string label, bool drawBackground = true, bool doMouseoverSound = false, bool active = true)
		{
			return ButtonTextDraggable(rect, label, drawBackground, doMouseoverSound, NormalOptionColor, active);
		}

		public static DraggableResult ButtonTextDraggable(Rect rect, string label, bool drawBackground, bool doMouseoverSound, Color textColor, bool active = true)
		{
			return ButtonTextWorker(rect, label, drawBackground, doMouseoverSound, NormalOptionColor, active, draggable: true);
		}

		private static DraggableResult ButtonTextWorker(Rect rect, string label, bool drawBackground, bool doMouseoverSound, Color textColor, bool active, bool draggable)
		{
			TextAnchor anchor = Text.Anchor;
			Color color = GUI.color;
			if (drawBackground)
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
			if (drawBackground)
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

		public static bool CustomButtonText(ref Rect rect, string label, Color bgColor, Color textColor, Color borderColor, bool cacheHeight = false, int borderSize = 1, bool doMouseoverSound = true, bool active = true)
		{
			if (cacheHeight)
			{
				LabelCacheHeight(ref rect, label, renderLabel: false);
			}
			Rect position = new Rect(rect);
			position.x += borderSize;
			position.y += borderSize;
			position.width -= borderSize * 2;
			position.height -= borderSize * 2;
			DrawRectFast(rect, borderColor);
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

		public static bool ButtonTextSubtle(Rect rect, string label, float barPercent = 0f, float textLeftMargin = -1f, SoundDef mouseoverSound = null, Vector2 functionalSizeOffset = default(Vector2))
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
			Label(rect3, label);
			Text.Anchor = TextAnchor.UpperLeft;
			Text.WordWrap = true;
			return ButtonInvisible(rect2, doMouseoverSound: false);
		}

		public static bool ButtonImage(Rect butRect, Texture2D tex, bool doMouseoverSound = true)
		{
			return ButtonImage(butRect, tex, Color.white, doMouseoverSound);
		}

		public static bool ButtonImage(Rect butRect, Texture2D tex, Color baseColor, bool doMouseoverSound = true)
		{
			return ButtonImage(butRect, tex, baseColor, GenUI.MouseoverColor, doMouseoverSound);
		}

		public static bool ButtonImage(Rect butRect, Texture2D tex, Color baseColor, Color mouseoverColor, bool doMouseoverSound = true)
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
			return ButtonInvisible(butRect, doMouseoverSound);
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
			Rect position = (!imageSize.HasValue) ? butRect : new Rect(Mathf.Floor(butRect.x + butRect.width / 2f - imageSize.Value.x / 2f), Mathf.Floor(butRect.y + butRect.height / 2f - imageSize.Value.y / 2f), imageSize.Value.x, imageSize.Value.y);
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

		public static string TextField(Rect rect, string text, int maxLength, Regex inputValidator)
		{
			string text2 = TextField(rect, text);
			if (text2.Length <= maxLength && inputValidator.IsMatch(text2))
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

		public static string TextAreaScrollable(Rect rect, string text, ref Vector2 scrollbarPosition, bool readOnly = false)
		{
			Rect rect2 = new Rect(0f, 0f, rect.width - 16f, Mathf.Max(Text.CalcHeight(text, rect.width) + 10f, rect.height));
			BeginScrollView(rect, ref scrollbarPosition, rect2);
			string result = TextArea(rect2, text, readOnly);
			EndScrollView();
			return result;
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

		public static void TextFieldNumeric<T>(Rect rect, ref T val, ref string buffer, float min = 0f, float max = 1E+09f) where T : struct
		{
			if (buffer == null)
			{
				buffer = val.ToString();
			}
			string text = "TextField" + rect.y.ToString("F0") + rect.x.ToString("F0");
			GUI.SetNextControlName(text);
			string text2 = GUI.TextField(rect, buffer, Text.CurTextFieldStyle);
			if (GUI.GetNameOfFocusedControl() != text)
			{
				ResolveParseNow(buffer, ref val, ref buffer, min, max, force: true);
			}
			else if (text2 != buffer && IsPartiallyOrFullyTypedNumber(ref val, text2, min, max))
			{
				buffer = text2;
				if (text2.IsFullyTypedNumber<T>())
				{
					ResolveParseNow(text2, ref val, ref buffer, min, max, force: false);
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
				if (float.TryParse(edited, out float result2))
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

		public static float HorizontalSlider(Rect rect, float value, float leftValue, float rightValue, bool middleAlignment = false, string label = null, string leftAlignedLabel = null, string rightAlignedLabel = null, float roundTo = -1f)
		{
			if (middleAlignment || !label.NullOrEmpty())
			{
				rect.y += Mathf.Round((rect.height - 16f) / 2f);
			}
			if (!label.NullOrEmpty())
			{
				rect.y += 5f;
			}
			float num = GUI.HorizontalSlider(rect, value, leftValue, rightValue);
			if (!label.NullOrEmpty() || !leftAlignedLabel.NullOrEmpty() || !rightAlignedLabel.NullOrEmpty())
			{
				TextAnchor anchor = Text.Anchor;
				GameFont font = Text.Font;
				Text.Font = GameFont.Tiny;
				float num2 = label.NullOrEmpty() ? 18f : Text.CalcSize(label).y;
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
				SoundDefOf.DragSlider.PlayOneShotOnCamera();
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
			string label = (freq == 1f) ? ((string)"EveryDay".Translate()) : ((!(freq < 1f)) ? ((string)"EveryDays".Translate(freq.ToString("0.##"))) : ((string)"TimesPerDay".Translate((1f / freq).ToString("0.##"))));
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

		public static void FloatRange(Rect rect, int id, ref FloatRange range, float min = 0f, float max = 1f, string labelKey = null, ToStringStyle valueStyle = ToStringStyle.FloatTwo)
		{
			Rect rect2 = rect;
			rect2.xMin += 8f;
			rect2.xMax -= 8f;
			GUI.color = RangeControlTextColor;
			string text = range.min.ToStringByStyle(valueStyle) + " - " + range.max.ToStringByStyle(valueStyle);
			if (labelKey != null)
			{
				text = labelKey.Translate(text);
			}
			GameFont font = Text.Font;
			Text.Font = GameFont.Tiny;
			Text.Anchor = TextAnchor.UpperCenter;
			Rect rect3 = rect2;
			rect3.yMin -= 2f;
			rect3.height = Mathf.Max(rect3.height, Text.CalcHeight(text, rect3.width));
			Label(rect3, text);
			Text.Anchor = TextAnchor.UpperLeft;
			Rect position = new Rect(rect2.x, rect2.yMax - 8f - 1f, rect2.width, 2f);
			GUI.DrawTexture(position, BaseContent.WhiteTex);
			GUI.color = Color.white;
			float num = rect2.x + rect2.width * Mathf.InverseLerp(min, max, range.min);
			float num2 = rect2.x + rect2.width * Mathf.InverseLerp(min, max, range.max);
			Rect position2 = new Rect(num - 16f, position.center.y - 8f, 16f, 16f);
			GUI.DrawTexture(position2, FloatRangeSliderTex);
			Rect position3 = new Rect(num2 + 16f, position.center.y - 8f, -16f, 16f);
			GUI.DrawTexture(position3, FloatRangeSliderTex);
			if (curDragEnd != 0 && (Event.current.type == EventType.MouseUp || Event.current.rawType == EventType.MouseDown))
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
						float num3 = Mathf.Abs(x - position2.xMax);
						float num4 = Mathf.Abs(x - (position3.x - 16f));
						curDragEnd = ((num3 < num4) ? RangeEnd.Min : RangeEnd.Max);
					}
					flag = true;
					Event.current.Use();
					SoundDefOf.DragSlider.PlayOneShotOnCamera();
				}
				if (flag || (curDragEnd != 0 && Event.current.type == EventType.MouseDrag))
				{
					float value = (Event.current.mousePosition.x - rect2.x) / rect2.width * (max - min) + min;
					value = Mathf.Clamp(value, min, max);
					if (curDragEnd == RangeEnd.Min)
					{
						if (value != range.min)
						{
							range.min = value;
							if (range.max < range.min)
							{
								range.max = range.min;
							}
							CheckPlayDragSliderSound();
						}
					}
					else if (curDragEnd == RangeEnd.Max && value != range.max)
					{
						range.max = value;
						if (range.min > range.max)
						{
							range.min = range.max;
						}
						CheckPlayDragSliderSound();
					}
					Event.current.Use();
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
			Text.Font = GameFont.Tiny;
			Text.Anchor = TextAnchor.UpperCenter;
			Rect rect3 = rect2;
			rect3.yMin -= 2f;
			Label(rect3, text);
			Text.Anchor = TextAnchor.UpperLeft;
			Rect position = new Rect(rect2.x, rect2.yMax - 8f - 1f, rect2.width, 2f);
			GUI.DrawTexture(position, BaseContent.WhiteTex);
			GUI.color = Color.white;
			float num = rect2.x + rect2.width * (float)(range.min - min) / (float)(max - min);
			float num2 = rect2.x + rect2.width * (float)(range.max - min) / (float)(max - min);
			Rect position2 = new Rect(num - 16f, position.center.y - 8f, 16f, 16f);
			GUI.DrawTexture(position2, FloatRangeSliderTex);
			Rect position3 = new Rect(num2 + 16f, position.center.y - 8f, -16f, 16f);
			GUI.DrawTexture(position3, FloatRangeSliderTex);
			if (curDragEnd != 0 && (Event.current.type == EventType.MouseUp || Event.current.rawType == EventType.MouseDown))
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
						float num3 = Mathf.Abs(x - position2.xMax);
						float num4 = Mathf.Abs(x - (position3.x - 16f));
						curDragEnd = ((num3 < num4) ? RangeEnd.Min : RangeEnd.Max);
					}
					flag = true;
					Event.current.Use();
					SoundDefOf.DragSlider.PlayOneShotOnCamera();
				}
				if (flag || (curDragEnd != 0 && Event.current.type == EventType.MouseDrag))
				{
					int num5 = Mathf.RoundToInt(Mathf.Clamp((Event.current.mousePosition.x - rect2.x) / rect2.width * (float)(max - min) + (float)min, min, max));
					if (curDragEnd == RangeEnd.Min)
					{
						if (num5 != range.min)
						{
							range.min = num5;
							if (range.min > max - minWidth)
							{
								range.min = max - minWidth;
							}
							int num6 = Mathf.Max(min, range.min + minWidth);
							if (range.max < num6)
							{
								range.max = num6;
							}
							CheckPlayDragSliderSound();
						}
					}
					else if (curDragEnd == RangeEnd.Max && num5 != range.max)
					{
						range.max = num5;
						if (range.max < min + minWidth)
						{
							range.max = min + minWidth;
						}
						int num7 = Mathf.Min(max, range.max - minWidth);
						if (range.min > num7)
						{
							range.min = num7;
						}
						CheckPlayDragSliderSound();
					}
					Event.current.Use();
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
			string label = (range == RimWorld.QualityRange.All) ? ((string)"AnyQuality".Translate()) : ((range.max != range.min) ? (range.min.GetLabel() + " - " + range.max.GetLabel()) : ((string)"OnlyQuality".Translate(range.min.GetLabel())));
			GameFont font = Text.Font;
			Text.Font = GameFont.Tiny;
			Text.Anchor = TextAnchor.UpperCenter;
			Rect rect3 = rect2;
			rect3.yMin -= 2f;
			Label(rect3, label);
			Text.Anchor = TextAnchor.UpperLeft;
			Rect position = new Rect(rect2.x, rect2.yMax - 8f - 1f, rect2.width, 2f);
			GUI.DrawTexture(position, BaseContent.WhiteTex);
			GUI.color = Color.white;
			int length = Enum.GetValues(typeof(QualityCategory)).Length;
			float num = rect2.x + rect2.width / (float)(length - 1) * (float)(int)range.min;
			float num2 = rect2.x + rect2.width / (float)(length - 1) * (float)(int)range.max;
			Rect position2 = new Rect(num - 16f, position.center.y - 8f, 16f, 16f);
			GUI.DrawTexture(position2, FloatRangeSliderTex);
			Rect position3 = new Rect(num2 + 16f, position.center.y - 8f, -16f, 16f);
			GUI.DrawTexture(position3, FloatRangeSliderTex);
			if (curDragEnd != 0 && (Event.current.type == EventType.MouseUp || Event.current.type == EventType.MouseDown))
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
						float num3 = Mathf.Abs(x - position2.xMax);
						float num4 = Mathf.Abs(x - (position3.x - 16f));
						curDragEnd = ((num3 < num4) ? RangeEnd.Min : RangeEnd.Max);
					}
					flag = true;
					Event.current.Use();
					SoundDefOf.DragSlider.PlayOneShotOnCamera();
				}
				if (flag || (curDragEnd != 0 && Event.current.type == EventType.MouseDrag))
				{
					int value = Mathf.RoundToInt((Event.current.mousePosition.x - rect2.x) / rect2.width * (float)(length - 1));
					value = Mathf.Clamp(value, 0, length - 1);
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
					Event.current.Use();
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
			DrawBox(rect);
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

		private static Rect AdjustRectToUIScaling(Rect rect)
		{
			Rect result = rect;
			result.xMin = AdjustCoordToUIScalingFloor(rect.xMin);
			result.yMin = AdjustCoordToUIScalingFloor(rect.yMin);
			result.xMax = AdjustCoordToUIScalingCeil(rect.xMax);
			result.yMax = AdjustCoordToUIScalingCeil(rect.yMax);
			return result;
		}

		public static float AdjustCoordToUIScalingFloor(float coord)
		{
			double num = Prefs.UIScale * coord;
			float num2 = (float)(num - Math.Floor(num)) / Prefs.UIScale;
			return coord - num2;
		}

		public static float AdjustCoordToUIScalingCeil(float coord)
		{
			double num = Prefs.UIScale * coord;
			float num2 = (float)(num - Math.Ceiling(num)) / Prefs.UIScale;
			return coord - num2;
		}

		public static void DrawAtlas(Rect rect, Texture2D atlas, bool drawTop)
		{
			rect.x = Mathf.Round(rect.x);
			rect.y = Mathf.Round(rect.y);
			rect.width = Mathf.Round(rect.width);
			rect.height = Mathf.Round(rect.height);
			rect = AdjustRectToUIScaling(rect);
			float a = (float)atlas.width * 0.25f;
			a = AdjustCoordToUIScalingCeil(GenMath.Min(a, rect.height / 2f, rect.width / 2f));
			GUI.BeginGroup(rect);
			Rect uvRect;
			Rect drawRect;
			if (drawTop)
			{
				drawRect = new Rect(0f, 0f, a, a);
				uvRect = new Rect(0f, 0f, 0.25f, 0.25f);
				DrawTexturePart(drawRect, uvRect, atlas);
				drawRect = new Rect(rect.width - a, 0f, a, a);
				uvRect = new Rect(0.75f, 0f, 0.25f, 0.25f);
				DrawTexturePart(drawRect, uvRect, atlas);
			}
			drawRect = new Rect(0f, rect.height - a, a, a);
			uvRect = new Rect(0f, 0.75f, 0.25f, 0.25f);
			DrawTexturePart(drawRect, uvRect, atlas);
			drawRect = new Rect(rect.width - a, rect.height - a, a, a);
			uvRect = new Rect(0.75f, 0.75f, 0.25f, 0.25f);
			DrawTexturePart(drawRect, uvRect, atlas);
			drawRect = new Rect(a, a, rect.width - a * 2f, rect.height - a * 2f);
			if (!drawTop)
			{
				drawRect.height += a;
				drawRect.y -= a;
			}
			DrawTexturePart(uvRect: new Rect(0.25f, 0.25f, 0.5f, 0.5f), drawRect: drawRect, tex: atlas);
			if (drawTop)
			{
				drawRect = new Rect(a, 0f, rect.width - a * 2f, a);
				uvRect = new Rect(0.25f, 0f, 0.5f, 0.25f);
				DrawTexturePart(drawRect, uvRect, atlas);
			}
			drawRect = new Rect(a, rect.height - a, rect.width - a * 2f, a);
			uvRect = new Rect(0.25f, 0.75f, 0.5f, 0.25f);
			DrawTexturePart(drawRect, uvRect, atlas);
			drawRect = new Rect(0f, a, a, rect.height - a * 2f);
			if (!drawTop)
			{
				drawRect.height += a;
				drawRect.y -= a;
			}
			DrawTexturePart(uvRect: new Rect(0f, 0.25f, 0.25f, 0.5f), drawRect: drawRect, tex: atlas);
			drawRect = new Rect(rect.width - a, a, a, rect.height - a * 2f);
			if (!drawTop)
			{
				drawRect.height += a;
				drawRect.y -= a;
			}
			DrawTexturePart(uvRect: new Rect(0.75f, 0.25f, 0.25f, 0.5f), drawRect: drawRect, tex: atlas);
			GUI.EndGroup();
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
			if (showScrollbars)
			{
				scrollPosition = GUI.BeginScrollView(outRect, scrollPosition, viewRect);
			}
			else
			{
				scrollPosition = GUI.BeginScrollView(outRect, scrollPosition, viewRect, GUIStyle.none, GUIStyle.none);
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

		public static void DrawLightHighlight(Rect rect)
		{
			GUI.DrawTexture(rect, LightHighlight);
		}

		public static void DrawTitleBG(Rect rect)
		{
			GUI.DrawTexture(rect, TexUI.TitleBGTex);
		}

		public static bool InfoCardButton(float x, float y, Thing thing)
		{
			IConstructible constructible = thing as IConstructible;
			if (constructible != null)
			{
				ThingDef thingDef = thing.def.entityDefToBuild as ThingDef;
				if (thingDef != null)
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

		public static bool InfoCardButtonCentered(Rect rect, Thing thing)
		{
			return InfoCardButton(rect.center.x - 12f, rect.center.y - 12f, thing);
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

		private static bool InfoCardButtonWorker(float x, float y)
		{
			Rect rect = new Rect(x, y, 24f, 24f);
			MouseoverSounds.DoRegion(rect);
			TooltipHandler.TipRegionByKey(rect, "DefInfoTip");
			bool result = ButtonImage(rect, TexButton.Info, GUI.color);
			UIHighlighter.HighlightOpportunity(rect, "InfoCard");
			return result;
		}

		public static void DrawTextureFitted(Rect outerRect, Texture tex, float scale)
		{
			DrawTextureFitted(outerRect, tex, scale, new Vector2(tex.width, tex.height), new Rect(0f, 0f, 1f, 1f));
		}

		public static void DrawTextureFitted(Rect outerRect, Texture tex, float scale, Vector2 texProportions, Rect texCoords, float angle = 0f, Material mat = null)
		{
			if (Event.current.type == EventType.Repaint)
			{
				Rect rect = new Rect(0f, 0f, texProportions.x, texProportions.y);
				float num = (!(rect.width / rect.height < outerRect.width / outerRect.height)) ? (outerRect.width / rect.width) : (outerRect.height / rect.height);
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
				GenUI.DrawTextureWithMaterial(rect, tex, mat, texCoords);
				if (angle != 0f)
				{
					GUI.matrix = matrix;
				}
			}
		}

		public static void DrawTextureRotated(Vector2 center, Texture tex, float angle, float scale = 1f)
		{
			float num = (float)tex.width * scale;
			float num2 = (float)tex.height * scale;
			DrawTextureRotated(new Rect(center.x - num / 2f, center.y - num2 / 2f, num, num2), tex, angle);
		}

		public static void DrawTextureRotated(Rect rect, Texture tex, float angle)
		{
			if (Event.current.type == EventType.Repaint)
			{
				if (angle == 0f)
				{
					GUI.DrawTexture(rect, tex);
					return;
				}
				Matrix4x4 matrix = GUI.matrix;
				UI.RotateAroundPivot(angle, rect.center);
				GUI.DrawTexture(rect, tex);
				GUI.matrix = matrix;
			}
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

		public static void Dropdown<Target, Payload>(Rect rect, Target target, Func<Target, Payload> getPayload, Func<Target, IEnumerable<DropdownMenuElement<Payload>>> menuGenerator, string buttonLabel = null, Texture2D buttonIcon = null, string dragLabel = null, Texture2D dragIcon = null, Action dropdownOpened = null, bool paintable = false)
		{
			Dropdown(rect, target, Color.white, getPayload, menuGenerator, buttonLabel, buttonIcon, dragLabel, dragIcon, dropdownOpened, paintable);
		}

		public static void Dropdown<Target, Payload>(Rect rect, Target target, Color iconColor, Func<Target, Payload> getPayload, Func<Target, IEnumerable<DropdownMenuElement<Payload>>> menuGenerator, string buttonLabel = null, Texture2D buttonIcon = null, string dragLabel = null, Texture2D dragIcon = null, Action dropdownOpened = null, bool paintable = false)
		{
			MouseoverSounds.DoRegion(rect);
			DraggableResult draggableResult;
			if (buttonIcon != null)
			{
				DrawHighlightIfMouseover(rect);
				GUI.color = iconColor;
				DrawTextureFitted(rect, buttonIcon, 1f);
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
}
