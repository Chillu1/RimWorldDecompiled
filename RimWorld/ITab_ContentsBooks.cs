using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld;

public class ITab_ContentsBooks : ITab_ContentsBase
{
	private static readonly CachedTexture DropTex = new CachedTexture("UI/Buttons/Drop");

	public override IList<Thing> container => Bookcase.GetDirectlyHeldThings().ToList();

	public override bool IsVisible
	{
		get
		{
			if (base.SelThing != null)
			{
				return base.IsVisible;
			}
			return false;
		}
	}

	public Building_Bookcase Bookcase => base.SelThing as Building_Bookcase;

	public override bool VisibleInBlueprintMode => false;

	public ITab_ContentsBooks()
	{
		labelKey = "TabCasketContents";
		containedItemsKey = "TabCasketContents";
	}

	protected override void DoItemsLists(Rect inRect, ref float curY)
	{
		ListContainedBooks(inRect, container, ref curY);
	}

	private void ListContainedBooks(Rect inRect, IList<Thing> books, ref float curY)
	{
		GUI.BeginGroup(inRect);
		float num = curY;
		Widgets.ListSeparator(ref curY, inRect.width, containedItemsKey.Translate());
		Rect rect = new Rect(0f, num, inRect.width, curY - num - 3f);
		if (Mouse.IsOver(rect))
		{
			Widgets.DrawHighlight(rect);
			TooltipHandler.TipRegionByKey(rect, "ContainedBooksDesc");
		}
		bool flag = false;
		for (int i = 0; i < books.Count; i++)
		{
			if (books[i] is Book book)
			{
				flag = true;
				DoRow(book, inRect.width, i, ref curY);
			}
		}
		if (!flag)
		{
			Widgets.NoneLabel(ref curY, inRect.width);
		}
		GUI.EndGroup();
	}

	private void DoRow(Book book, float width, int i, ref float curY)
	{
		Rect rect = new Rect(0f, curY, width, 28f);
		Widgets.InfoCardButton(0f, curY, book);
		if (Mouse.IsOver(rect))
		{
			Widgets.DrawHighlightSelected(rect);
		}
		else if (i % 2 == 1)
		{
			Widgets.DrawLightHighlight(rect);
		}
		Rect rect2 = new Rect(rect.width - 24f, curY, 24f, 24f);
		if (Widgets.ButtonImage(rect2, DropTex.Texture))
		{
			if (!Bookcase.OccupiedRect().AdjacentCells.Where((IntVec3 x) => x.Walkable(Bookcase.Map)).TryRandomElement(out var result))
			{
				result = Bookcase.Position;
			}
			Bookcase.GetDirectlyHeldThings().TryDrop(book, result, Bookcase.Map, ThingPlaceMode.Near, 1, out var resultingThing);
			if (resultingThing.TryGetComp(out CompForbiddable comp))
			{
				comp.Forbidden = true;
			}
		}
		else if (Widgets.ButtonInvisible(rect))
		{
			Find.Selector.ClearSelection();
			Find.Selector.Select(book);
			InspectPaneUtility.OpenTab(typeof(ITab_Book));
		}
		TooltipHandler.TipRegionByKey(rect2, "EjectBookTooltip");
		Widgets.ThingIcon(new Rect(24f, curY, 28f, 28f), book);
		Rect rect3 = new Rect(60f, curY, rect.width - 36f, rect.height);
		rect3.xMax = rect2.xMin;
		Text.Anchor = TextAnchor.MiddleLeft;
		Widgets.Label(rect3, book.LabelCap.Truncate(rect3.width));
		Text.Anchor = TextAnchor.UpperLeft;
		if (Mouse.IsOver(rect))
		{
			TargetHighlighter.Highlight(book, arrow: true, colonistBar: false);
			TooltipHandler.TipRegion(rect, book.DescriptionDetailed);
		}
		curY += 28f;
	}
}
