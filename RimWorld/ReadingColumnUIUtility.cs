using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld;

public static class ReadingColumnUIUtility
{
	public const string ReadingAssigningTutorHighlightTag = "ReadingAssignPolicy";

	public static void DoAssignReadingButtons(Rect rect, Pawn pawn)
	{
		Rect rect2 = rect.ContractedBy(0f, 2f);
		string label = pawn.reading.CurrentPolicy.label;
		Widgets.Dropdown(rect2, pawn, (Pawn p) => p.reading.CurrentPolicy, Button_GenerateMenu, label.Truncate(rect2.width), null, pawn.reading.CurrentPolicy.label, null, null, paintable: true);
		UIHighlighter.HighlightOpportunity(rect2, "ReadingAssignPolicy");
	}

	private static IEnumerable<Widgets.DropdownMenuElement<ReadingPolicy>> Button_GenerateMenu(Pawn pawn)
	{
		foreach (ReadingPolicy policy in Current.Game.readingPolicyDatabase.AllReadingPolicies)
		{
			yield return new Widgets.DropdownMenuElement<ReadingPolicy>
			{
				option = new FloatMenuOption(policy.label, delegate
				{
					pawn.reading.CurrentPolicy = policy;
				}),
				payload = policy
			};
		}
		yield return new Widgets.DropdownMenuElement<ReadingPolicy>
		{
			option = new FloatMenuOption(string.Format("{0}...", "AssignTabEdit".Translate()), delegate
			{
				Find.WindowStack.Add(new Dialog_ManageReadingPolicies(pawn.reading.CurrentPolicy));
			})
		};
	}
}
