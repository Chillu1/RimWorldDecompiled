using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Verse
{
	public class EditWindow_CurveEditor : EditWindow
	{
		private SimpleCurve curve;

		public List<float> debugInputValues;

		private int draggingPointIndex = -1;

		private int draggingButton = -1;

		private const float ViewDragPanSpeed = 0.002f;

		private const float ScrollZoomSpeed = 0.025f;

		private const float PointClickDistanceLimit = 7f;

		private bool DraggingView => draggingButton >= 0;

		public override Vector2 InitialSize => new Vector2(600f, 400f);

		public override bool IsDebug => true;

		public EditWindow_CurveEditor(SimpleCurve curve, string title)
		{
			this.curve = curve;
			optionalTitle = title;
		}

		public override void DoWindowContents(Rect inRect)
		{
			WidgetRow widgetRow = new WidgetRow(0f, 0f);
			if (widgetRow.ButtonIcon(TexButton.CenterOnPointsTex, "Center view around points."))
			{
				curve.View.SetViewRectAround(curve);
			}
			if (widgetRow.ButtonIcon(TexButton.CurveResetTex, "Reset to growth from 0 to 1."))
			{
				List<CurvePoint> points = new List<CurvePoint>
				{
					new CurvePoint(0f, 0f),
					new CurvePoint(1f, 1f)
				};
				curve.SetPoints(points);
				curve.View.SetViewRectAround(curve);
			}
			if (widgetRow.ButtonIcon(TexButton.QuickZoomHor1Tex, "Reset horizontal zoom to 0-1"))
			{
				curve.View.rect.xMin = 0f;
				curve.View.rect.xMax = 1f;
			}
			if (widgetRow.ButtonIcon(TexButton.QuickZoomHor100Tex, "Reset horizontal zoom to 0-100"))
			{
				curve.View.rect.xMin = 0f;
				curve.View.rect.xMax = 100f;
			}
			if (widgetRow.ButtonIcon(TexButton.QuickZoomHor20kTex, "Reset horizontal zoom to 0-20,000"))
			{
				curve.View.rect.xMin = 0f;
				curve.View.rect.xMax = 20000f;
			}
			if (widgetRow.ButtonIcon(TexButton.QuickZoomVer1Tex, "Reset vertical zoom to 0-1"))
			{
				curve.View.rect.yMin = 0f;
				curve.View.rect.yMax = 1f;
			}
			if (widgetRow.ButtonIcon(TexButton.QuickZoomVer100Tex, "Reset vertical zoom to 0-100"))
			{
				curve.View.rect.yMin = 0f;
				curve.View.rect.yMax = 100f;
			}
			if (widgetRow.ButtonIcon(TexButton.QuickZoomVer20kTex, "Reset vertical zoom to 0-20,000"))
			{
				curve.View.rect.yMin = 0f;
				curve.View.rect.yMax = 20000f;
			}
			Rect screenRect = new Rect(inRect.AtZero());
			screenRect.yMin += 26f;
			screenRect.yMax -= 24f;
			DoCurveEditor(screenRect);
		}

		private void DoCurveEditor(Rect screenRect)
		{
			Widgets.DrawMenuSection(screenRect);
			SimpleCurveDrawer.DrawCurve(screenRect, curve);
			Vector2 mousePosition = Event.current.mousePosition;
			if (Mouse.IsOver(screenRect))
			{
				Rect rect = new Rect(mousePosition.x + 8f, mousePosition.y + 18f, 100f, 100f);
				Vector2 v = SimpleCurveDrawer.ScreenToCurveCoords(screenRect, curve.View.rect, mousePosition);
				Widgets.Label(rect, v.ToStringTwoDigits());
			}
			Rect rect2 = new Rect(0f, 0f, 50f, 24f);
			rect2.x = screenRect.x;
			rect2.y = screenRect.y + screenRect.height / 2f - 12f;
			if (float.TryParse(Widgets.TextField(rect2, curve.View.rect.x.ToString()), out float result))
			{
				curve.View.rect.x = result;
			}
			rect2.x = screenRect.xMax - rect2.width;
			rect2.y = screenRect.y + screenRect.height / 2f - 12f;
			if (float.TryParse(Widgets.TextField(rect2, curve.View.rect.xMax.ToString()), out result))
			{
				curve.View.rect.xMax = result;
			}
			rect2.x = screenRect.x + screenRect.width / 2f - rect2.width / 2f;
			rect2.y = screenRect.yMax - rect2.height;
			if (float.TryParse(Widgets.TextField(rect2, curve.View.rect.y.ToString()), out result))
			{
				curve.View.rect.y = result;
			}
			rect2.x = screenRect.x + screenRect.width / 2f - rect2.width / 2f;
			rect2.y = screenRect.y;
			if (float.TryParse(Widgets.TextField(rect2, curve.View.rect.yMax.ToString()), out result))
			{
				curve.View.rect.yMax = result;
			}
			if (Mouse.IsOver(screenRect))
			{
				if (Event.current.type == EventType.ScrollWheel)
				{
					float num = -1f * Event.current.delta.y * 0.025f;
					float num2 = curve.View.rect.center.x - curve.View.rect.x;
					float num3 = curve.View.rect.center.y - curve.View.rect.y;
					curve.View.rect.xMin += num2 * num;
					curve.View.rect.xMax -= num2 * num;
					curve.View.rect.yMin += num3 * num;
					curve.View.rect.yMax -= num3 * num;
					Event.current.Use();
				}
				if (Event.current.type == EventType.MouseDown && (Event.current.button == 0 || Event.current.button == 2))
				{
					List<int> list = PointsNearMouse(screenRect).ToList();
					if (list.Any())
					{
						draggingPointIndex = list[0];
					}
					else
					{
						draggingPointIndex = -1;
					}
					if (draggingPointIndex < 0)
					{
						draggingButton = Event.current.button;
					}
					Event.current.Use();
				}
				if (Event.current.type == EventType.MouseDown && Event.current.button == 1)
				{
					Vector2 mouseCurveCoords = SimpleCurveDrawer.ScreenToCurveCoords(screenRect, curve.View.rect, Event.current.mousePosition);
					List<FloatMenuOption> list2 = new List<FloatMenuOption>();
					list2.Add(new FloatMenuOption("Add point at " + mouseCurveCoords.ToString(), delegate
					{
						curve.Add(new CurvePoint(mouseCurveCoords));
					}));
					foreach (int item in PointsNearMouse(screenRect))
					{
						CurvePoint point = curve[item];
						list2.Add(new FloatMenuOption("Remove point at " + point.ToString(), delegate
						{
							curve.RemovePointNear(point);
						}));
					}
					Find.WindowStack.Add(new FloatMenu(list2));
					Event.current.Use();
				}
			}
			if (draggingPointIndex >= 0)
			{
				curve[draggingPointIndex] = new CurvePoint(SimpleCurveDrawer.ScreenToCurveCoords(screenRect, curve.View.rect, Event.current.mousePosition));
				curve.SortPoints();
				if (Event.current.type == EventType.MouseUp && Event.current.button == 0)
				{
					draggingPointIndex = -1;
					Event.current.Use();
				}
			}
			if (DraggingView)
			{
				if (Event.current.type == EventType.MouseDrag)
				{
					Vector2 delta = Event.current.delta;
					curve.View.rect.x -= delta.x * curve.View.rect.width * 0.002f;
					curve.View.rect.y += delta.y * curve.View.rect.height * 0.002f;
					Event.current.Use();
				}
				if (Event.current.type == EventType.MouseUp && Event.current.button == draggingButton)
				{
					draggingButton = -1;
				}
			}
		}

		private IEnumerable<int> PointsNearMouse(Rect screenRect)
		{
			GUI.BeginGroup(screenRect);
			try
			{
				for (int i = 0; i < curve.PointsCount; i++)
				{
					if ((SimpleCurveDrawer.CurveToScreenCoordsInsideScreenRect(screenRect, curve.View.rect, curve[i].Loc) - Event.current.mousePosition).sqrMagnitude < 49f)
					{
						yield return i;
					}
				}
			}
			finally
			{
				GUI.EndGroup();
			}
		}
	}
}
