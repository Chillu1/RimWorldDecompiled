using System.Collections.Generic;
using UnityEngine;

namespace Verse;

[StaticConstructorOnStartup]
public static class SimpleCurveDrawer
{
	private const float PointSize = 10f;

	private static readonly Color AxisLineColor = new Color(0.2f, 0.5f, 1f, 1f);

	private static readonly Color MajorLineColor = new Color(0.2f, 0.4f, 1f, 0.6f);

	private static readonly Color MinorLineColor = new Color(0.2f, 0.3f, 1f, 0.19f);

	private const float MeasureWidth = 60f;

	private const float MeasureHeight = 30f;

	private const float MeasureLinePeekOut = 5f;

	private const float LegendCellWidth = 140f;

	private const float LegendCellHeight = 20f;

	private static readonly Texture2D CurvePoint = ContentFinder<Texture2D>.Get("UI/Widgets/Dev/CurvePoint");

	public static void DrawCurve(Rect rect, SimpleCurve curve, SimpleCurveDrawerStyle style = null, List<CurveMark> marks = null, Rect legendScreenRect = default(Rect))
	{
		SimpleCurveDrawInfo simpleCurveDrawInfo = new SimpleCurveDrawInfo();
		simpleCurveDrawInfo.curve = curve;
		DrawCurve(rect, simpleCurveDrawInfo, style, marks, legendScreenRect);
	}

	public static void DrawCurve(Rect rect, SimpleCurveDrawInfo curve, SimpleCurveDrawerStyle style = null, List<CurveMark> marks = null, Rect legendScreenRect = default(Rect))
	{
		if (curve.curve != null)
		{
			List<SimpleCurveDrawInfo> list = new List<SimpleCurveDrawInfo>();
			list.Add(curve);
			DrawCurves(rect, list, style, marks, legendScreenRect);
		}
	}

	public static void DrawCurves(Rect rect, List<SimpleCurveDrawInfo> curves, SimpleCurveDrawerStyle style = null, List<CurveMark> marks = null, Rect legendRect = default(Rect))
	{
		if (Event.current.type != EventType.Repaint)
		{
			return;
		}
		if (style == null)
		{
			style = new SimpleCurveDrawerStyle();
		}
		if (curves.Count == 0)
		{
			return;
		}
		bool flag = true;
		Rect viewRect = default(Rect);
		for (int i = 0; i < curves.Count; i++)
		{
			SimpleCurveDrawInfo simpleCurveDrawInfo = curves[i];
			if (simpleCurveDrawInfo.curve != null)
			{
				if (flag)
				{
					flag = false;
					viewRect = simpleCurveDrawInfo.curve.View.rect;
					continue;
				}
				viewRect.xMin = Mathf.Min(viewRect.xMin, simpleCurveDrawInfo.curve.View.rect.xMin);
				viewRect.xMax = Mathf.Max(viewRect.xMax, simpleCurveDrawInfo.curve.View.rect.xMax);
				viewRect.yMin = Mathf.Min(viewRect.yMin, simpleCurveDrawInfo.curve.View.rect.yMin);
				viewRect.yMax = Mathf.Max(viewRect.yMax, simpleCurveDrawInfo.curve.View.rect.yMax);
			}
		}
		if (style.UseFixedScale)
		{
			viewRect.yMin = style.FixedScale.x;
			viewRect.yMax = style.FixedScale.y;
		}
		if (style.OnlyPositiveValues)
		{
			if (viewRect.xMin < 0f)
			{
				viewRect.xMin = 0f;
			}
			if (viewRect.yMin < 0f)
			{
				viewRect.yMin = 0f;
			}
		}
		if (style.UseFixedSection)
		{
			viewRect.xMin = style.FixedSection.min;
			viewRect.xMax = style.FixedSection.max;
		}
		if (Mathf.Approximately(viewRect.width, 0f) || Mathf.Approximately(viewRect.height, 0f))
		{
			return;
		}
		Rect rect2 = rect;
		if (style.DrawMeasures)
		{
			rect2.xMin += 60f;
			rect2.yMax -= 30f;
		}
		if (marks != null)
		{
			Rect rect3 = rect2;
			rect3.height = 15f;
			DrawCurveMarks(rect3, viewRect, marks);
			rect2.yMin = rect3.yMax;
		}
		if (style.DrawBackground)
		{
			GUI.color = new Color(0.302f, 0.318f, 0.365f);
			GUI.DrawTexture(rect2, BaseContent.WhiteTex);
		}
		if (style.DrawBackgroundLines)
		{
			DrawGraphBackgroundLines(rect2, viewRect);
		}
		if (style.DrawMeasures)
		{
			DrawCurveMeasures(rect, viewRect, rect2, style.MeasureLabelsXCount, style.MeasureLabelsYCount, style.XIntegersOnly, style.YIntegersOnly);
		}
		foreach (SimpleCurveDrawInfo curf in curves)
		{
			DrawCurveLines(rect2, curf, style.DrawPoints, viewRect, style.UseAntiAliasedLines, style.PointsRemoveOptimization);
		}
		if (style.DrawLegend)
		{
			DrawCurvesLegend(legendRect, curves);
		}
		if (style.DrawCurveMousePoint)
		{
			DrawCurveMousePoint(curves, rect2, viewRect, style.LabelX);
		}
	}

