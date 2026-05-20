using UnityEngine;

namespace Verse
{
	public class ScrollPositioner
	{
		private Rect? interestRect;

		private bool armed;

		public void Arm(bool armed = true)
		{
			this.armed = armed;
		}

		public void ClearInterestRects()
		{
			interestRect = null;
		}

		public void RegisterInterestRect(Rect rect)
		{
			if (interestRect.HasValue)
			{
				interestRect = rect.Union(interestRect.Value);
			}
			else
			{
				interestRect = rect;
			}
		}

		public void ScrollHorizontally(ref Vector2 scrollPos, Vector2 outRectSize)
		{
			Scroll(ref scrollPos, outRectSize, scrollHorizontally: true, scrollVertically: false);
		}

		public void ScrollVertically(ref Vector2 scrollPos, Vector2 outRectSize)
		{
			Scroll(ref scrollPos, outRectSize, scrollHorizontally: false);
		}

		public void Scroll(ref Vector2 scrollPos, Vector2 outRectSize, bool scrollHorizontally = true, bool scrollVertically = true)
		{
			if (Event.current.type != EventType.Layout || !armed)
			{
				return;
			}
			armed = false;
			if (interestRect.HasValue)
			{
				if (scrollHorizontally)
				{
					ScrollInDimension(ref scrollPos.x, outRectSize.x, interestRect.Value.xMin, interestRect.Value.xMax);
				}
				if (scrollVertically)
				{
					ScrollInDimension(ref scrollPos.y, outRectSize.y, interestRect.Value.yMin, interestRect.Value.yMax);
				}
			}
		}

		private void ScrollInDimension(ref float scrollPos, float scrollViewSize, float v0, float v1)
		{
			float num = v1 - v0;
			if (num <= scrollViewSize)
			{
				scrollPos = v0 + num / 2f - scrollViewSize / 2f;
			}
			else
			{
				scrollPos = v0;
			}
		}
	}
}
