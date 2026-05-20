using Verse;
using Verse.AI.Group;

namespace RimWorld;

public abstract class GatheringWorker
{
	public GatheringDef def;

	public virtual bool CanExecute(Map map, Pawn organizer = null)
	{
		if (organizer == null)
		{
			organizer = FindOrganizer(map);
		}
		if (organizer == null)
		{
			return false;
		}
		if (!TryFindGatherSpot(organizer, out var _))
		{
			return false;
		}
		if (!GatheringsUtility.PawnCanStartOrContinueGathering(organizer))
		{
			return false;
		}
		return true;
	}

	public virtual bool TryExecute(Map map, Pawn organizer = null)
	{
		if (organizer == null)
		{
			organizer = FindOrganizer(map);
		}
		if (organizer == null)
		{
			return false;
		}
		if (!TryFindGatherSpot(organizer, out var spot))
		{
			return false;
		}
		LordJob lordJob = CreateLordJob(spot, organizer);
		LordMaker.MakeNewLord(organizer.Faction, lordJob, organizer.Map, (!lordJob.OrganizerIsStartingPawn) ? null : new Pawn[1] { organizer });
		SendLetter(spot, organizer);
		return true;
	}

	protected virtual void SendLetter(IntVec3 spot, Pawn organizer)
	{
		Find.LetterStack.ReceiveLetter(def.letterTitle, def.letterText.Formatted(organizer.Named("ORGANIZER")), LetterDefOf.PositiveEvent, new TargetInfo(spot, organizer.Map));
	}

	protected virtual Pawn FindOrganizer(Map map)
	{
		return GatheringsUtility.FindRandomGatheringOrganizer(Faction.OfPlayer, map, def);
	}

	protected abstract bool TryFindGatherSpot(Pawn organizer, out IntVec3 spot);

	protected abstract LordJob CreateLordJob(IntVec3 spot, Pawn organizer);
}
