using System;
using UnityEngine;

namespace Verse
{
	public abstract class Dialog_OptionLister : Window
	{
		protected Vector2 scrollPosition;

		protected string filter = "";

		protected float totalOptionsHeight;

		protected Listing_Standard listing;

		protected bool focusOnFilterOnOpen = true;

		private bool focusFilter;

		protected const string FilterControlName = "DebugFilter";

		public override Vector2 InitialSize => new Vector2(UI.screenWidth, UI.screenHeight);

		public override bool IsDebug => true;

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
			filter = Widgets.TextField(new Rect(0f, 0f, 200f, 30f), filter);
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
			Rect rect = new Rect(0f, 0f, outRect.width - 16f, num2);
			Widgets.BeginScrollView(outRect, ref scrollPosition, rect);
			listing = new Listing_Standard();
			listing.ColumnWidth = (rect.width - 17f * (float)(num - 1)) / (float)num;
			listing.Begin(rect);
			DoListingItems();
			listing.End();
			Widgets.EndScrollView();
		}

		public override void PostClose()
		{
			base.PostClose();
			UI.UnfocusCurrentControl();
		}

		protected abstract void DoListingItems();

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
}