	public static void DrawCurveLines(Rect rect, SimpleCurveDrawInfo curve, bool drawPoints, Rect viewRect, bool useAALines, bool pointsRemoveOptimization)
	{
		if (curve.curve == null || curve.curve.PointsCount == 0)
		{
			return;
		}
		Rect rect2 = rect;
		rect2.yMin -= 1f;
		rect2.yMax += 1f;
		Widgets.BeginGroup(rect2);
		if (Event.current.type == EventType.Repaint)
		{
			if (useAALines)
			{
				bool flag = true;
				Vector2 start = default(Vector2);
				Vector2 curvePoint = default(Vector2);
				int num = curve.curve.Points.Count((CurvePoint x) => x.x >= viewRect.xMin && x.x <= viewRect.xMax);
				int num2 = RemovePointsOptimizationFreq(num);
				for (int num3 = 0; num3 < curve.curve.PointsCount; num3++)
				{
					CurvePoint curvePoint2 = curve.curve[num3];
					if (!pointsRemoveOptimization || num3 % num2 != 0 || num3 == 0 || num3 == num - 1)
					{
						curvePoint.x = curvePoint2.x;
						curvePoint.y = curvePoint2.y;
						Vector2 vector = CurveToScreenCoordsInsideScreenRect(rect, viewRect, curvePoint);
						if (flag)
						{
							flag = false;
						}
						else if ((start.x >= 0f && start.x <= rect.width) || (vector.x >= 0f && vector.x <= rect.width))
						{
							Widgets.DrawLine(start, vector, curve.color, 1f);
						}
						start = vector;
					}
				}
				Vector2 start2 = CurveToScreenCoordsInsideScreenRect(rect, viewRect, curve.curve[0]);
				Vector2 start3 = CurveToScreenCoordsInsideScreenRect(rect, viewRect, curve.curve[curve.curve.PointsCount - 1]);
				Widgets.DrawLine(start2, new Vector2(0f, start2.y), curve.color, 1f);
				Widgets.DrawLine(start3, new Vector2(rect.width, start3.y), curve.color, 1f);
			}
			else
			{
				GUI.color = curve.color;
				float num4 = viewRect.x;
				float num5 = rect.width / 1f;
				float num6 = viewRect.width / num5;
				while (num4 < viewRect.xMax)
				{
					num4 += num6;
					Vector2 vector2 = CurveToScreenCoordsInsideScreenRect(rect, viewRect, new Vector2(num4, curve.curve.Evaluate(num4)));
					GUI.DrawTexture(new Rect(vector2.x, vector2.y, 1f, 1f), BaseContent.WhiteTex);
				}
			}
			GUI.color = Color.white;
		}
		if (drawPoints)
		{
			for (int num7 = 0; num7 < curve.curve.PointsCount; num7++)
			{
				CurvePoint curvePoint3 = curve.curve[num7];
				DrawPoint(CurveToScreenCoordsInsideScreenRect(rect, viewRect, curvePoint3.Loc));
			}
		}
		foreach (float debugInputValue in curve.curve.View.DebugInputValues)
		{
			GUI.color = new Color(0f, 1f, 0f, 0.25f);
			DrawInfiniteVerticalLine(rect, viewRect, debugInputValue);
			float y = curve.curve.Evaluate(debugInputValue);
			Vector2 screenPoint = CurveToScreenCoordsInsideScreenRect(curvePoint: new Vector2(debugInputValue, y), rect: rect, viewRect: viewRect);
			GUI.color = Color.green;
			DrawPoint(screenPoint);
			GUI.color = Color.white;
		}
		Widgets.EndGroup();
	}

