using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld;

public class ITab_StudyNotes : ITab
{
	private Vector2 leftScroll;

	private Vector2 rightScroll;

	private ChoiceLetter selectedLetter;

	private Thing previous;

	private const float TopPadding = 20f;

	private const float InitialHeight = 350f;

	private const float TitleHeight = 30f;

	private const float InitialWidth = 610f;

	private const float DateSize = 90f;

	private const float RowHeight = 30f;

	protected Thing StudiableThing => (base.SelThing as Building_HoldingPlatform)?.HeldPawn ?? base.SelThing;

	protected bool Studiable => StudiableThing.TryGetComp<CompStudiable>()?.StudyUnlocked() ?? false;

	public override bool IsVisible
	{
		get
		{
			if (Studiable)
			{
				return StudyUnlocks != null;
			}
			return false;
		}
	}

	private CompStudyUnlocks StudyUnlocks => StudiableThing.TryGetComp<CompStudyUnlocks>();

	protected virtual IReadOnlyList<ChoiceLetter> Letters => StudyUnlocks.Letters;

	protected virtual bool StudyCompleted => StudyUnlocks.Completed;

	protected virtual bool DrawThingIcon => true;

	public ITab_StudyNotes()
	{
		size = new Vector2(Mathf.Min(610f, UI.screenWidth), 350f);
		labelKey = "TabStudyNotesContents";
	}

	public override void OnOpen()
	{
		selectedLetter = (Letters.EnumerableNullOrEmpty() ? null : Letters.Last());
	}

	protected override void FillTab()
	{
		if (previous != StudiableThing)
		{
			selectedLetter = (Letters.EnumerableNullOrEmpty() ? null : Letters.Last());
			previous = StudiableThing;
		}
		Rect rect = new Rect(0f, 20f, size.x, size.y - 20f);
		rect = rect.ContractedBy(10f);
		Rect rect2 = rect;
		rect2.y = 10f;
		rect2.height = 30f;
		Rect rect3 = rect;
		rect3.yMin = rect2.yMax + 17f;
		rect3.SplitVerticallyWithMargin(out var left, out var right, 17f);
		right.yMin += 17f;
		Rect rect4 = right;
		rect4.xMin -= 17f;
		rect4.yMin = rect.yMin;
		DrawTitle(rect2);
		DrawLetters(left);
		if (selectedLetter != null)
		{
			Widgets.LabelScrollable(right, selectedLetter.Text, ref rightScroll);
			return;
		}
		using (new TextBlock(GameFont.Small, TextAnchor.MiddleCenter, Color.gray))
		{
			Widgets.Label(rect4, "StudyNotesTab_NoDiscoveries".Translate());
		}
	}

	private void DrawTitle(Rect rect)
	{
		float num = 0f;
		if (DrawThingIcon)
		{
			Rect position = rect;
			position.width = position.height;
			num = rect.height + 10f;
			GUI.DrawTexture(position, StudiableThing.def.uiIcon);
		}
		Rect rect2 = rect;
		rect2.xMin += num;
		rect2.xMax = rect.x + rect.width / 2f;
		Rect rect3 = rect;
		rect3.xMin = rect2.xMax;
		Rect rect4 = rect3;
		rect4.y = rect2.yMax - 4f;
		using (new TextBlock(GameFont.Medium, TextAnchor.MiddleLeft))
		{
			Widgets.LabelFit(rect2, StudiableThing.LabelCap);
		}
		CompStudiable compStudiable = StudiableThing.TryGetComp<CompStudiable>();
		if (compStudiable == null)
		{
			return;
		}
		Widgets.CheckboxLabeled(rect3, "StudyNotesTab_ToggleStudy".Translate(), ref compStudiable.studyEnabled, disabled: false, null, null, placeCheckboxNearText: true);
		if (!compStudiable.EverStudiable(out var reason) && !reason.NullOrEmpty())
		{
			using (new TextBlock(ColorLibrary.RedReadable))
			{
				Widgets.Label(rect4, reason);
			}
		}
	}

	private void DrawLetters(Rect rect)
	{
		Rect rect2 = rect;
		rect2.height = Text.LineHeight;
		TaggedString taggedString = (StudyCompleted ? "StudyNotesTab_StudyProgressCompleted".Translate() : "StudyNotesTab_StudyProgressOngoing".Translate());
		TaggedString taggedString2 = "StudyNotesTab_StudyProgress".Translate();
		using (new TextBlock(TextAnchor.MiddleLeft))
		{
			Widgets.Label(rect2, $"{taggedString2}: {taggedString}");
		}
		Widgets.DrawLineHorizontal(rect.x, rect2.yMax + 4f, rect.width, Color.gray);
		int num = ((!Letters.EnumerableNullOrEmpty()) ? Letters.Count : 0);
		Rect outRect = rect;
		outRect.yMin = rect2.yMax + 10f;
		Rect rect3 = new Rect(0f, 0f, rect.width, 30f * (float)num);
		float y = 0f;
		Widgets.BeginScrollView(outRect, ref leftScroll, rect3);
		for (int num2 = num - 1; num2 >= 0; num2--)
		{
			DoLetterRow(rect3, ref y, Letters[num2], num2);
		}
		Widgets.EndScrollView();
	}

	private void DoLetterRow(Rect rect, ref float y, ChoiceLetter letter, int index)
	{
		rect.y = y;
		rect.height = 30f;
		y += 30f;
		if (Mouse.IsOver(rect))
		{
			Widgets.DrawHighlightSelected(rect);
			selectedLetter = letter;
		}
		else if (selectedLetter == letter)
		{
			Widgets.DrawHighlightSelected(rect);
		}
		else if (index % 2 == 1)
		{
			Widgets.DrawLightHighlight(rect);
		}
		Rect rect2 = rect;
		rect2.width = 90f;
		Vector2 location = ((Find.CurrentMap != null) ? Find.WorldGrid.LongLatOf(Find.CurrentMap.Tile) : default(Vector2));
		string str = GenDate.DateShortStringAt(GenDate.TickGameToAbs(letter.arrivalTick), location);
		Rect rect3 = rect;
		rect3.xMin = rect2.xMax + 4f;
		using (new TextBlock(GameFont.Small, TextAnchor.MiddleLeft, false))
		{
			Widgets.Label(rect2, str.Truncate(rect2.width));
			using (new TextBlock(new Color(0.75f, 0.75f, 0.75f)))
			{
				Widgets.LabelEllipses(rect3, letter.Label);
			}
		}
	}
}
