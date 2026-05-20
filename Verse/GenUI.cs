using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;

namespace Verse;

[StaticConstructorOnStartup]
public static class GenUI
{
	private struct StackedElementRect
	{
		public Rect rect;

		public int elementIndex;

		public StackedElementRect(Rect rect, int elementIndex)
		{
			this.rect = rect;
			this.elementIndex = elementIndex;
		}
	}

	public class AnonymousStackElement
	{
		public Action<Rect> drawer;

		public float width;
	}

	private struct SpacingCache
	{
		private int maxElements;

		private float[] spaces;

		public void Reset(int elem = 16)
		{
			if (spaces == null || maxElements != elem)
			{
				maxElements = elem;
				spaces = new float[maxElements];
				return;
			}
			for (int i = 0; i < maxElements; i++)
			{
				spaces[i] = 0f;
			}
		}

		public float GetSpaceFor(int elem)
		{
			if (spaces == null || maxElements < 1)
			{
				Reset();
			}
			if (elem >= 0 && elem < maxElements)
			{
				return spaces[elem];
			}
			return 0f;
		}

		public void AddSpace(int elem, float space)
		{
			if (spaces == null || maxElements < 1)
			{
				Reset();
			}
			if (elem >= 0 && elem < maxElements)
			{
				spaces[elem] += space;
			}
		}
	}

	public delegate void StackElementDrawer<T>(Rect rect, T element);

	public delegate float StackElementWidthGetter<T>(T element);

	public const float Pad = 10f;

	public const float GapTiny = 4f;

	public const float GapSmall = 10f;

	public const float Gap = 17f;

	public const float GapWide = 26f;

	public const float GapLabel = 22f;

	public const float ListSpacing = 28f;

	public const float MouseAttachIconSize = 32f;

	public const float MouseAttachIconOffset = 8f;

	public const float ScrollBarWidth = 16f;

	public const float HorizontalSliderHeight = 10f;

	public static readonly Vector2 TradeableDrawSize = new Vector2(150f, 45f);

	public static readonly Color MouseoverColor = new Color(0.3f, 0.7f, 0.9f);

	public static readonly Color SubtleMouseoverColor = new Color(0.7f, 0.7f, 0.7f);

	public static readonly Color FillableBar_Green = new Color(0.40392157f, 0.7019608f, 0.28627452f);

	public static readonly Color FillableBar_Empty = new Color(0.03f, 0.035f, 0.05f);

	public static readonly Vector2 MaxWinSize = new Vector2(1010f, 754f);

	public const float SmallIconSize = 24f;

	public const int RootGUIDepth = 50;

	private const float MouseIconSize = 32f;

	private const float MouseIconOffset = 12f;

	private static readonly Material MouseoverBracketMaterial = MaterialPool.MatFrom("UI/Overlays/MouseoverBracketTex", ShaderDatabase.MetaOverlay);

	private static readonly Texture2D UnderShadowTex = ContentFinder<Texture2D>.Get("UI/Misc/ScreenCornerShadow");

	private static readonly Texture2D UIFlash = ContentFinder<Texture2D>.Get("UI/Misc/Flash");

	private static Dictionary<string, Vector2> labelWidthCache = new Dictionary<string, Vector2>();

	private static readonly Vector2 PieceBarSize = new Vector2(100f, 17f);

	public const float PawnDirectClickRadius = 0.4f;

	private static List<Thing> cellThings = new List<Thing>(32);

	private static readonly Texture2D ArrowTex = ContentFinder<Texture2D>.Get("UI/Overlays/Arrow");

	private static List<StackedElementRect> tmpRects = new List<StackedElementRect>();

	private static List<StackedElementRect> tmpRects2 = new List<StackedElementRect>();

	public const float ElementStackDefaultElementMargin = 5f;

	public const float ElementStackDefaultRowMargin = 4f;

	private static SpacingCache spacingCache;

	public static void SetLabelAlign(TextAnchor a)
	{
		Text.Anchor = a;
	}

	public static void ResetLabelAlign()
	{
		Text.Anchor = TextAnchor.UpperLeft;
	}

	public static float BackgroundDarkAlphaForText()
	{
		if (Find.CurrentMap == null)
		{
			return 0f;
		}
		float num = GenCelestial.CurCelestialSunGlow(Find.CurrentMap);
		float num2 = ((Find.CurrentMap.Biome == BiomeDefOf.IceSheet) ? 1f : Mathf.Clamp01(Find.CurrentMap.snowGrid.TotalDepth / 1000f));
		return num * num2 * 0.41f;
	}

