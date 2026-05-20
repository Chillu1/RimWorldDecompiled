using System;
using UnityEngine;

namespace Verse
{
	public struct RectAggregator
	{
		private Rect currentRect;

		private Vector2 margin;

		private int contextHash;

		public Rect Rect => currentRect;

		public RectAggregator(Rect parent, int contextHash, Vector2? margin = null)
		{
			currentRect = parent;
			this.margin = margin ?? new Vector2(10f, 4f);
			this.contextHash = contextHash;
		}

		public static implicit operator Rect(RectAggregator rect)
		{
			return rect.currentRect;
		}

		public RectDivider NewRow(float height, VerticalJustification addAt = VerticalJustification.Bottom)
		{
			Rect parent;
			switch (addAt)
			{
			case VerticalJustification.Top:
				currentRect.yMin -= margin.y + height;
				parent = new Rect(currentRect.x, currentRect.y, currentRect.width, height);
				break;
			case VerticalJustification.Bottom:
				currentRect.yMax += margin.y + height;
				parent = new Rect(currentRect.x, currentRect.yMax - height, currentRect.width, height);
				break;
			default:
				throw new InvalidOperationException();
			}
			return new RectDivider(parent, contextHash, margin);
		}

		public RectDivider NewCol(float width, HorizontalJustification addAt = HorizontalJustification.Right)
		{
			Rect parent;
			switch (addAt)
			{
			case HorizontalJustification.Left:
				currentRect.xMin -= margin.x + width;
				parent = new Rect(currentRect.x, currentRect.y, width, currentRect.height);
				break;
			case HorizontalJustification.Right:
				currentRect.xMax += margin.x + width;
				parent = new Rect(currentRect.xMax - width, currentRect.y, width, currentRect.height);
				break;
			default:
				throw new InvalidOperationException();
			}
			return new RectDivider(parent, contextHash, margin);
		}
	}
}
