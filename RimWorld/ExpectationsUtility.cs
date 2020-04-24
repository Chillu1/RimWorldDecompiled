using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld
{
	public static class ExpectationsUtility
	{
		private static List<ExpectationDef> wealthExpectationsInOrder;

		public static void Reset()
		{
			wealthExpectationsInOrder = (from ed in DefDatabase<ExpectationDef>.AllDefs
				where ed.WealthTriggered
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
					return expectationDef;
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
			return wealthExpectationsInOrder[wealthExpectationsInOrder.Count - 1];
		}
	}
}
