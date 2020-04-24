using System;
using UnityEngine;

namespace Verse
{
	[StaticConstructorOnStartup]
	public class TabRecord
	{
		public string label = "Tab";

		public Action clickedAction;

		public bool selected;

		public Func<bool> selectedGetter;

		private const float TabEndWidth = 30f;

		private const float TabMiddleGraphicWidth = 4f;

		private static readonly Texture2D TabAtlas = ContentFinder<Texture2D>.Get("UI/Widgets/TabAtlas");

		public bool Selected
		{
			get
			{
				if (selectedGetter == null)
				{
					return selected;
				}
				return selectedGetter();
			}
		}

		public TabRecord(string label, Action clickedAction, bool selected)
		{
			this.label = label;
			this.clickedAction = clickedAction;
			this.selected = selected;
		}

		public TabRecord(string label, Action clickedAction, Func<bool> selected)
		{
			this.label = label;
			this.clickedAction = clickedAction;
			selectedGetter = selected;
		}

		public void Draw(Rect rect)
		{
			Rect drawRect = new Rect(rect);
			drawRect.width = 30f;
			drawRect.xMax = Widgets.AdjustCoordToUIScalingCeil(drawRect.xMax);
			Rect drawRect2 = new Rect(rect);
			drawRect2.width = 30f;
			drawRect2.x = rect.x + rect.width - 30f;
			Rect uvRect = new Rect(17f / 32f, 0f, 15f / 32f, 1f);
			Rect drawRect3 = new Rect(rect);
			drawRect3.x += drawRect.width;
			drawRect3.width -= 60f;
			drawRect3.xMin = Widgets.AdjustCoordToUIScalingFloor(drawRect3.xMin);
			drawRect3.xMax = Widgets.AdjustCoordToUIScalingCeil(drawRect3.xMax);
			Rect uvRect2 = new Rect(30f, 0f, 4f, TabAtlas.height).ToUVRect(new Vector2(TabAtlas.width, TabAtlas.height));
			Widgets.DrawTexturePart(drawRect, new Rect(0f, 0f, 15f / 32f, 1f), TabAtlas);
			Widgets.DrawTexturePart(drawRect3, uvRect2, TabAtlas);
			Widgets.DrawTexturePart(drawRect2, uvRect, TabAtlas);
			Rect rect2 = rect;
			rect2.width -= 10f;
			if (Mouse.IsOver(rect2))
			{
				GUI.color = Color.yellow;
				rect2.x += 2f;
				rect2.y -= 2f;
			}
			Text.WordWrap = false;
			Widgets.Label(rect2, label);
			Text.WordWrap = true;
			GUI.color = Color.white;
			if (!Selected)
			{
				Rect drawRect4 = new Rect(rect);
				drawRect4.y += rect.height;
				drawRect4.y -= 1f;
				drawRect4.height = 1f;
				Widgets.DrawTexturePart(uvRect: new Rect(0.5f, 0.01f, 0.01f, 0.01f), drawRect: drawRect4, tex: TabAtlas);
			}
		}
	}
}