	public static void DrawTextWinterShadow(Rect rect)
	{
		float num = BackgroundDarkAlphaForText();
		if (num > 0.001f)
		{
			GUI.color = new Color(1f, 1f, 1f, num);
			GUI.DrawTexture(rect, UnderShadowTex);
			GUI.color = Color.white;
		}
	}

	public static void DrawTextureWithMaterial(Rect rect, Texture texture, Material material, Rect texCoords = default(Rect))
	{
		if (texCoords == default(Rect))
		{
			if (material == null)
			{
				GUI.DrawTexture(rect, texture);
			}
			else if (Event.current.type == EventType.Repaint)
			{
				Color color = (material.shader.SupportsMaskTex() ? GUI.color : new Color(GUI.color.r * 0.5f, GUI.color.g * 0.5f, GUI.color.b * 0.5f, GUI.color.a));
				Graphics.DrawTexture(rect, texture, new Rect(0f, 0f, 1f, 1f), 0, 0, 0, 0, color, material);
			}
		}
		else if (material == null)
		{
			GUI.DrawTextureWithTexCoords(rect, texture, texCoords);
		}
		else if (Event.current.type == EventType.Repaint)
		{
			Color color2 = (material.shader.SupportsMaskTex() ? GUI.color : new Color(GUI.color.r * 0.5f, GUI.color.g * 0.5f, GUI.color.b * 0.5f, GUI.color.a));
			Graphics.DrawTexture(rect, texture, texCoords, 0, 0, 0, 0, color2, material);
		}
	}

	public static void DrawTexturePartWithMaterial(Rect rect, Texture texture, Material material, Rect texCoords)
	{
		if (texCoords == default(Rect))
		{
			if (material == null)
			{
				GUI.DrawTexture(rect, texture);
			}
			else if (Event.current.type == EventType.Repaint)
			{
				Color color = (material.shader.SupportsMaskTex() ? GUI.color : (GUI.color * 0.5f));
				Graphics.DrawTexture(rect, texture, new Rect(0f, 0f, 1f, 1f), 0, 0, 0, 0, color, material);
			}
			return;
		}
		texCoords.y = 1f - texCoords.y - texCoords.height;
		if (material == null)
		{
			GUI.DrawTextureWithTexCoords(rect, texture, texCoords);
		}
		else if (Event.current.type == EventType.Repaint)
		{
			Color color2 = (material.shader.SupportsMaskTex() ? GUI.color : (GUI.color * 0.5f));
			Graphics.DrawTexture(rect, texture, texCoords, 0, 0, 0, 0, color2, material);
		}
	}

	public static float IconDrawScale(ThingDef tDef)
	{
		float num = tDef.uiIconScale;
		if (tDef.uiIconPath.NullOrEmpty() && tDef.graphicData != null)
		{
			num *= Mathf.Min(tDef.graphicData.drawSize.x / (float)tDef.Size.x, tDef.graphicData.drawSize.y / (float)tDef.Size.z);
		}
		return num;
	}

	public static void ErrorDialog(string message)
	{
		if (Find.WindowStack != null)
		{
			Find.WindowStack.Add(new Dialog_MessageBox(message));
		}
	}

	public static void DrawFlash(float centerX, float centerY, float size, float alpha, Color color)
	{
		Rect position = new Rect(centerX - size / 2f, centerY - size / 2f, size, size);
		Color color2 = color;
		color2.a = alpha;
		GUI.color = color2;
		GUI.DrawTexture(position, UIFlash);
		GUI.color = Color.white;
	}

	public static Vector2 GetSizeCached(this string s)
	{
		if (labelWidthCache.Count > 2000 || (Time.frameCount % 40000 == 0 && labelWidthCache.Count > 100))
		{
			labelWidthCache.Clear();
		}
		s = s.StripTags();
		if (!labelWidthCache.TryGetValue(s, out var value))
		{
			value = Text.CalcSize(s);
			labelWidthCache.Add(s, value);
		}
		return value;
	}

	public static float GetWidthCached(this string s)
	{
		return s.GetSizeCached().x;
	}

	public static float GetHeightCached(this string s)
	{
		return s.GetSizeCached().y;
	}

	public static void ClearLabelWidthCache()
	{
		labelWidthCache.Clear();
	}

