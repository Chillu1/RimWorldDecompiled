using UnityEngine;
using Verse;

namespace RimWorld;

public class ITab_Book : ITab
{
	private Vector2 infoScroll;

	protected const float TopPadding = 20f;

	protected const float InitialHeight = 350f;

	public const float TitleHeight = 30f;

	protected const float InitialWidth = 610f;

	public override bool IsVisible => base.SelThing is Book;

	public ITab_Book()
	{
		size = new Vector2(Mathf.Min(610f, UI.screenWidth), 350f);
		labelKey = "TabBookContents";
	}

	protected override void FillTab()
	{
		if (base.SelThing is Book book)
		{
			Rect rect = new Rect(0f, 20f, size.x, size.y - 20f).ContractedBy(10f);
			Rect rect2 = rect;
			rect2.y = 10f;
			rect2.height = 30f;
			Rect rect3 = rect;
			rect3.xMax = rect.center.x - 17f;
			rect3.y = rect2.yMax + 17f;
			rect3.yMax = rect.yMax;
			Rect rect4 = rect;
			rect4.x = rect3.xMax + 20f;
			rect4.xMax = rect.xMax + 10f;
			rect4.y = rect2.yMax + 26f;
			rect4.yMax = rect.yMax;
			BookUIUtility.DrawTitle(rect2, book);
			BookUIUtility.DrawBookInfoPanel(rect3, book);
			BookUIUtility.DrawBookDescPanel(rect4, book, ref infoScroll);
		}
	}
}
