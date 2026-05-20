using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class QuestPart_RequirementsToAcceptThroneRoom : QuestPart_RequirementsToAccept
{
	public RoyalTitleDef forTitle;

	public Faction faction;

	public Pawn forPawn;

	private List<string> acceptanceReportUnmetRequirements = new List<string>();

	public override IEnumerable<Dialog_InfoCard.Hyperlink> Hyperlinks
	{
		get
		{
			yield return new Dialog_InfoCard.Hyperlink(forTitle, faction);
		}
	}

	public override AcceptanceReport CanAccept()
	{
		acceptanceReportUnmetRequirements.Clear();
		if (forTitle.throneRoomRequirements.NullOrEmpty())
		{
			return true;
		}
		Building_Throne assignedThrone = forPawn.ownership.AssignedThrone;
		if (assignedThrone == null)
		{
			return "QuestNoThroneRoom".Translate(forPawn.Named("PAWN"));
		}
		foreach (RoomRequirement throneRoomRequirement in forTitle.throneRoomRequirements)
		{
			if (!throneRoomRequirement.MetOrDisabled(assignedThrone.GetRoom(), forPawn))
			{
				acceptanceReportUnmetRequirements.Add(throneRoomRequirement.LabelCap(assignedThrone.GetRoom()));
			}
		}
		if (acceptanceReportUnmetRequirements.Count != 0)
		{
			return new AcceptanceReport("QuestThroneRoomRequirementsUnsatisfied".Translate(forPawn.Named("PAWN"), forTitle.GetLabelFor(forPawn).Named("TITLE")) + ":\n\n" + acceptanceReportUnmetRequirements.ToLineList("- "));
		}
		return true;
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Defs.Look(ref forTitle, "forTitle");
		Scribe_References.Look(ref faction, "faction");
		Scribe_References.Look(ref forPawn, "forPawn");
	}

	public override void ReplacePawnReferences(Pawn replace, Pawn with)
	{
		if (forPawn == replace)
		{
			forPawn = with;
		}
	}
}
