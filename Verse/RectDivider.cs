using System;
using UnityEngine;

namespace Verse;

public struct RectDivider
{
	private Rect currentRect;

	private Vector2 margin;

	private int contextHash;

	private int ErrorHash => contextHash ^ 0x3BFEED84;

	public Rect Rect => currentRect;

	public RectDivider(Rect parent, int contextHash, Vector2? margin = null)
	{
		currentRect = parent;
		this.margin = margin ?? new Vector2(17f, 4f);
		this.contextHash = contextHash;
	}

	public static implicit operator Rect(RectDivider rect)
	{
		return rect.currentRect;
	}

	public RectDivider NewRow(float height, VerticalJustification justification = VerticalJustification.Top, float? marginOverride = null)
	{
		switch (justification)
		{
		case VerticalJustification.Top:
		{
			if (!currentRect.SplitHorizontallyWithMargin(out var top2, out var bottom2, out var overflow2, marginOverride ?? margin.y, height))
			{
				Log.ErrorOnce($"Rect height was too small by {overflow2} for the requested row height.", ErrorHash);
			}
			currentRect = bottom2;
			return new RectDivider(top2, contextHash, margin);
		}
		case VerticalJustification.Bottom:
		{
			Rect rect = currentRect;
			float compressibleMargin = marginOverride ?? margin.y;
			float? bottomHeight = height;
			if (!rect.SplitHorizontallyWithMargin(out var top, out var bottom, out var overflow, compressibleMargin, null, bottomHeight))
			{
				Log.ErrorOnce($"Rect height was too small by {overflow} for the requested rows height.", ErrorHash);
			}
			currentRect = top;
			return new RectDivider(bottom, contextHash, margin);
		}
		default:
			throw new InvalidOperationException();
		}
	}

	public RectDivider NewCol(float width, HorizontalJustification justification = HorizontalJustification.Left, float? marginOverride = null)
	{
		switch (justification)
		{
		case HorizontalJustification.Left:
		{
			if (!currentRect.SplitVerticallyWithMargin(out var left2, out var right2, out var overflow2, marginOverride ?? margin.x, width))
			{
				Log.ErrorOnce($"Rect width was too small by {overflow2} for the requested column width.", ErrorHash);
			}
			currentRect = right2;
			return new RectDivider(left2, contextHash, margin);
		}
		case HorizontalJustification.Right:
		{
			Rect rect = currentRect;
			float compressibleMargin = marginOverride ?? margin.x;
			float? rightWidth = width;
			if (!rect.SplitVerticallyWithMargin(out var left, out var right, out var overflow, compressibleMargin, null, rightWidth))
			{
				Log.ErrorOnce($"Rect width was too small by {overflow} for the requested column width.", ErrorHash);
			}
			currentRect = left;
			return new RectDivider(right, contextHash, margin);
		}
		default:
			throw new InvalidOperationException();
		}
	}

	public RectDivider CreateViewRect(int count, float rowHeight)
	{
		float num = 0f;
		if (count > 0)
		{
			num = (float)count * rowHeight + margin.y * (float)(count - 1);
		}
		float num2 = Rect.width;
		if (num > Rect.height)
		{
			num2 -= 16f;
		}
		return new RectDivider(new Rect(0f, 0f, num2, num), contextHash);
	}
}