	public static Rect Rounded(this Rect r)
	{
		return new Rect((int)r.x, (int)r.y, (int)r.width, (int)r.height);
	}

	public static Rect RoundedCeil(this Rect r)
	{
		return new Rect(Mathf.CeilToInt(r.x), Mathf.CeilToInt(r.y), Mathf.CeilToInt(r.width), Mathf.CeilToInt(r.height));
	}

	public static Vector2 Rounded(this Vector2 v)
	{
		return new Vector2((int)v.x, (int)v.y);
	}

	public static float DistFromRect(Rect r, Vector2 p)
	{
		float num = Mathf.Abs(p.x - r.center.x) - r.width / 2f;
		if (num < 0f)
		{
			num = 0f;
		}
		float num2 = Mathf.Abs(p.y - r.center.y) - r.height / 2f;
		if (num2 < 0f)
		{
			num2 = 0f;
		}
		return Mathf.Sqrt(num * num + num2 * num2);
	}

	public static void DrawMouseAttachment(Texture iconTex, string text = "", float angle = 0f, Vector2 offset = default(Vector2), Rect? customRect = null, Color? textColor = null, bool drawTextBackground = false, Color textBgColor = default(Color), Color? iconColor = null, Action<Rect> postDrawAction = null)
	{
		Vector2 mousePosition = Event.current.mousePosition;
		float num = mousePosition.y + 12f;
		if (drawTextBackground && text != "")
		{
			Rect rect;
			if (customRect.HasValue)
			{
				rect = customRect.Value;
			}
			else
			{
				float num2 = Mathf.Min(Text.CalcSize(text).x, 200f);
				float height = Text.CalcHeight(text, 200f);
				float num3 = ((iconTex != null) ? 42f : 0f);
				rect = new Rect(mousePosition.x + 12f - 4f, num + num3, num2 + 8f, height);
			}
			Widgets.DrawBoxSolid(rect, textBgColor);
		}
		if (iconTex != null)
		{
			Rect mouseRect;
			if (customRect.HasValue)
			{
				mouseRect = customRect.Value;
			}
			else
			{
				mouseRect = new Rect(mousePosition.x + 8f, num + 8f, 32f, 32f);
			}
			Find.WindowStack.ImmediateWindow(34003428, mouseRect, WindowLayer.Super, delegate
			{
				Rect rect2 = mouseRect.AtZero();
				rect2.position += new Vector2(offset.x * rect2.size.x, offset.y * rect2.size.y);
				GUI.color = iconColor ?? Color.white;
				Widgets.DrawTextureRotated(rect2, iconTex, angle);
				GUI.color = Color.white;
				postDrawAction?.Invoke(rect2);
			}, doBackground: false, absorbInputAroundWindow: false, 0f);
			num += mouseRect.height + 10f;
		}
		if (text != "")
		{
			Rect textRect = new Rect(mousePosition.x + 12f, num, 200f, 9999f);
			Find.WindowStack.ImmediateWindow(34003429, textRect, WindowLayer.Super, delegate
			{
				GameFont font = Text.Font;
				Text.Font = GameFont.Small;
				GUI.color = textColor ?? Color.white;
				Widgets.Label(textRect.AtZero(), text);
				GUI.color = Color.white;
				Text.Font = font;
			}, doBackground: false, absorbInputAroundWindow: false, 0f);
		}
	}

	public static void DrawMouseAttachment(Texture2D icon)
	{
		Vector2 mousePosition = Event.current.mousePosition;
		Rect mouseRect = new Rect(mousePosition.x + 8f, mousePosition.y + 8f, 32f, 32f);
		Find.WindowStack.ImmediateWindow(34003428, mouseRect, WindowLayer.Super, delegate
		{
			GUI.DrawTexture(mouseRect.AtZero(), icon);
		}, doBackground: false, absorbInputAroundWindow: false, 0f);
	}

	public static void RenderMouseoverBracket()
	{
		Vector3 position = UI.MouseCell().ToVector3ShiftedWithAltitude(AltitudeLayer.MetaOverlays);
		Graphics.DrawMesh(MeshPool.plane10, position, Quaternion.identity, MouseoverBracketMaterial, 0);
	}

