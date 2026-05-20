using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimWorld;

public class PawnColumnWorker_FoodRestriction : PawnColumnWorker
{
	private const int TopAreaHeight = 65;

	public const int ManageFoodRestrictionsButtonHeight = 32;

	public override void DoHeader(Rect rect, PawnTable table)
	{
		base.DoHeader(rect, table);
		MouseoverSounds.DoRegion(rect);
		if (Widgets.ButtonText(new Rect(rect.x, rect.y + (rect.height - 65f), Mathf.Min(rect.width, 360f), 32f), "ManageFoodPolicies".Translate()))
		{
			Find.WindowStack.Add(new Dialog_ManageFoodPolicies(null));
		}
	}

	public override void DoCell(Rect rect, Pawn pawn, PawnTable table)
	{
		if (pawn.foodRestriction != null)
		{
			DoAssignFoodRestrictionButtons(rect, pawn);
		}
	}

	private IEnumerable<Widgets.DropdownMenuElement<FoodPolicy>> Button_GenerateMenu(Pawn pawn)
	{
		foreach (FoodPolicy foodRestriction in Current.Game.foodRestrictionDatabase.AllFoodRestrictions)
		{
			yield return new Widgets.DropdownMenuElement<FoodPolicy>
			{
				option = new FloatMenuOption(foodRestriction.label, delegate
				{
					pawn.foodRestriction.CurrentFoodPolicy = foodRestriction;
				}),
				payload = foodRestriction
			};
		}
		yield return new Widgets.DropdownMenuElement<FoodPolicy>
		{
			option = new FloatMenuOption(string.Format("{0}...", "AssignTabEdit".Translate()), delegate
			{
				Find.WindowStack.Add(new Dialog_ManageFoodPolicies(pawn.foodRestriction.CurrentFoodPolicy));
			})
		};
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
		if (pawn.foodRestriction != null && pawn.foodRestriction.CurrentFoodPolicy != null)
		{
			return pawn.foodRestriction.CurrentFoodPolicy.id;
		}
		return int.MinValue;
	}

	private void DoAssignFoodRestrictionButtons(Rect rect, Pawn pawn)
	{
		Rect rect2 = rect.ContractedBy(0f, 2f);
		Widgets.Dropdown(rect2, pawn, (Pawn p) => p.foodRestriction.CurrentFoodPolicy, Button_GenerateMenu, pawn.foodRestriction.CurrentFoodPolicy.label.Truncate(rect2.width), null, pawn.foodRestriction.CurrentFoodPolicy.label, null, null, paintable: true);
	}
}
