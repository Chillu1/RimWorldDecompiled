using RimWorld;
using UnityEngine;

namespace Verse
{
	public class ScreenshotModeHandler
	{
		private bool active;

		public bool Active => active;

		public bool FiltersCurrentEvent
		{
			get
			{
				if (!active)
				{
					return false;
				}
				if (Event.current.type == EventType.Repaint || Event.current.type == EventType.Layout)
				{
					return true;
				}
				if (Event.current.type == EventType.MouseDown || Event.current.type == EventType.MouseUp || Event.current.type == EventType.MouseDrag)
				{
					return true;
				}
				return false;
			}
		}

		public void ScreenshotModesOnGUI()
		{
			if (KeyBindingDefOf.ToggleScreenshotMode.KeyDownEvent)
			{
				active = !active;
				Event.current.Use();
			}
		}
	}
}