	public static void DrawStatusLevel(Need status, Rect rect)
	{
		Widgets.BeginGroup(rect);
		Widgets.Label(new Rect(0f, 2f, rect.width, 25f), status.LabelCap);
		Rect rect2 = new Rect(100f, 3f, PieceBarSize.x, PieceBarSize.y);
		Widgets.FillableBar(rect2, status.CurLevelPercentage);
		Widgets.FillableBarChangeArrows(rect2, status.GUIChangeArrow);
		Widgets.EndGroup();
		if (Mouse.IsOver(rect))
		{
			TooltipHandler.TipRegion(rect, status.GetTipString());
		}
		if (Mouse.IsOver(rect))
		{
			GUI.DrawTexture(rect, TexUI.HighlightTex);
		}
	}

	public static IEnumerable<LocalTargetInfo> TargetsAtMouse(TargetingParameters clickParams, bool thingsOnly = false, ITargetingSource source = null)
	{
		return TargetsAt(UI.MouseMapPosition(), clickParams, thingsOnly, source);
	}

	public static IEnumerable<LocalTargetInfo> TargetsAt(Vector3 clickPos, TargetingParameters clickParams, bool thingsOnly = false, ITargetingSource source = null)
	{
		List<Thing> clickableList = ThingsUnderMouse(clickPos, 0.8f, clickParams, source);
		Thing caster = source?.Caster;
		for (int i = 0; i < clickableList.Count; i++)
		{
			if (!(clickableList[i] is Pawn pawn) || !pawn.IsPsychologicallyInvisible() || caster == null || caster.Faction == pawn.Faction)
			{
				yield return clickableList[i];
			}
		}
		if (!thingsOnly)
		{
			IntVec3 intVec = UI.MouseCell();
			if (intVec.InBounds(Find.CurrentMap, clickParams.mapBoundsContractedBy) && clickParams.CanTarget(new TargetInfo(intVec, Find.CurrentMap), source))
			{
				yield return intVec;
			}
		}
	}

	public static List<Thing> ThingsUnderMouse(Vector3 clickPos, float pawnWideClickRadius, TargetingParameters clickParams, ITargetingSource source = null)
	{
		IntVec3 intVec = IntVec3.FromVector3(clickPos);
		List<Thing> list = new List<Thing>();
		IReadOnlyList<Pawn> allPawnsSpawned = Find.CurrentMap.mapPawns.AllPawnsSpawned;
		for (int i = 0; i < allPawnsSpawned.Count; i++)
		{
			Pawn pawn = allPawnsSpawned[i];
			if ((pawn.DrawPos - clickPos).MagnitudeHorizontal() < 0.4f && clickParams.CanTarget(pawn, source))
			{
				list.Add(pawn);
				list.AddRange(ContainingSelectionUtility.SelectableContainedThings(pawn));
			}
		}
		list.Sort(CompareThingsByDistanceToMousePointer);
		cellThings.Clear();
		foreach (Thing item in Find.CurrentMap.thingGrid.ThingsAt(intVec))
		{
			if (!list.Contains(item) && clickParams.CanTarget(item, source))
			{
				cellThings.Add(item);
				cellThings.AddRange(ContainingSelectionUtility.SelectableContainedThings(item));
			}
		}
		IntVec3[] adjacentCells = GenAdj.AdjacentCells;
		for (int j = 0; j < adjacentCells.Length; j++)
		{
			IntVec3 c = adjacentCells[j] + intVec;
			if (!c.InBounds(Find.CurrentMap) || c.GetItemCount(Find.CurrentMap) <= 1)
			{
				continue;
			}
			foreach (Thing item2 in Find.CurrentMap.thingGrid.ThingsAt(c))
			{
				if (item2.def.category == ThingCategory.Item && (item2.TrueCenter() - UI.MouseMapPosition()).MagnitudeHorizontalSquared() <= 0.25f && !list.Contains(item2) && clickParams.CanTarget(item2, source))
				{
					cellThings.Add(item2);
				}
			}
		}
		List<Thing> list2 = Find.CurrentMap.listerThings.ThingsInGroup(ThingRequestGroup.WithCustomRectForSelector);
		for (int k = 0; k < list2.Count; k++)
		{
			Thing thing = list2[k];
			if (thing.CustomRectForSelector.HasValue && thing.CustomRectForSelector.Value.Contains(intVec) && !list.Contains(thing) && clickParams.CanTarget(thing, source))
			{
				cellThings.Add(thing);
			}
		}
		cellThings.Sort(CompareThingsByDrawAltitudeOrDistToItem);
		list.AddRange(cellThings);
		cellThings.Clear();
		IReadOnlyList<Pawn> allPawnsSpawned2 = Find.CurrentMap.mapPawns.AllPawnsSpawned;
		for (int l = 0; l < allPawnsSpawned2.Count; l++)
		{
			Pawn pawn2 = allPawnsSpawned2[l];
			if ((pawn2.DrawPos - clickPos).MagnitudeHorizontal() < pawnWideClickRadius && clickParams.CanTarget(pawn2, source))
			{
				cellThings.Add(pawn2);
			}
		}
		cellThings.Sort(CompareThingsByDistanceToMousePointer);
		for (int m = 0; m < cellThings.Count; m++)
		{
			if (!list.Contains(cellThings[m]))
			{
				list.Add(cellThings[m]);
				list.AddRange(ContainingSelectionUtility.SelectableContainedThings(cellThings[m]));
			}
		}
		list.RemoveAll((Thing thing2) => !clickParams.CanTarget(thing2, source));
		list.RemoveAll((Thing thing2) => thing2 is Pawn pawn3 && pawn3.IsHiddenFromPlayer());
		cellThings.Clear();
		return list;
	}

