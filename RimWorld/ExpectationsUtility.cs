using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld;

public static class ExpectationsUtility
{
	private static List<ExpectationDef> wealthExpectationsInOrder;

	private static List<ExpectationDef> roleExpectationsInOrder;

	public static void Reset()
	{
		wealthExpectationsInOrder = (from ed in DefDatabase<ExpectationDef>.AllDefs
			where ed.WealthTriggered
			orderby ed.order
			select ed).ToList();
		roleExpectationsInOrder = (from ed in DefDatabase<ExpectationDef>.AllDefs
			where ed.forRoles
			orderby ed.order
			select ed).ToList();
	}

	public static ExpectationDef CurrentExpectationFor(Pawn p)
	{
		if (Current.ProgramState != ProgramState.Playing)
		{
			return null;
		}
		if (p.Faction != Faction.OfPlayer && !p.IsPrisonerOfColony)
		{
			return ExpectationDefOf.ExtremelyLow;
		}
		if (p.MapHeld != null)
		{
			ExpectationDef expectationDef = CurrentExpectationFor(p.MapHeld);
			if (p.royalty != null && p.MapHeld.IsPlayerHome)
			{
				foreach (Faction item in Find.FactionManager.AllFactionsListForReading)
				{
					RoyalTitle currentTitleInFaction = p.royalty.GetCurrentTitleInFaction(item);
					if (currentTitleInFaction != null && currentTitleInFaction.conceited && currentTitleInFaction.def.minExpectation != null && currentTitleInFaction.def.minExpectation.order > expectationDef.order)
					{
						expectationDef = currentTitleInFaction.def.minExpectation;
					}
				}
			}
			if (ModsConfig.IdeologyActive)
			{
				Precept_Role precept_Role = p.Ideo?.GetRole(p);
				if (precept_Role != null && precept_Role.def.expectationsOffset != 0 && !MoveColonyUtility.TitleAndRoleRequirementsGracePeriodActive)
				{
					ExpectationDef expectationDef2 = ExpectationForOrder(Math.Max(expectationDef.order + precept_Role.def.expectationsOffset, 0), forRole: true);
					if (expectationDef2 != null)
					{
						expectationDef = expectationDef2;
					}
				}
			}
			return expectationDef;
		}
		return ExpectationDefOf.VeryLow;
	}

	public static ExpectationDef CurrentExpectationFor(Map m)
	{
		float wealthTotal = m.wealthWatcher.WealthTotal;
		for (int i = 0; i < wealthExpectationsInOrder.Count; i++)
		{
			ExpectationDef expectationDef = wealthExpectationsInOrder[i];
			if (wealthTotal < expectationDef.maxMapWealth)
			{
				return expectationDef;
			}
		}
		List<ExpectationDef> list = wealthExpectationsInOrder;
		return list[list.Count - 1];
	}

	public static ExpectationDef ExpectationForOrder(int order, bool forRole = false)
	{
		for (int i = 0; i < wealthExpectationsInOrder.Count; i++)
		{
			ExpectationDef expectationDef = wealthExpectationsInOrder[i];
			if (order == expectationDef.order)
			{
				return expectationDef;
			}
		}
		if (forRole)
		{
			for (int j = 0; j < roleExpectationsInOrder.Count; j++)
			{
				ExpectationDef expectationDef2 = roleExpectationsInOrder[j];
				if (order == expectationDef2.order)
				{
					return expectationDef2;
				}
			}
		}
		return null;
	}

	public static bool OffsetByRole(Pawn p)
	{
		if (ModsConfig.IdeologyActive && p.ideo != null)
		{
			Precept_Role role = p.Ideo.GetRole(p);
			if (role != null && role.def.expectationsOffset != 0)
			{
				return true;
			}
		}
		return false;
	}
}
