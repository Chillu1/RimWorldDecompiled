using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimWorld;

public class PawnColumnWorker_Reading : PawnColumnWorker
{
	private const int TopAreaHeight = 65;

	private const int ManageReadingButtonHeight = 32;

	public override void DoHeader(Rect rect, PawnTable table)
	{
		base.DoHeader(rect, table);
		MouseoverSounds.DoRegion(rect);
		Rect rect2 = new Rect(rect.x, rect.y + (rect.height - 65f), Mathf.Min(rect.width, 360f), 32f);
		if (Widgets.ButtonText(rect2, "ManageReadingPolicies".Translate()))
		{
			Find.WindowStack.Add(new Dialog_ManageReadingPolicies(null));
		}
		UIHighlighter.HighlightOpportunity(rect2, "ManageReadingAssignments");
		UIHighlighter.HighlightOpportunity(rect2, "ReadingAssignPolicy");
	}

	public override void DoCell(Rect rect, Pawn pawn, PawnTable table)
	{
		if (pawn.reading != null)
		{
			ReadingColumnUIUtility.DoAssignReadingButtons(rect, pawn);
		}
	}

	public override int GetMinWidth(PawnTable table)
	{
		return Mathf.Max(base.GetMinWidth(table), Mathf.CeilToInt(194f));
	}

	public override int GetOptimalWidth(PawnTable table)
	{
		return Mathf.Clamp(Mathf.CeilToInt(251f), GetMinWidth(table), GetMaxWidth(table));
	}

	public override int GetMinHeaderHeight(PawnTable table)
	{
		return Mathf.Max(base.GetMinHeaderHeight(table), 65);
	}

	public override int Compare(Pawn a, Pawn b)
	{
		return GetValueToCompare(a).CompareTo(GetValueToCompare(b));
	}

	private int GetValueToCompare(Pawn pawn)
	{
		if (pawn.reading != null && pawn.reading.CurrentPolicy != null)
		{
			return pawn.reading.CurrentPolicy.id;
		}
		return int.MinValue;
	}
}