	private static int CompareThingsByDistanceToMousePointer(Thing a, Thing b)
	{
		Vector3 vector = UI.MouseMapPosition();
		float num = (a.DrawPosHeld.Value - vector).MagnitudeHorizontalSquared();
		float num2 = (b.DrawPosHeld.Value - vector).MagnitudeHorizontalSquared();
		if (num < num2)
		{
			return -1;
		}
		if (num == num2)
		{
			return b.Spawned.CompareTo(a.Spawned);
		}
		return 1;
	}

	private static int CompareThingsByDrawAltitudeOrDistToItem(Thing A, Thing B)
	{
		if (A.def.category == ThingCategory.Item && B.def.category == ThingCategory.Item)
		{
			return (A.TrueCenter() - UI.MouseMapPosition()).MagnitudeHorizontalSquared().CompareTo((B.TrueCenter() - UI.MouseMapPosition()).MagnitudeHorizontalSquared());
		}
		Thing spawnedParentOrMe = A.SpawnedParentOrMe;
		Thing spawnedParentOrMe2 = B.SpawnedParentOrMe;
		if (spawnedParentOrMe.def.Altitude != spawnedParentOrMe2.def.Altitude)
		{
			return spawnedParentOrMe2.def.Altitude.CompareTo(spawnedParentOrMe.def.Altitude);
		}
		return B.Spawned.CompareTo(A.Spawned);
	}

	public static int CurrentAdjustmentMultiplier()
	{
		if (KeyBindingDefOf.ModifierIncrement_10x.IsDownEvent && KeyBindingDefOf.ModifierIncrement_100x.IsDownEvent)
		{
			return 1000;
		}
		if (KeyBindingDefOf.ModifierIncrement_100x.IsDownEvent)
		{
			return 100;
		}
		if (KeyBindingDefOf.ModifierIncrement_10x.IsDownEvent)
		{
			return 10;
		}
		return 1;
	}

	public static Rect GetInnerRect(this Rect rect)
	{
		return rect.ContractedBy(17f);
	}

	public static Rect ExpandedBy(this Rect rect, float margin)
	{
		return new Rect(rect.x - margin, rect.y - margin, rect.width + margin * 2f, rect.height + margin * 2f);
	}

	public static Rect ExpandedBy(this Rect rect, float marginX, float marginY)
	{
		return new Rect(rect.x - marginX, rect.y - marginY, rect.width + marginX * 2f, rect.height + marginY * 2f);
	}

	public static Rect ContractedBy(this Rect rect, float margin)
	{
		return new Rect(rect.x + margin, rect.y + margin, rect.width - margin * 2f, rect.height - margin * 2f);
	}

	public static Rect ContractedBy(this Rect rect, float marginX, float marginY)
	{
		return new Rect(rect.x + marginX, rect.y + marginY, rect.width - marginX * 2f, rect.height - marginY * 2f);
	}

	public static Rect ScaledBy(this Rect rect, float scale)
	{
		rect.x -= rect.width * (scale - 1f) / 2f;
		rect.y -= rect.height * (scale - 1f) / 2f;
		rect.width *= scale;
		rect.height *= scale;
		return rect;
	}

