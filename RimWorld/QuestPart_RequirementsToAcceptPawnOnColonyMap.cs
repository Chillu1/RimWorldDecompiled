using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class QuestPart_RequirementsToAcceptPawnOnColonyMap : QuestPart_RequirementsToAccept
{
	public Pawn pawn;

	public override IEnumerable<Dialog_InfoCard.Hyperlink> Hyperlinks
	{
		get
		{
			if (pawn != null)
			{
				yield return new Dialog_InfoCard.Hyperlink(pawn);
			}
		}
	}

	public override AcceptanceReport CanAccept()
	{
		if (pawn != null && pawn.Map != null && pawn.Map.IsPlayerHome)
		{
			return true;
		}
		return new AcceptanceReport("QuestPawnNotOnColonyMap".Translate(pawn.Named("PAWN")));
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_References.Look(ref pawn, "pawn");
	}
}
