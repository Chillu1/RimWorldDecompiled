using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld;

public static class DrugPolicyUIUtility
{
	public const string AssigningDrugsTutorHighlightTag = "ButtonAssignDrugs";

	public static void DoAssignDrugPolicyButtons(Rect rect, Pawn pawn)
	{
		if (pawn.drugs.CurrentPolicy == null)
		{
			return;
		}
		Rect rect2 = rect.ContractedBy(0f, 2f);
		string text = pawn.drugs.CurrentPolicy.label;
		if (pawn.story?.traits != null)
		{
			Trait trait = pawn.story.traits.GetTrait(TraitDefOf.DrugDesire);
			Gene_ChemicalDependency gene;
			if (trait != null)
			{
				text = text + " (" + trait.Label + ")";
			}
			else if (ModsConfig.BiotechActive && PawnUtility.TryGetChemicalDependencyGene(pawn, out gene))
			{
				text = text + " (" + gene.Label + ")";
			}
		}
		Widgets.Dropdown(rect2, pawn, (Pawn p) => p.drugs.CurrentPolicy, Button_GenerateMenu, text.Truncate(rect2.width), null, pawn.drugs.CurrentPolicy.label, null, delegate
		{
			PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.DrugPolicies, KnowledgeAmount.Total);
		}, paintable: true);
		UIHighlighter.HighlightOpportunity(rect2, "ButtonAssignDrugs");
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
		yield return new Widgets.DropdownMenuElement<DrugPolicy>
		{
			option = new FloatMenuOption(string.Format("{0}...", "AssignTabEdit".Translate()), delegate
			{
				Find.WindowStack.Add(new Dialog_ManageDrugPolicies(pawn.drugs.CurrentPolicy));
			})
		};
	}
}