	public static Rect CenteredOnXIn(this Rect rect, Rect otherRect)
	{
		return new Rect(otherRect.x + (otherRect.width - rect.width) / 2f, rect.y, rect.width, rect.height);
	}

	public static Rect CenteredOnYIn(this Rect rect, Rect otherRect)
	{
		return new Rect(rect.x, otherRect.y + (otherRect.height - rect.height) / 2f, rect.width, rect.height);
	}

	public static Rect AtZero(this Rect rect)
	{
		return new Rect(0f, 0f, rect.width, rect.height);
	}

	public static Rect Union(this Rect a, Rect b)
	{
		return new Rect
		{
			min = Vector2.Min(a.min, b.min),
			max = Vector2.Max(a.max, b.max)
		};
	}

	public static void AbsorbClicksInRect(Rect r)
	{
		if (Event.current.type == EventType.MouseDown && r.Contains(Event.current.mousePosition))
		{
			Event.current.Use();
		}
	}

	public static Rect LeftHalf(this Rect rect)
	{
		return new Rect(rect.x, rect.y, rect.width / 2f, rect.height);
	}

	public static Rect LeftPart(this Rect rect, float pct)
	{
		return new Rect(rect.x, rect.y, rect.width * pct, rect.height);
	}

	public static Rect LeftPartPixels(this Rect rect, float width)
	{
		return new Rect(rect.x, rect.y, width, rect.height);
	}

	public static Rect RightHalf(this Rect rect)
	{
		return new Rect(rect.x + rect.width / 2f, rect.y, rect.width / 2f, rect.height);
	}

	public static Rect RightPart(this Rect rect, float pct)
	{
		return new Rect(rect.x + rect.width * (1f - pct), rect.y, rect.width * pct, rect.height);
	}

	public static Rect RightPartPixels(this Rect rect, float width)
	{
		return new Rect(rect.x + rect.width - width, rect.y, width, rect.height);
	}

	public static Rect TopHalf(this Rect rect)
	{
		return new Rect(rect.x, rect.y, rect.width, rect.height / 2f);
	}

	public static Rect TopPart(this Rect rect, float pct)
	{
		return new Rect(rect.x, rect.y, rect.width, rect.height * pct);
	}

	public static Rect TopPartPixels(this Rect rect, float height)
	{
		return new Rect(rect.x, rect.y, rect.width, height);
	}

	public static Rect BottomHalf(this Rect rect)
	{
		return new Rect(rect.x, rect.y + rect.height / 2f, rect.width, rect.height / 2f);
	}

	public static Rect BottomPart(this Rect rect, float pct)
	{
		return new Rect(rect.x, rect.y + rect.height * (1f - pct), rect.width, rect.height * pct);
	}

	public static Rect BottomPartPixels(this Rect rect, float height)
	{
		return new Rect(rect.x, rect.y + rect.height - height, rect.width, height);
	}

	public static Rect MiddlePart(this Rect rect, float pctWidth, float pctHeight)
	{
		return new Rect(rect.x + rect.width / 2f - rect.width * pctWidth / 2f, rect.y + rect.height / 2f - rect.height * pctHeight / 2f, rect.width * pctWidth, rect.height * pctHeight);
	}

	public static Rect MiddlePartPixels(this Rect rect, float width, float height)
	{
		return new Rect(rect.x + rect.width / 2f - width / 2f, rect.y + rect.height / 2f - height / 2f, width, height);
	}

	public static IEnumerable<Vector2> Corners(this Rect rect)
	{
		yield return new Vector2(rect.x, rect.y);
		yield return new Vector2(rect.x, rect.yMax);
		yield return new Vector2(rect.xMax, rect.yMax);
		yield return new Vector2(rect.xMax, rect.y);
	}

	public static bool SplitHorizontallyWithMargin(this Rect rect, out Rect top, out Rect bottom, out float overflow, float compressibleMargin = 0f, float? topHeight = null, float? bottomHeight = null)
	{
		if (topHeight.HasValue == bottomHeight.HasValue)
		{
			throw new ArgumentException("Exactly one null height and one non-null height must be provided.");
		}
		overflow = Mathf.Max(0f, (topHeight ?? bottomHeight.GetValueOrDefault()) - rect.height);
		float height = Mathf.Clamp(topHeight ?? (rect.height - bottomHeight.Value - compressibleMargin), 0f, rect.height);
		float num = Mathf.Clamp(bottomHeight ?? (rect.height - topHeight.Value - compressibleMargin), 0f, rect.height);
		top = new Rect(rect.x, rect.y, rect.width, height);
		bottom = new Rect(rect.x, rect.yMax - num, rect.width, num);
		return overflow == 0f;
	}

