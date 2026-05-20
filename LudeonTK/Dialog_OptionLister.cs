using System;
using RimWorld;
using UnityEngine;
using Verse;

namespace LudeonTK;

public abstract class Dialog_OptionLister : Window_Dev
{
	protected Vector2 scrollPosition;

	protected string filter = "";

	protected float totalOptionsHeight;

	protected bool focusOnFilterOnOpen = true;

	private bool focusFilter;

	protected float curY;

	protected float curX;

	private int boundingRectCachedForFrame = -1;

	private Rect? boundingRectCached;

	private Rect? boundingRect;

	public float verticalSpacing = 2f;

	protected const string FilterControlName = "DebugFilter";

	public override Vector2 InitialSize => new Vector2(UI.screenWidth, UI.screenHeight);

	public override bool IsDebug => true;

	public Rect? BoundingRectCached
	{
		get
		{
			if (boundingRectCachedForFrame != Time.frameCount)
			{
				if (boundingRect.HasValue)
				{
					Rect value = boundingRect.Value;
					Vector2 vector = scrollPosition;
					value.x += vector.x;
					value.y += vector.y;
					boundingRectCached = value;
				}
				boundingRectCachedForFrame = Time.frameCount;
			}
			return boundingRectCached;
		}
	}

	public Dialog_OptionLister()
	{
		doCloseX = true;
		onlyOneOfTypeAllowed = true;
		absorbInputAroundWindow = true;
	}

	public override void PostOpen()
	{
		base.PostOpen();
		if (focusOnFilterOnOpen)
		{
			focusFilter = true;
		}
	}

	public override void DoWindowContents(Rect inRect)
	{
		GUI.SetNextControlName("DebugFilter");
		if (Event.current.type != EventType.KeyDown || (!KeyBindingDefOf.Dev_ToggleDebugSettingsMenu.KeyDownEvent && !KeyBindingDefOf.Dev_ToggleDebugActionsMenu.KeyDownEvent))
		{
			filter = DevGUI.TextField(new Rect(0f, 0f, 200f, 30f), filter);
			if ((Event.current.type == EventType.KeyDown || Event.current.type == EventType.Repaint) && focusFilter)
			{
				GUI.FocusControl("DebugFilter");
				filter = "";
				focusFilter = false;
			}
			if (Event.current.type == EventType.Layout)
			{
				totalOptionsHeight = 0f;
			}
			Rect outRect = new Rect(inRect);
			outRect.yMin += 35f;
			int num = (int)(InitialSize.x / 200f);
			float num2 = (totalOptionsHeight + 24f * (float)(num - 1)) / (float)num;
			if (num2 < outRect.height)
			{
				num2 = outRect.height;
			}
			curX = 0f;
			curY = 0f;
			Rect viewRect = new Rect(0f, 0f, outRect.width - 16f, num2);
			DevGUI.BeginScrollView(outRect, ref scrollPosition, viewRect);
			DevGUI.BeginGroup(inRect);
			float columnWidth = (viewRect.width - 17f * (float)(num - 1)) / (float)num;
			DoListingItems(inRect.AtZero(), columnWidth);
			DevGUI.EndGroup();
			DevGUI.EndScrollView();
		}
	}

	public override void PostClose()
	{
		base.PostClose();
		UI.UnfocusCurrentControl();
	}

	protected abstract void DoListingItems(Rect inRect, float columnWidth);

	protected bool FilterAllows(string label)
	{
		if (filter.NullOrEmpty())
		{
			return true;
		}
		if (label.NullOrEmpty())
		{
			return true;
		}
		return label.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0;
	}
}
