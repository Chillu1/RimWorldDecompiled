using UnityEngine;

namespace Verse
{
	public abstract class Listing
	{
		public float verticalSpacing = 2f;

		protected Rect listingRect;

		protected float curY;

		protected float curX;

		private float columnWidthInt;

		private bool hasCustomColumnWidth;

		public bool maxOneColumn;

		public const float ColumnSpacing = 17f;

		public const float DefaultGap = 12f;

		public float CurHeight => curY;

		public float ColumnWidth
		{
			get
			{
				return columnWidthInt;
			}
			set
			{
				columnWidthInt = value;
				hasCustomColumnWidth = true;
			}
		}

		public void NewColumn()
		{
			curY = 0f;
			curX += ColumnWidth + 17f;
		}

		protected void NewColumnIfNeeded(float neededHeight)
		{
			if (!maxOneColumn && curY + neededHeight > listingRect.height)
			{
				NewColumn();
			}
		}

		public Rect GetRect(float height)
		{
			NewColumnIfNeeded(height);
			Rect result = new Rect(curX, curY, ColumnWidth, height);
			curY += height;
			return result;
		}

		public void Gap(float gapHeight = 12f)
		{
			curY += gapHeight;
		}

		public void GapLine(float gapHeight = 12f)
		{
			float y = curY + gapHeight / 2f;
			Color color = GUI.color;
			GUI.color = color * new Color(1f, 1f, 1f, 0.4f);
			Widgets.DrawLineHorizontal(curX, y, ColumnWidth);
			GUI.color = color;
			curY += gapHeight;
		}

		public virtual void Begin(Rect rect)
		{
			listingRect = rect;
			if (hasCustomColumnWidth)
			{
				if (columnWidthInt > listingRect.width)
				{
					Log.Error("Listing set ColumnWith to " + columnWidthInt + " which is more than the whole listing rect width of " + listingRect.width + ". Clamping.");
					columnWidthInt = listingRect.width;
				}
			}
			else
			{
				columnWidthInt = listingRect.width;
			}
			curX = 0f;
			curY = 0f;
			GUI.BeginGroup(rect);
		}

		public virtual void End()
		{
			GUI.EndGroup();
		}
	}
}