	public static void SplitVerticallyWithMargin(this Rect rect, out Rect left, out Rect right, float margin)
	{
		float num = rect.width / 2f;
		left = new Rect(rect.x, rect.y, num - margin / 2f, rect.height);
		right = new Rect(left.xMax + margin, rect.y, num - margin / 2f, rect.height);
	}

	public static bool SplitVerticallyWithMargin(this Rect rect, out Rect left, out Rect right, out float overflow, float compressibleMargin = 0f, float? leftWidth = null, float? rightWidth = null)
	{
		if (leftWidth.HasValue == rightWidth.HasValue)
		{
			throw new ArgumentException("Exactly one null width and one non-null width must be provided.");
		}
		overflow = Mathf.Max(0f, (leftWidth ?? rightWidth.GetValueOrDefault()) - rect.width);
		float width = Mathf.Clamp(leftWidth ?? (rect.width - rightWidth.Value - compressibleMargin), 0f, rect.width);
		float num = Mathf.Clamp(rightWidth ?? (rect.width - leftWidth.Value - compressibleMargin), 0f, rect.width);
		left = new Rect(rect.x, rect.y, width, rect.height);
		right = new Rect(rect.xMax - num, rect.y, num, rect.height);
		return overflow == 0f;
	}

	public static void SplitHorizontally(this Rect rect, float topHeight, out Rect top, out Rect bottom)
	{
		rect.SplitHorizontallyWithMargin(out top, out bottom, out var _, 0f, topHeight);
	}

	public static void SplitVertically(this Rect rect, float leftWidth, out Rect left, out Rect right)
	{
		rect.SplitVerticallyWithMargin(out left, out right, out var _, 0f, leftWidth);
	}

	public static Color LerpColor(List<Pair<float, Color>> colors, float value)
	{
		if (colors.Count == 0)
		{
			return Color.white;
		}
		for (int i = 0; i < colors.Count; i++)
		{
			if (value < colors[i].First)
			{
				if (i == 0)
				{
					return colors[i].Second;
				}
				return Color.Lerp(colors[i - 1].Second, colors[i].Second, Mathf.InverseLerp(colors[i - 1].First, colors[i].First, value));
			}
		}
		return colors.Last().Second;
	}

	public static Vector2 GetMouseAttachedWindowPos(float width, float height)
	{
		Vector2 mousePosition = Event.current.mousePosition;
		float num = 0f;
		num = ((mousePosition.y + 14f + height < (float)UI.screenHeight) ? (mousePosition.y + 14f) : ((!(mousePosition.y - 5f - height >= 0f)) ? ((float)UI.screenHeight - (14f + height)) : (mousePosition.y - 5f - height)));
		float num2 = 0f;
		num2 = ((!(mousePosition.x + 16f + width < (float)UI.screenWidth)) ? (mousePosition.x - 4f - width) : (mousePosition.x + 16f));
		return new Vector2(num2, num);
	}

	public static float GetCenteredButtonPos(int buttonIndex, int buttonsCount, float totalWidth, float buttonWidth, float pad = 10f)
	{
		float num = (float)buttonsCount * buttonWidth + (float)(buttonsCount - 1) * pad;
		return Mathf.Floor((totalWidth - num) / 2f + (float)buttonIndex * (buttonWidth + pad));
	}

	public static void DrawArrowPointingAt(Rect rect)
	{
		Vector2 vector = new Vector2(UI.screenWidth, UI.screenHeight) / 2f;
		float angle = Mathf.Atan2(rect.center.x - vector.x, vector.y - rect.center.y) * 57.29578f;
		Vector2 vector2 = new Bounds(rect.center, rect.size).ClosestPoint(vector);
		Rect position = new Rect(vector2 + Vector2.left * ArrowTex.width * 0.5f, new Vector2(ArrowTex.width, ArrowTex.height));
		Matrix4x4 matrix = GUI.matrix;
		GUI.matrix = Matrix4x4.identity;
		Vector2 center = GUIUtility.GUIToScreenPoint(vector2);
		GUI.matrix = matrix;
		UI.RotateAroundPivot(angle, center);
		GUI.DrawTexture(position, ArrowTex);
		GUI.matrix = matrix;
		UnityGUIBugsFixer.Notify_GUIMatrixChanged();
	}