	public static void DrawCurveMeasures(Rect rect, Rect viewRect, Rect graphRect, int xLabelsCount, int yLabelsCount, bool xIntegersOnly, bool yIntegersOnly)
	{
		Text.Font = GameFont.Small;
		Color color = new Color(0.45f, 0.45f, 0.45f);
		Color color2 = new Color(0.7f, 0.7f, 0.7f);
		Widgets.BeginGroup(rect);
		CalculateMeasureStartAndInc(out var start, out var inc, out var count, viewRect.xMin, viewRect.xMax, xLabelsCount, xIntegersOnly);
		Text.Anchor = TextAnchor.UpperCenter;
		string text = string.Empty;
		for (int i = 0; i < count; i++)
		{
			float x = start + inc * (float)i;
			string text2 = x.ToString("F0");
			if (!(text2 == text))
			{
				text = text2;
				float num = CurveToScreenCoordsInsideScreenRect(graphRect, viewRect, new Vector2(x, 0f)).x + 60f;
				float num2 = rect.height - 30f;
				GUI.color = color;
				Widgets.DrawLineVertical(num, num2, 5f);
				GUI.color = color2;
				Rect rect2 = new Rect(num - 31f, num2 + 2f, 60f, 30f);
				Text.Font = GameFont.Tiny;
				Widgets.Label(rect2, text2);
				Text.Font = GameFont.Small;
			}
		}
		CalculateMeasureStartAndInc(out var start2, out var inc2, out var count2, viewRect.yMin, viewRect.yMax, yLabelsCount, yIntegersOnly);
		string text3 = string.Empty;
		Text.Anchor = TextAnchor.UpperRight;
		for (int j = 0; j < count2; j++)
		{
			float y = start2 + inc2 * (float)j;
			string text4 = y.ToString("F0");
			if (!(text4 == text3))
			{
				text3 = text4;
				float num3 = CurveToScreenCoordsInsideScreenRect(graphRect, viewRect, new Vector2(0f, y)).y + (graphRect.y - rect.y);
				GUI.color = color;
				Widgets.DrawLineHorizontal(55f, num3, 5f + graphRect.width);
				GUI.color = color2;
				Rect rect3 = new Rect(0f, num3 - 10f, 55f, 20f);
				Text.Font = GameFont.Tiny;
				Widgets.Label(rect3, text4);
				Text.Font = GameFont.Small;
			}
		}
		Widgets.EndGroup();
		GUI.color = new Color(1f, 1f, 1f);
		Text.Anchor = TextAnchor.UpperLeft;
	}

	private static void CalculateMeasureStartAndInc(out float start, out float inc, out int count, float min, float max, int wantedCount, bool integersOnly)
	{
		if (integersOnly && GenMath.AnyIntegerInRange(min, max))
		{
			int num = Mathf.CeilToInt(min);
			int num2 = Mathf.FloorToInt(max);
			start = num;
			inc = Mathf.CeilToInt((float)(num2 - num + 1) / (float)wantedCount);
			count = (num2 - num) / (int)inc + 1;
		}
		else
		{
			start = min;
			inc = (max - min) / (float)wantedCount;
			count = wantedCount;
		}
	}

