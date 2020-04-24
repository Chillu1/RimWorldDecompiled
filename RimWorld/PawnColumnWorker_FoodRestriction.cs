using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimWorld
{
	public class PawnColumnWorker_FoodRestriction : PawnColumnWorker
	{
		private const int TopAreaHeight = 65;

		public const int ManageFoodRestrictionsButtonHeight = 32;

		public override void DoHeader(Rect rect, PawnTable table)
		{
			base.DoHeader(rect, table);
			MouseoverSounds.DoRegion(rect);
			if (Widgets.ButtonText(new Rect(rect.x, rect.y + (rect.height - 65f), Mathf.Min(rect.width, 360f), 32f), "ManageFoodRestrictions".Translate()))
			{
				Find.WindowStack.Add(new Dialog_ManageFoodRestrictions(null));
			}
		}

		public override void DoCell(Rect rect, Pawn pawn, PawnTable table)
		{
			if (pawn.foodRestriction != null)
			{
				DoAssignFoodRestrictionButtons(rect, pawn);
			}
		}

		private IEnumerable<Widgets.DropdownMenuElement<FoodRestriction>> Button_GenerateMenu(Pawn pawn)
		{
			foreach (FoodRestriction foodRestriction in Current.Game.foodRestrictionDatabase.AllFoodRestrictions)
			{
				yield return new Widgets.DropdownMenuElement<FoodRestriction>
				{
					option = new FloatMenuOption(foodRestriction.label, delegate
					{
						pawn.foodRestriction.CurrentFoodRestriction = foodRestriction;
					}),
					payload = foodRestriction
				};
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
			if (pawn.foodRestriction != null && pawn.foodRestriction.CurrentFoodRestriction != null)
			{
				return pawn.foodRestriction.CurrentFoodRestriction.id;
			}
			return int.MinValue;
		}

		private void DoAssignFoodRestrictionButtons(Rect rect, Pawn pawn)
		{
			int num = Mathf.FloorToInt((rect.width - 4f) * 0.714285731f);
			int num2 = Mathf.FloorToInt((rect.width - 4f) * 0.2857143f);
			float x = rect.x;
			Rect rect2 = new Rect(x, rect.y + 2f, num, rect.height - 4f);
			Widgets.Dropdown(rect2, pawn, (Pawn p) => p.foodRestriction.CurrentFoodRestriction, Button_GenerateMenu, pawn.foodRestriction.CurrentFoodRestriction.label.Truncate(rect2.width), null, pawn.foodRestriction.CurrentFoodRestriction.label, null, null, paintable: true);
			x += (float)num;
			x += 4f;
			if (Widgets.ButtonText(new Rect(x, rect.y + 2f, num2, rect.height - 4f), "AssignTabEdit".Translate()))
			{
				Find.WindowStack.Add(new Dialog_ManageFoodRestrictions(pawn.foodRestriction.CurrentFoodRestriction));
			}
			x += (float)num2;
		}
	}
}
