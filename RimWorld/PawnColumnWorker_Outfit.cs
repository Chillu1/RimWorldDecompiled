using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimWorld;

public class PawnColumnWorker_Outfit : PawnColumnWorker
{
	public const int TopAreaHeight = 65;

	public const int ManageOutfitsButtonHeight = 32;

	public override void DoHeader(Rect rect, PawnTable table)
	{
		base.DoHeader(rect, table);
		MouseoverSounds.DoRegion(rect);
		Rect rect2 = new Rect(rect.x, rect.y + (rect.height - 65f), Mathf.Min(rect.width, 360f), 32f);
		if (Widgets.ButtonText(rect2, "ManageApparelPolicies".Translate()))
		{
			Find.WindowStack.Add(new Dialog_ManageApparelPolicies(null));
			PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.Outfits, KnowledgeAmount.Total);
		}
		UIHighlighter.HighlightOpportunity(rect2, "ManageOutfits");
	}

	public override void DoCell(Rect rect, Pawn pawn, PawnTable table)
	{
		if (pawn.outfits == null)
		{
			return;
		}
		Rect rect2 = rect.ContractedBy(0f, 2f);
		bool somethingIsForced = pawn.outfits.forcedHandler.SomethingIsForced;
		Rect left = rect2;
		Rect right = default(Rect);
		if (somethingIsForced)
		{
			rect2.SplitVerticallyWithMargin(out left, out right, 4f);
		}
		if (pawn.IsQuestLodger())
		{
			Text.Anchor = TextAnchor.MiddleCenter;
			Widgets.Label(left, "Unchangeable".Translate().Truncate(left.width));
			TooltipHandler.TipRegionByKey(left, "QuestRelated_Outfit");
			Text.Anchor = TextAnchor.UpperLeft;
		}
		else
		{
			Widgets.Dropdown(left, pawn, (Pawn p) => p.outfits.CurrentApparelPolicy, Button_GenerateMenu, pawn.outfits.CurrentApparelPolicy.label.Truncate(left.width), null, pawn.outfits.CurrentApparelPolicy.label, null, null, paintable: true);
		}
		if (!somethingIsForced)
		{
			return;
		}
		if (Widgets.ButtonText(right, "ClearForcedApparel".Translate()))
		{
			pawn.outfits.forcedHandler.Reset();
		}
		if (!Mouse.IsOver(right))
		{
			return;
		}
		TooltipHandler.TipRegion(right, new TipSignal(delegate
		{
			string text = "ForcedApparel".Translate() + ":\n";
			foreach (Apparel item in pawn.outfits.forcedHandler.ForcedApparel)
			{
				text = text + "\n   " + item.LabelCap;
			}
			return text;
		}, pawn.GetHashCode() * 612));
	}

	private IEnumerable<Widgets.DropdownMenuElement<ApparelPolicy>> Button_GenerateMenu(Pawn pawn)
	{
		foreach (ApparelPolicy outfit in Current.Game.outfitDatabase.AllOutfits)
		{
			yield return new Widgets.DropdownMenuElement<ApparelPolicy>
			{
				option = new FloatMenuOption(outfit.label, delegate
				{
					pawn.outfits.CurrentApparelPolicy = outfit;
				}),
				payload = outfit
			};
		}
		yield return new Widgets.DropdownMenuElement<ApparelPolicy>
		{
			option = new FloatMenuOption(string.Format("{0}...", "AssignTabEdit".Translate()), delegate
			{
				Find.WindowStack.Add(new Dialog_ManageApparelPolicies(pawn.outfits.CurrentApparelPolicy));
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
		if (pawn.outfits != null && pawn.outfits.CurrentApparelPolicy != null)
		{
			return pawn.outfits.CurrentApparelPolicy.id;
		}
		return int.MinValue;
	}
}
