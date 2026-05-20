using System.Text;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimWorld;

[StaticConstructorOnStartup]
public abstract class PawnColumnWorker
{
	public PawnColumnDef def;

	protected const int DefaultCellHeight = 30;

	private static readonly Texture2D SortingIcon = ContentFinder<Texture2D>.Get("UI/Icons/Sorting");

	private static readonly Texture2D SortingDescendingIcon = ContentFinder<Texture2D>.Get("UI/Icons/SortingDescending");

	private const int IconMargin = 2;

	protected virtual Color DefaultHeaderColor => Color.white;

	protected virtual GameFont DefaultHeaderFont => GameFont.Small;

	protected virtual TextAnchor DefaultHeaderAlignment => TextAnchor.LowerCenter;

	public virtual bool VisibleCurrently => true;

	public virtual void DoHeader(Rect rect, PawnTable table)
	{
		if (!def.label.NullOrEmpty())
		{
			Text.Font = DefaultHeaderFont;
			GUI.color = DefaultHeaderColor;
			Text.Anchor = DefaultHeaderAlignment;
			Rect rect2 = rect;
			rect2.xMin += GetHeaderOffsetX(rect);
			Widgets.Label(rect2, def.LabelCap.Resolve().Truncate(rect.width));
			Text.Anchor = TextAnchor.UpperLeft;
			GUI.color = Color.white;
			Text.Font = GameFont.Small;
		}
		else if (def.HeaderIcon != null)
		{
			Vector2 headerIconSize = def.HeaderIconSize;
			int num = (int)((rect.width - headerIconSize.x) / 2f);
			GUI.DrawTexture(new Rect(rect.x + (float)num, rect.yMax - headerIconSize.y, headerIconSize.x, headerIconSize.y).ContractedBy(2f), def.HeaderIcon);
		}
		if (table.SortingBy == def)
		{
			Texture2D texture2D = (table.SortingDescending ? SortingDescendingIcon : SortingIcon);
			GUI.DrawTexture(new Rect(rect.xMax - (float)texture2D.width - 1f, rect.yMax - (float)texture2D.height - 1f, texture2D.width, texture2D.height), texture2D);
		}
		if (!def.HeaderInteractable)
		{
			return;
		}
		Rect interactableHeaderRect = GetInteractableHeaderRect(rect, table);
		if (Mouse.IsOver(interactableHeaderRect))
		{
			Widgets.DrawHighlight(interactableHeaderRect);
			string headerTip = GetHeaderTip(table);
			if (!headerTip.NullOrEmpty())
			{
				TooltipHandler.TipRegion(interactableHeaderRect, headerTip);
			}
		}
		if (Widgets.ButtonInvisible(interactableHeaderRect))
		{
			HeaderClicked(rect, table);
		}
	}

	protected virtual float GetHeaderOffsetX(Rect rect)
	{
		return 0f;
	}

	public abstract void DoCell(Rect rect, Pawn pawn, PawnTable table);

	public virtual bool CanGroupWith(Pawn pawn, Pawn other)
	{
		return false;
	}

	public virtual void Recache()
	{
	}

	public virtual int GetMinWidth(PawnTable table)
	{
		if (!def.label.NullOrEmpty())
		{
			Text.Font = DefaultHeaderFont;
			int result = Mathf.CeilToInt(Text.CalcSize(def.LabelCap).x);
			Text.Font = GameFont.Small;
			return result;
		}
		if (def.HeaderIcon != null)
		{
			return Mathf.CeilToInt(def.HeaderIconSize.x);
		}
		return 1;
	}

	public virtual int GetMaxWidth(PawnTable table)
	{
		return 1000000;
	}

	public virtual int GetOptimalWidth(PawnTable table)
	{
		return GetMinWidth(table);
	}

	public virtual int GetMinCellHeight(Pawn pawn)
	{
		return 30;
	}

	public virtual int GetMinHeaderHeight(PawnTable table)
	{
		if (!def.label.NullOrEmpty())
		{
			Text.Font = DefaultHeaderFont;
			int result = Mathf.CeilToInt(Text.CalcSize(def.LabelCap).y);
			Text.Font = GameFont.Small;
			return result;
		}
		if (def.HeaderIcon != null)
		{
			return Mathf.CeilToInt(def.HeaderIconSize.y);
		}
		return 0;
	}

	public virtual int Compare(Pawn a, Pawn b)
	{
		return 0;
	}

	protected virtual Rect GetInteractableHeaderRect(Rect headerRect, PawnTable table)
	{
		float num = Mathf.Min(25f, headerRect.height);
		return new Rect(headerRect.x, headerRect.yMax - num, headerRect.width, num);
	}

	protected virtual void HeaderClicked(Rect headerRect, PawnTable table)
	{
		if (!def.sortable || Event.current.shift)
		{
			return;
		}
		if (Event.current.button == 0)
		{
			if (table.SortingBy != def)
			{
				table.SortBy(def, descending: true);
				SoundDefOf.Tick_High.PlayOneShotOnCamera();
			}
			else if (table.SortingDescending)
			{
				table.SortBy(def, descending: false);
				SoundDefOf.Tick_High.PlayOneShotOnCamera();
			}
			else
			{
				table.SortBy(null, descending: false);
				SoundDefOf.Tick_Low.PlayOneShotOnCamera();
			}
		}
		else if (Event.current.button == 1)
		{
			if (table.SortingBy != def)
			{
				table.SortBy(def, descending: false);
				SoundDefOf.Tick_High.PlayOneShotOnCamera();
			}
			else if (table.SortingDescending)
			{
				table.SortBy(null, descending: false);
				SoundDefOf.Tick_Low.PlayOneShotOnCamera();
			}
			else
			{
				table.SortBy(def, descending: true);
				SoundDefOf.Tick_High.PlayOneShotOnCamera();
			}
		}
	}

	protected virtual string GetHeaderTip(PawnTable table)
	{
		StringBuilder stringBuilder = new StringBuilder();
		if (!def.headerTip.NullOrEmpty())
		{
			stringBuilder.Append(def.headerTip);
		}
		if (def.sortable)
		{
			if (stringBuilder.Length != 0)
			{
				stringBuilder.AppendLine();
				stringBuilder.AppendLine();
			}
			stringBuilder.Append("ClickToSortByThisColumn".Translate());
		}
		return stringBuilder.ToString();
	}
}
