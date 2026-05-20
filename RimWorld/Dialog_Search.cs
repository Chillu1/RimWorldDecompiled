using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld;

public abstract class Dialog_Search<T> : Window where T : class
{
	protected QuickSearchWidget quickSearchWidget;

	protected SortedList<string, T> searchResults;

	protected HashSet<T> searchResultsSet;

	protected T highlightedElement;

	protected Vector2 scrollPos;

	protected float scrollHeight;

	protected List<T> allElements;

	protected int searchIndex;

	protected bool triedToFocus;

	private int openFrames;

	protected const float ElementHeight = 26f;

	private const int MaxSearchesPerFrame = 500;

	public override Vector2 InitialSize => new Vector2(350f, 100f);

	public override QuickSearchWidget CommonSearchWidget => quickSearchWidget;

	protected bool Searching
	{
		get
		{
			if (!quickSearchWidget.filter.Text.NullOrEmpty() && allElements.Any())
			{
				return searchIndex < allElements.Count;
			}
			return false;
		}
	}

	protected abstract List<T> SearchSet { get; }

	protected abstract bool ShouldClose { get; }

	protected abstract TaggedString SearchLabel { get; }

	public T Highlighted => highlightedElement;

	protected abstract void TryAddElement(T element);

	protected abstract void TryRemoveElement(T element);

	protected abstract void DoIcon(T element, Rect iconRect);

	protected abstract void DoLabel(T element, Rect labelRect);

	protected virtual void DoExtraIcon(T element, Rect iconRect)
	{
	}

	protected abstract void ClikedOnElement(T element);

	protected abstract bool ShouldSkipElement(T element);

	protected abstract void OnHighlightUpdate(T element);

	protected virtual void CheckAnyElementRemoved()
	{
	}

	public bool IsListed(T element)
	{
		return searchResultsSet.Contains(element);
	}

	protected override Rect QuickSearchWidgetRect(Rect winRect, Rect inRect)
	{
		return new Rect(inRect.x, inRect.yMax - 24f, inRect.width, 24f);
	}

	public Dialog_Search()
	{
		doCloseX = true;
		closeOnAccept = false;
		preventCameraMotion = false;
		quickSearchWidget = new QuickSearchWidget();
		searchResults = new SortedList<string, T>(new DuplicateKeyComparer<string>());
		searchResultsSet = new HashSet<T>();
		allElements = new List<T>();
	}

	public override void DoWindowContents(Rect inRect)
	{
		Text.Font = GameFont.Small;
		highlightedElement = null;
		float num = Text.CalcHeight(SearchLabel, inRect.width);
		Rect rect = new Rect(0f, inRect.yMax - 24f - num, inRect.width, num);
		using (new TextBlock(ColoredText.SubtleGrayColor, TextAnchor.MiddleLeft, newWordWrap: true))
		{
			Widgets.Label(label: Searching ? QuickSearchUtility.CurrentSearchText : ((quickSearchWidget.filter.Text.Length <= 0) ? ((string)SearchLabel) : ((string)((searchResults.Count == 1) ? "MapSearchResultSingular".Translate() : "MapSearchResults".Translate(searchResults.Count)))), rect: rect);
		}
		if (searchResults.Count > 0)
		{
			Rect outRect = new Rect(0f, 0f, inRect.width, inRect.height);
			outRect.yMax = rect.yMin;
			Rect viewRect = new Rect(0f, 0f, inRect.width - 16f, scrollHeight);
			Rect rect2 = new Rect(0f, scrollPos.y, outRect.width, outRect.height);
			bool flag = scrollHeight >= outRect.height;
			Widgets.BeginScrollView(outRect, ref scrollPos, viewRect);
			using (new ProfilerBlock("DrawSearchResults"))
			{
				for (int i = 0; i < searchResults.Count; i++)
				{
					Rect rect3 = new Rect(0f, 26f * (float)i, inRect.width, 26f);
					if (!rect2.Overlaps(rect3))
					{
						continue;
					}
					if (i % 2 == 1)
					{
						Widgets.DrawLightHighlight(rect3);
					}
					T val = searchResults.Values[i];
					if (val != null && !ShouldSkipElement(val))
					{
						Rect iconRect = rect3;
						iconRect.xMax = 26f;
						DoIcon(val, iconRect);
						Rect iconRect2 = rect3;
						if (flag)
						{
							iconRect2.xMin = rect3.xMax - 26f - 16f;
							iconRect2.width = 26f;
						}
						else
						{
							iconRect2.xMin = rect3.xMax - 26f;
						}
						DoExtraIcon(val, iconRect2);
						Rect labelRect = rect3;
						labelRect.xMin = iconRect.xMax + 4f;
						labelRect.xMax = iconRect2.xMin - 4f;
						DoLabel(val, labelRect);
						if (Mouse.IsOver(rect3))
						{
							Widgets.DrawHighlight(rect3);
							highlightedElement = val;
						}
						if (Widgets.ButtonInvisible(rect3))
						{
							ClikedOnElement(val);
						}
					}
				}
			}
			Widgets.EndScrollView();
		}
		if (!triedToFocus && openFrames == 2)
		{
			quickSearchWidget.Focus();
			triedToFocus = true;
		}
	}

	public override void WindowUpdate()
	{
		base.WindowUpdate();
		openFrames++;
		if (ShouldClose)
		{
			Close();
			return;
		}
		if (highlightedElement != null)
		{
			OnHighlightUpdate(highlightedElement);
		}
		if (Searching)
		{
			using (new ProfilerBlock("Searching"))
			{
				for (int i = 0; i < 500; i++)
				{
					searchIndex++;
					if (searchIndex >= allElements.Count)
					{
						allElements.Clear();
						break;
					}
					TryAddElement(allElements[searchIndex]);
				}
				return;
			}
		}
		if (Time.frameCount % 20 == 0)
		{
			CheckAnyElementRemoved();
		}
	}

	protected override void SetInitialSizeAndPosition()
	{
		scrollHeight = (float)searchResults.Count * 26f;
		Vector2 initialSize = InitialSize;
		initialSize.y = Mathf.Clamp(initialSize.y + scrollHeight, InitialSize.y, (float)UI.screenHeight / 2f);
		windowRect = new Rect((float)UI.screenWidth - initialSize.x, (float)UI.screenHeight - initialSize.y - 35f, initialSize.x, initialSize.y).Rounded();
	}

	public override void Notify_CommonSearchChanged()
	{
		scrollPos = Vector2.zero;
		searchIndex = 0;
		searchResults.Clear();
		searchResultsSet.Clear();
		allElements.Clear();
		allElements.AddRange(SearchSet);
		SetInitialSizeAndPosition();
	}

	protected bool TextMatch(string text)
	{
		if (text.NullOrEmpty())
		{
			return false;
		}
		return text.IndexOf(quickSearchWidget.filter.Text, StringComparison.InvariantCultureIgnoreCase) >= 0;
	}
}