	public static void DrawCurvesLegend(Rect rect, List<SimpleCurveDrawInfo> curves)
	{
		Text.Anchor = TextAnchor.UpperLeft;
		Text.Font = GameFont.Small;
		Text.WordWrap = false;
		Widgets.BeginGroup(rect);
		float num = 0f;
		float num2 = 0f;
		int num3 = (int)(rect.width / 140f);
		int num4 = 0;
		foreach (SimpleCurveDrawInfo curf in curves)
		{
			GUI.color = curf.color;
			GUI.DrawTexture(new Rect(num, num2 + 2f, 15f, 15f), BaseContent.WhiteTex);
			GUI.color = Color.white;
			num += 20f;
			if (curf.label != null)
			{
				Widgets.Label(new Rect(num, num2, 140f, 100f), curf.label);
			}
			num4++;
			if (num4 == num3)
			{
				num4 = 0;
				num = 0f;
				num2 += 20f;
			}
			else
			{
				num += 140f;
			}
		}
		Widgets.EndGroup();
		GUI.color = Color.white;
		Text.WordWrap = true;
	}

	public static void DrawCurveMousePoint(List<SimpleCurveDrawInfo> curves, Rect screenRect, Rect viewRect, string labelX)
	{
		if (curves.Count == 0 || !Mouse.IsOver(screenRect))
		{
			return;
		}
		Widgets.BeginGroup(screenRect);
		Vector2 mousePosition = Event.current.mousePosition;
		Vector2 vector = default(Vector2);
		Vector2 vector2 = default(Vector2);
		SimpleCurveDrawInfo simpleCurveDrawInfo = null;
		bool flag = false;
		foreach (SimpleCurveDrawInfo curf in curves)
		{
			if (curf.curve.PointsCount != 0)
			{
				Vector2 vector3 = ScreenToCurveCoords(screenRect, viewRect, mousePosition);
				vector3.y = curf.curve.Evaluate(vector3.x);
				Vector2 vector4 = CurveToScreenCoordsInsideScreenRect(screenRect, viewRect, vector3);
				if (!flag || Vector2.Distance(vector4, mousePosition) < Vector2.Distance(vector2, mousePosition))
				{
					flag = true;
					vector = vector3;
					vector2 = vector4;
					simpleCurveDrawInfo = curf;
				}
			}
		}
		if (flag)
		{
			DrawPoint(vector2);
			Rect rect = new Rect(vector2.x, vector2.y, 120f, 60f);
			Text.Anchor = TextAnchor.UpperLeft;
			if (rect.x + rect.width > screenRect.width)
			{
				rect.x -= rect.width;
				Text.Anchor = TextAnchor.UpperRight;
			}
			if (rect.y + rect.height > screenRect.height)
			{
				rect.y -= rect.height;
				if (Text.Anchor == TextAnchor.UpperLeft)
				{
					Text.Anchor = TextAnchor.LowerLeft;
				}
				else
				{
					Text.Anchor = TextAnchor.LowerRight;
				}
			}
			string text = ((!simpleCurveDrawInfo.valueFormat.NullOrEmpty()) ? string.Format(simpleCurveDrawInfo.valueFormat, vector.y.ToString("0.##")) : vector.y.ToString("0.##"));
			Widgets.Label(rect, simpleCurveDrawInfo.label + "\n" + labelX + " " + vector.x.ToString("0.##") + "\n" + text);
			Text.Anchor = TextAnchor.UpperLeft;
		}
		Widgets.EndGroup();
	}

