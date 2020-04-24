using UnityEngine;

namespace Verse
{
	public class WindowResizer
	{
		public Vector2 minWindowSize = new Vector2(150f, 150f);

		private bool isResizing;

		private Rect resizeStart;

		private const float ResizeButtonSize = 24f;

		public Rect DoResizeControl(Rect winRect)
		{
			Vector2 mousePosition = Event.current.mousePosition;
			Rect rect = new Rect(winRect.width - 24f, winRect.height - 24f, 24f, 24f);
			if (Event.current.type == EventType.MouseDown && Mouse.IsOver(rect))
			{
				isResizing = true;
				resizeStart = new Rect(mousePosition.x, mousePosition.y, winRect.width, winRect.height);
			}
			if (isResizing)
			{
				winRect.width = resizeStart.width + (mousePosition.x - resizeStart.x);
				winRect.height = resizeStart.height + (mousePosition.y - resizeStart.y);
				if (winRect.width < minWindowSize.x)
				{
					winRect.width = minWindowSize.x;
				}
				if (winRect.height < minWindowSize.y)
				{
					winRect.height = minWindowSize.y;
				}
				winRect.xMax = Mathf.Min(UI.screenWidth, winRect.xMax);
				winRect.yMax = Mathf.Min(UI.screenHeight, winRect.yMax);
				if (Event.current.type == EventType.MouseUp)
				{
					isResizing = false;
				}
			}
			Widgets.ButtonImage(rect, TexUI.WinExpandWidget);
			return new Rect(winRect.x, winRect.y, (int)winRect.width, (int)winRect.height);
		}
	}
}
