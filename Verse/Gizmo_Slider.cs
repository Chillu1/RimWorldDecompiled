using System.Collections.Generic;
using UnityEngine;

namespace Verse;

[StaticConstructorOnStartup]
public abstract class Gizmo_Slider : Gizmo
{
	private Texture2D barTex;

	private Texture2D barHighlightTex;

	private Texture2D barDragTex;

	private float targetValuePct;

	private bool initialized;

	protected Rect barRect;

	private const float Spacing = 8f;

	private static readonly Texture2D BarTex = SolidColorMaterials.NewSolidColorTexture(new Color(0.34f, 0.42f, 0.43f));

	private static readonly Texture2D BarHighlightTex = SolidColorMaterials.NewSolidColorTexture(new Color(0.43f, 0.54f, 0.55f));

	private static readonly Texture2D EmptyBarTex = SolidColorMaterials.NewSolidColorTexture(new Color(0.03f, 0.035f, 0.05f));

	private static readonly Texture2D DragBarTex = SolidColorMaterials.NewSolidColorTexture(new Color(0.74f, 0.97f, 0.8f));

	protected virtual float Width => 160f;

	protected abstract float Target { get; set; }

	protected abstract float ValuePercent { get; }

	protected virtual Color BarColor { get; }

	protected virtual Color BarHighlightColor { get; }

	protected virtual Color BarDragColor { get; }

	protected virtual FloatRange DragRange { get; } = FloatRange.ZeroToOne;

	protected virtual bool IsDraggable { get; }

	protected virtual string BarLabel => ValuePercent.ToStringPercent("0");

	protected abstract string Title { get; }

	protected virtual int Increments { get; } = 20;

	protected virtual string HighlightTag => null;

	protected abstract bool DraggingBar { get; set; }

	public sealed override float Order
	{
		get
		{
			return -100f;
		}
		set
		{
			base.Order = value;
		}
	}

	public sealed override float GetWidth(float maxWidth)
	{
		return Width;
	}

	protected virtual IEnumerable<float> GetBarThresholds()
	{
		yield break;
	}

	protected abstract string GetTooltip();

	private void Initialize()
	{
		if (!initialized)
		{
			initialized = true;
			targetValuePct = Mathf.Clamp(Target, DragRange.min, DragRange.max);
			barTex = ((BarColor == default(Color)) ? BarTex : SolidColorMaterials.NewSolidColorTexture(BarColor));
			barHighlightTex = ((BarHighlightColor == default(Color)) ? BarHighlightTex : SolidColorMaterials.NewSolidColorTexture(BarHighlightColor));
			barDragTex = ((BarDragColor == default(Color)) ? DragBarTex : SolidColorMaterials.NewSolidColorTexture(BarDragColor));
		}
	}

	public override GizmoResult GizmoOnGUI(Vector2 topLeft, float maxWidth, GizmoRenderParms parms)
	{
		if (!initialized)
		{
			Initialize();
		}
		Rect rect = new Rect(topLeft.x, topLeft.y, GetWidth(maxWidth), 75f);
		Rect rect2 = rect.ContractedBy(8f);
		Widgets.DrawWindowBackground(rect);
		bool mouseOverElement = false;
		Text.Font = GameFont.Small;
		Rect headerRect = rect2;
		headerRect.height = Text.LineHeight;
		DrawHeader(headerRect, ref mouseOverElement);
		barRect = rect2;
		barRect.yMin = headerRect.yMax + 8f;
		if (!IsDraggable)
		{
			Widgets.FillableBar(barRect, ValuePercent, barTex, EmptyBarTex, doBorder: true);
			foreach (float barThreshold in GetBarThresholds())
			{
				GUI.DrawTexture(new Rect
				{
					x = barRect.x + 3f + (barRect.width - 8f) * barThreshold,
					y = barRect.y + barRect.height - 9f,
					width = 2f,
					height = 6f
				}, (ValuePercent < barThreshold) ? BaseContent.GreyTex : BaseContent.BlackTex);
			}
		}
		else
		{
			bool draggingBar = DraggingBar;
			Widgets.DraggableBar(barRect, barTex, barHighlightTex, EmptyBarTex, barDragTex, ref draggingBar, ValuePercent, ref targetValuePct, GetBarThresholds(), Increments, DragRange.min, DragRange.max);
			DraggingBar = draggingBar;
			targetValuePct = Mathf.Clamp(targetValuePct, DragRange.min, DragRange.max);
			Target = targetValuePct;
		}
		Text.Anchor = TextAnchor.MiddleCenter;
		Widgets.Label(barRect, BarLabel);
		Text.Anchor = TextAnchor.UpperLeft;
		if (Mouse.IsOver(rect) && !mouseOverElement)
		{
			Widgets.DrawHighlight(rect);
			TooltipHandler.TipRegion(rect, GetTooltip, Gen.HashCombineInt(GetHashCode(), 8573612));
		}
		if (!HighlightTag.NullOrEmpty())
		{
			UIHighlighter.HighlightOpportunity(rect, HighlightTag);
		}
		return new GizmoResult(GizmoState.Clear);
	}

	protected virtual void DrawHeader(Rect headerRect, ref bool mouseOverElement)
	{
		string title = Title;
		title = title.Truncate(headerRect.width);
		Widgets.Label(headerRect, title);
	}
}
