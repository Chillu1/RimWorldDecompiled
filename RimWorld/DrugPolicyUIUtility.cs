using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public static class DrugPolicyUIUtility
	{
		public const string AssigningDrugsTutorHighlightTag = "ButtonAssignDrugs";

		public static void DoAssignDrugPolicyButtons(Rect rect, Pawn pawn)
		{
			int num = Mathf.FloorToInt((rect.width - 4f) * 0.714285731f);
			int num2 = Mathf.FloorToInt((rect.width - 4f) * 0.2857143f);
			float x = rect.x;
			Rect rect2 = new Rect(x, rect.y + 2f, num, rect.height - 4f);
			string text = pawn.drugs.CurrentPolicy.label;
			if (pawn.story != null && pawn.story.traits != null)
			{
				Trait trait = pawn.story.traits.GetTrait(TraitDefOf.DrugDesire);
				if (trait != null)
				{
					text = text + " (" + trait.Label + ")";
				}
			}
			Widgets.Dropdown(rect2, pawn, (Pawn p) => p.drugs.CurrentPolicy, Button_GenerateMenu, text.Truncate(rect2.width), null, pawn.drugs.CurrentPolicy.label, null, delegate
			{
				PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.DrugPolicies, KnowledgeAmount.Total);
			}, paintable: true);
			x += (float)num;
			x += 4f;
			Rect rect3 = new Rect(x, rect.y + 2f, num2, rect.height - 4f);
			if (Widgets.ButtonText(rect3, "AssignTabEdit".Translate()))
			{
				Find.WindowStack.Add(new Dialog_ManageDrugPolicies(pawn.drugs.CurrentPolicy));
			}
			UIHighlighter.HighlightOpportunity(rect2, "ButtonAssignDrugs");
			UIHighlighter.HighlightOpportunity(rect3, "ButtonAssignDrugs");
			x += (float)num2;
		}

		private static IEnumerable<Widgets.DropdownMenuElement<DrugPolicy>> Button_GenerateMenu(Pawn pawn)
		{
			foreach (DrugPolicy assignedDrugs in Current.Game.drugPolicyDatabase.AllPolicies)
			{
				yield return new Widgets.DropdownMenuElement<DrugPolicy>
				{
					option = new FloatMenuOption(assignedDrugs.label, delegate
					{
						pawn.drugs.CurrentPolicy = assignedDrugs;
					}),
					payload = assignedDrugs
				};
			}
		}
	}
}
