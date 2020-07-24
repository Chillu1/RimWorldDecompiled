using System;
using UnityEngine;

namespace Verse
{
	public abstract class InspectTabBase
	{
		public string labelKey;

		protected Vector2 size;

		public string tutorTag;

		private string cachedTutorHighlightTagClosed;

		protected abstract float PaneTopY
		{
			get;
		}

		protected abstract bool StillValid
		{
			get;
		}

		public virtual bool IsVisible => true;

		public string TutorHighlightTagClosed
		{
			get
			{
				if (tutorTag == null)
				{
					return null;
				}
				if (cachedTutorHighlightTagClosed == null)
				{
					cachedTutorHighlightTagClosed = "ITab-" + tutorTag + "-Closed";
				}
				return cachedTutorHighlightTagClosed;
			}
		}

		protected Rect TabRect
		{
			get
			{
				UpdateSize();
				float y = PaneTopY - 30f - size.y;
				return new Rect(0f, y, size.x, size.y);
			}
		}

		public void DoTabGUI()
		{
			Rect rect = TabRect;
			Find.WindowStack.ImmediateWindow(235086, rect, WindowLayer.GameUI, delegate
			{
				if (StillValid && IsVisible)
				{
					if (Widgets.CloseButtonFor(rect.AtZero()))
					{
						CloseTab();
					}
					try
					{
						FillTab();
					}
					catch (Exception ex)
					{
						Log.ErrorOnce(string.Concat("Exception filling tab ", GetType(), ": ", ex), 49827);
					}
				}
			});
			ExtraOnGUI();
		}

		protected abstract void CloseTab();

		protected abstract void FillTab();

		protected virtual void ExtraOnGUI()
		{
		}

		protected virtual void UpdateSize()
		{
		}

		public virtual void OnOpen()
		{
		}

		public virtual void TabTick()
		{
		}

		public virtual void TabUpdate()
		{
		}

		public virtual void Notify_ClearingAllMapsMemory()
		{
		}
	}
}