	public static void DrawArrowPointingAtWorldspace(Vector3 worldspace, Camera camera)
	{
		Vector3 vector = camera.WorldToScreenPoint(worldspace) / Prefs.UIScale;
		DrawArrowPointingAt(new Rect(new Vector2(vector.x, (float)UI.screenHeight - vector.y) + new Vector2(-2f, 2f), new Vector2(4f, 4f)));
	}

	public static Rect DrawElementStack<T>(Rect rect, float rowHeight, List<T> elements, StackElementDrawer<T> drawer, StackElementWidthGetter<T> widthGetter, float rowMargin = 4f, float elementMargin = 5f, bool allowOrderOptimization = true)
	{
		tmpRects.Clear();
		tmpRects2.Clear();
		for (int i = 0; i < elements.Count; i++)
		{
			tmpRects.Add(new StackedElementRect(new Rect(0f, 0f, widthGetter(elements[i]), rowHeight), i));
		}
		int num = Mathf.FloorToInt(rect.height / rowHeight);
		List<StackedElementRect> list = tmpRects;
		float num2;
		float num3;
		if (allowOrderOptimization)
		{
			num2 = (num3 = 0f);
			while (num3 < (float)num)
			{
				StackedElementRect item = default(StackedElementRect);
				int num4 = -1;
				for (int j = 0; j < list.Count; j++)
				{
					StackedElementRect stackedElementRect = list[j];
					if (num4 == -1 || (item.rect.width < stackedElementRect.rect.width && stackedElementRect.rect.width < rect.width - num2))
					{
						num4 = j;
						item = stackedElementRect;
					}
				}
				if (num4 == -1)
				{
					if (num2 == 0f)
					{
						break;
					}
					num2 = 0f;
					num3 += 1f;
				}
				else
				{
					num2 += item.rect.width + elementMargin;
					tmpRects2.Add(item);
				}
				list.RemoveAt(num4);
				if (list.Count <= 0)
				{
					break;
				}
			}
			list = tmpRects2;
		}
		num2 = (num3 = 0f);
		while (list.Count > 0)
		{
			StackedElementRect stackedElementRect2 = list[0];
			if (num2 + stackedElementRect2.rect.width > rect.width)
			{
				num2 = 0f;
				num3 += rowHeight + rowMargin;
			}
			drawer?.Invoke(new Rect(rect.x + num2, rect.y + num3, stackedElementRect2.rect.width, stackedElementRect2.rect.height), elements[stackedElementRect2.elementIndex]);
			num2 += stackedElementRect2.rect.width + elementMargin;
			list.RemoveAt(0);
		}
		return new Rect(rect.x, rect.y, rect.width, num3 + rowHeight);
	}

	public static Rect DrawElementStackVertical<T>(Rect rect, float rowHeight, List<T> elements, StackElementDrawer<T> drawer, StackElementWidthGetter<T> widthGetter, float elementMargin = 5f, bool highlightOdd = false)
	{
		tmpRects.Clear();
		for (int i = 0; i < elements.Count; i++)
		{
			tmpRects.Add(new StackedElementRect(new Rect(0f, 0f, widthGetter(elements[i]), rowHeight), i));
		}
		int elem = Mathf.FloorToInt(rect.height / rowHeight);
		spacingCache.Reset(elem);
		int num = 0;
		float num2 = 0f;
		float num3 = 0f;
		for (int j = 0; j < tmpRects.Count; j++)
		{
			StackedElementRect stackedElementRect = tmpRects[j];
			if (num3 + stackedElementRect.rect.height > rect.height)
			{
				num3 = 0f;
				num = 0;
			}
			Rect rect2 = new Rect(rect.x + spacingCache.GetSpaceFor(num), rect.y + num3, stackedElementRect.rect.width, stackedElementRect.rect.height);
			if (highlightOdd && j % 2 == 1)
			{
				Widgets.DrawLightHighlight(rect2);
			}
			drawer(rect2, elements[stackedElementRect.elementIndex]);
			num3 += stackedElementRect.rect.height + elementMargin;
			spacingCache.AddSpace(num, stackedElementRect.rect.width + elementMargin);
			num2 = Mathf.Max(num2, spacingCache.GetSpaceFor(num));
			num++;
		}
		return new Rect(rect.x, rect.y, num2, num3 + rowHeight);
	}
}
