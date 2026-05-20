using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld;

public static class BillDialogUtility
{
	public static IEnumerable<Widgets.DropdownMenuElement<Pawn>> GetPawnRestrictionOptionsForBill(Bill bill, Func<Pawn, bool> pawnValidator = null)
	{
		SkillDef workSkill = bill.recipe.workSkill;
		IEnumerable<Pawn> allMaps_FreeColonists = PawnsFinder.AllMaps_FreeColonists;
		allMaps_FreeColonists = from pawn2 in allMaps_FreeColonists
			where pawnValidator == null || pawnValidator(pawn2)
			orderby pawn2.LabelShortCap
			select pawn2;
		if (workSkill != null)
		{
			allMaps_FreeColonists = allMaps_FreeColonists.OrderByDescending((Pawn pawn2) => pawn2.skills.GetSkill(bill.recipe.workSkill).Level);
		}
		WorkGiverDef workGiver = bill.billStack.billGiver.GetWorkgiver();
		if (workGiver == null)
		{
			Log.ErrorOnce("Generating pawn restrictions for a BillGiver without a Workgiver", 96455148);
			yield break;
		}
		allMaps_FreeColonists = allMaps_FreeColonists.OrderByDescending((Pawn pawn2) => pawn2.workSettings.WorkIsActive(workGiver.workType));
		allMaps_FreeColonists = allMaps_FreeColonists.OrderBy((Pawn pawn2) => pawn2.WorkTypeIsDisabled(workGiver.workType));
		foreach (Pawn pawn in allMaps_FreeColonists)
		{
			if (pawn.WorkTypeIsDisabled(workGiver.workType))
			{
				yield return new Widgets.DropdownMenuElement<Pawn>
				{
					option = new FloatMenuOption(string.Format("{0} ({1})", pawn.LabelShortCap, "WillNever".Translate(workGiver.label)), null),
					payload = pawn
				};
			}
			else if (bill.recipe.workSkill != null && !pawn.workSettings.WorkIsActive(workGiver.workType))
			{
				yield return new Widgets.DropdownMenuElement<Pawn>
				{
					option = new FloatMenuOption(string.Format("{0} ({1} {2}, {3})", pawn.LabelShortCap, pawn.skills.GetSkill(bill.recipe.workSkill).Level, bill.recipe.workSkill.label, "NotAssigned".Translate()), delegate
					{
						bill.SetPawnRestriction(pawn);
					}),
					payload = pawn
				};
			}
			else if (!pawn.workSettings.WorkIsActive(workGiver.workType))
			{
				yield return new Widgets.DropdownMenuElement<Pawn>
				{
					option = new FloatMenuOption(string.Format("{0} ({1})", pawn.LabelShortCap, "NotAssigned".Translate()), delegate
					{
						bill.SetPawnRestriction(pawn);
					}),
					payload = pawn
				};
			}
			else if (bill.recipe.workSkill != null)
			{
				yield return new Widgets.DropdownMenuElement<Pawn>
				{
					option = new FloatMenuOption($"{pawn.LabelShortCap} ({pawn.skills.GetSkill(bill.recipe.workSkill).Level} {bill.recipe.workSkill.label})", delegate
					{
						bill.SetPawnRestriction(pawn);
					}),
					payload = pawn
				};
			}
			else
			{
				yield return new Widgets.DropdownMenuElement<Pawn>
				{
					option = new FloatMenuOption($"{pawn.LabelShortCap}", delegate
					{
						bill.SetPawnRestriction(pawn);
					}),
					payload = pawn
				};
			}
		}
	}
}
