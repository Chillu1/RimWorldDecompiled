using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld;

public static class BookUIUtility
{
	private const float KeyValSplit = 140f;

	private const float LineHeight = 25f;

	private const float HyperlinkBaseGap = 4f;

	public static void DrawTitle(Rect rect, Book book)
	{
		Rect position = rect;
		position.width = position.height;
		GUI.DrawTexture(position, book.def.uiIcon);
		Text.Anchor = TextAnchor.MiddleLeft;
		Text.Font = GameFont.Medium;
		Rect rect2 = rect;
		rect2.x += rect.height + 10f;
		rect2.width -= rect.height + 10f;
		Widgets.LabelFit(rect2, book.LabelCap);
		Text.Font = GameFont.Small;
		Text.Anchor = TextAnchor.UpperLeft;
	}

	public static void DrawBookDescPanel(Rect rect, Book book, ref Vector2 scroll)
	{
		Widgets.LabelScrollable(rect, book.FlavorUI, ref scroll);
	}

	public static void DrawBookInfoPanel(Rect rect, Book book)
	{
		float y = rect.y;
		DrawBenefits(rect, ref y, book);
		DrawDangers(rect, ref y, book);
		Rect rect2 = rect;
		rect2.y = y;
		rect2.yMax = rect.yMax;
		DrawHyperlinks(rect2, ref y, book);
	}

	private static void DrawDangers(Rect rect, ref float y, Book book)
	{
		if (book.MentalBreakChancePerHour > 0f)
		{
			DrawSubheader(rect, ref y, "Dangers".Translate());
			y += 10f;
			Widgets.Label(rect, ref y, string.Format("- {0}: {1}", "BookMentalBreak".Translate(), "PerHour".Translate(book.MentalBreakChancePerHour.ToStringPercent("0.0"))));
			y += 10f;
		}
	}

	private static void DrawSubheader(Rect rect, ref float y, string title)
	{
		Rect position = new Rect
		{
			x = rect.x,
			y = y,
			xMax = rect.xMax,
			yMax = rect.yMax
		};
		GUI.BeginGroup(position);
		float curY = 0f;
		Widgets.ListSeparator(ref curY, position.width, title);
		y += curY;
		GUI.EndGroup();
	}

	private static void DrawBenefits(Rect rect, ref float y, Book book)
	{
		bool flag = false;
		foreach (BookOutcomeDoer doer in book.BookComp.Doers)
		{
			if (!string.IsNullOrEmpty(doer.GetBenefitsString()))
			{
				flag = true;
				break;
			}
		}
		if (!flag)
		{
			return;
		}
		DrawSubheader(rect, ref y, "Benefits".Translate());
		y += 10f;
		foreach (BookOutcomeDoer doer2 in book.BookComp.Doers)
		{
			string benefitsString = doer2.GetBenefitsString();
			if (!string.IsNullOrEmpty(benefitsString))
			{
				Widgets.Label(rect, ref y, benefitsString);
			}
		}
		y += 10f;
	}

	private static void DrawHyperlinks(Rect rect, ref float y, Book book)
	{
		Color normalOptionColor = Widgets.NormalOptionColor;
		float num = 0f;
		foreach (Dialog_InfoCard.Hyperlink item in book.GetHyperlinks().Reverse())
		{
			float num2 = Text.CalcHeight(item.Label, rect.width);
			Rect rect2 = rect;
			rect2.y = rect.yMax - num2 - num - 4f;
			rect2.height = num2;
			TaggedString taggedString = "ViewHyperlink".Translate(item.Label);
			Widgets.HyperlinkWithIcon(rect2, item, taggedString, 2f, 6f, normalOptionColor);
			num += num2;
			y += num2;
		}
	}
}
