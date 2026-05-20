using System;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimWorld;

public class QuickSearchWidget
{
	public QuickSearchFilter filter = new QuickSearchFilter();

	public bool noResultsMatched;

	public Color inactiveTextColor = Color.white;

	public int maxSearchTextLength = 30;

	private readonly string controlName;

	public const float WidgetHeight = 24f;

	public const float IconSize = 18f;

	public const float IconMargin = 4f;

	private const int BaseMaxSearchTextLength = 30;

	private static int instanceCounter;

	public QuickSearchWidget()
	{
		controlName = $"QuickSearchWidget_{instanceCounter++}";
	}

	public void OnGUI(Rect rect, Action onFilterChange = null, Action onClear = null)
	{
		if (CurrentlyFocused() && Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Escape)
		{
			Unfocus();
			Event.current.Use();
		}
		if (OriginalEventUtility.EventType == EventType.MouseDown && !rect.Contains(Event.current.mousePosition))
		{
			Unfocus();
		}
		Color color = GUI.color;
		GUI.color = Color.white;
		float num = Mathf.Min(18f, rect.height);
		float num2 = num + 8f;
		float y = rect.y + (rect.height - num2) / 2f + 4f;
		Rect position = new Rect(rect.x + 4f, y, num, num);
		GUI.DrawTexture(position, TexButton.Search);
		GUI.SetNextControlName(controlName);
		Rect rect2 = new Rect(position.xMax + 4f, rect.y, rect.width - num2, rect.height);
		if (filter.Active)
		{
			rect2.xMax -= num2;
		}
		using (new TextBlock(GameFont.Small))
		{
			if (noResultsMatched && filter.Active)
			{
				GUI.color = ColorLibrary.RedReadable;
			}
			else if (!filter.Active && !CurrentlyFocused())
			{
				GUI.color = inactiveTextColor;
			}
			string text = Widgets.TextField(rect2, filter.Text, maxSearchTextLength);
			GUI.color = Color.white;
			if (text != filter.Text)
			{
				filter.Text = text;
				onFilterChange?.Invoke();
			}
		}
		if (filter.Active && Widgets.ButtonImage(new Rect(rect2.xMax + 4f, y, num, num), TexButton.CloseXSmall))
		{
			filter.Text = "";
			SoundDefOf.CancelMode.PlayOneShotOnCamera();
			onFilterChange?.Invoke();
			onClear?.Invoke();
		}
		GUI.color = color;
	}

	public void Unfocus()
	{
		if (CurrentlyFocused())
		{
			UI.UnfocusCurrentControl();
		}
	}

	public void Focus()
	{
		GUI.FocusControl(controlName);
	}

	public bool CurrentlyFocused()
	{
		return GUI.GetNameOfFocusedControl() == controlName;
	}

	public void Reset()
	{
		filter.Text = "";
		noResultsMatched = false;
	}
}
