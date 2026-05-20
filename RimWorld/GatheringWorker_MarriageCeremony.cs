using Verse;
using Verse.AI.Group;

namespace RimWorld;

public class GatheringWorker_MarriageCeremony : GatheringWorker
{
	private static void FindFiancees(Pawn organizer, out Pawn firstFiance, out Pawn secondFiance)
	{
		firstFiance = organizer;
		secondFiance = organizer.relations.GetFirstDirectRelationPawn(PawnRelationDefOf.Fiance);
	}

	protected override LordJob CreateLordJob(IntVec3 spot, Pawn organizer)
	{
		FindFiancees(organizer, out var firstFiance, out var secondFiance);
		return new LordJob_Joinable_MarriageCeremony(firstFiance, secondFiance, spot);
	}

	protected override bool TryFindGatherSpot(Pawn organizer, out IntVec3 spot)
	{
		FindFiancees(organizer, out var firstFiance, out var secondFiance);
		return RCellFinder.TryFindMarriageSite(firstFiance, secondFiance, out spot);
	}

	protected override void SendLetter(IntVec3 spot, Pawn organizer)
	{
		FindFiancees(organizer, out var firstFiance, out var secondFiance);
		Messages.Message("MessageNewMarriageCeremony".Translate(firstFiance.LabelShort, secondFiance.LabelShort, firstFiance.Named("PAWN1"), secondFiance.Named("PAWN2")), new TargetInfo(spot, firstFiance.Map), MessageTypeDefOf.PositiveEvent);
	}

	public override bool CanExecute(Map map, Pawn organizer = null)
	{
		if (organizer != null)
		{
			FindFiancees(organizer, out var firstFiance, out var secondFiance);
			if (!GatheringsUtility.PawnCanStartOrContinueGathering(firstFiance) || !GatheringsUtility.PawnCanStartOrContinueGathering(secondFiance))
			{
				return false;
			}
		}
		return base.CanExecute(map, organizer);
	}
}