	public static void DrawCurveMarks(Rect rect, Rect viewRect, List<CurveMark> marks)
	{
		float x = viewRect.x;
		float num = viewRect.x + viewRect.width;
		float y = rect.y + 5f;
		_ = rect.yMax;
		for (int i = 0; i < marks.Count; i++)
		{
			CurveMark curveMark = marks[i];
			if (curveMark.X >= x && curveMark.X <= num)
			{
				GUI.color = curveMark.Color;
				Vector2 screenPoint = new Vector2(rect.x + (curveMark.X - x) / (num - x) * rect.width, y);
				DrawPoint(screenPoint);
				Rect rect2 = new Rect(screenPoint.x - 5f, screenPoint.y - 5f, 10f, 10f);
				if (Mouse.IsOver(rect2))
				{
					TooltipHandler.TipRegion(rect2, new TipSignal(curveMark.Message));
				}
			}
		}
		GUI.color = Color.white;
	}

	private static void DrawPoint(Vector2 screenPoint)
	{
		GUI.DrawTexture(new Rect(screenPoint.x - 5f, screenPoint.y - 5f, 10f, 10f), CurvePoint);
	}

	private static void DrawInfiniteVerticalLine(Rect rect, Rect viewRect, float curveX)
	{
		Widgets.DrawLineVertical(CurveToScreenCoordsInsideScreenRect(rect, viewRect, new Vector2(curveX, 0f)).x, -999f, 9999f);
	}

	private static void DrawInfiniteHorizontalLine(Rect rect, Rect viewRect, float curveY)
	{
		Widgets.DrawLineHorizontal(-999f, CurveToScreenCoordsInsideScreenRect(rect, viewRect, new Vector2(0f, curveY)).y, 9999f);
	}

	public static Vector2 CurveToScreenCoordsInsideScreenRect(Rect rect, Rect viewRect, Vector2 curvePoint)
	{
		Vector2 result = curvePoint;
		result.x -= viewRect.x;
		result.y -= viewRect.y;
		result.x *= rect.width / viewRect.width;
		result.y *= rect.height / viewRect.height;
		result.y = rect.height - result.y;
		return result;
	}

	public static Vector2 ScreenToCurveCoords(Rect rect, Rect viewRect, Vector2 screenPoint)
	{
		Vector2 loc = screenPoint;
		loc.y = rect.height - loc.y;
		loc.x /= rect.width / viewRect.width;
		loc.y /= rect.height / viewRect.height;
		loc.x += viewRect.x;
		loc.y += viewRect.y;
		return new CurvePoint(loc);
	}

	public static void DrawGraphBackgroundLines(Rect rect, Rect viewRect)
	{
		Widgets.BeginGroup(rect);
		float num = 0.01f;
		while (viewRect.width / (num * 10f) > 4f)
		{
			num *= 10f;
		}
		for (float num2 = (float)Mathf.FloorToInt(viewRect.x / num) * num; num2 < viewRect.xMax; num2 += num)
		{
			if (Mathf.Abs(num2 % (10f * num)) < 0.001f)
			{
				GUI.color = MajorLineColor;
			}
			else
			{
				GUI.color = MinorLineColor;
			}
			DrawInfiniteVerticalLine(rect, viewRect, num2);
		}
		float num3 = 0.01f;
		while (viewRect.height / (num3 * 10f) > 4f)
		{
			num3 *= 10f;
		}
		for (float num4 = (float)Mathf.FloorToInt(viewRect.y / num3) * num3; num4 < viewRect.yMax; num4 += num3)
		{
			if (Mathf.Abs(num4 % (10f * num3)) < 0.001f)
			{
				GUI.color = MajorLineColor;
			}
			else
			{
				GUI.color = MinorLineColor;
			}
			DrawInfiniteHorizontalLine(rect, viewRect, num4);
		}
		GUI.color = AxisLineColor;
		DrawInfiniteHorizontalLine(rect, viewRect, 0f);
		DrawInfiniteVerticalLine(rect, viewRect, 0f);
		GUI.color = Color.white;
		Widgets.EndGroup();
	}

	private static int RemovePointsOptimizationFreq(int count)
	{
		int result = count + 1;
		if (count > 1000)
		{
			result = 5;
		}
		if (count > 1200)
		{
			result = 4;
		}
		if (count > 1400)
		{
			result = 3;
		}
		if (count > 1900)
		{
			result = 2;
		}
		return result;
	}
}
