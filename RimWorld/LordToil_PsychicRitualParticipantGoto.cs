using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace RimWorld;

public class LordToil_PsychicRitualParticipantGoto : LordToil
{
	private const string FinalSyncTag = "final";

	protected LordToilData_PsychicRitualParticipantGoto Data => (LordToilData_PsychicRitualParticipantGoto)data;

	public bool AnyPawnsArrived => Data.finalGoto.AnyPawnsDone;

	public bool AllPawnsArrived => Data.finalGoto.AllPawnsDone;

	public bool TimedOut
	{
		get
		{
			if (Data.timeoutTicks != -1)
			{
				return Data.ticksElapsed >= Data.timeoutTicks;
			}
			return false;
		}
	}

	public virtual IEnumerable<PsychicRitualParticipant> Participants => Data.participants;

	public LordToil_PsychicRitualParticipantGoto(int timeoutTicks = -1)
	{
		data = new LordToilData_PsychicRitualParticipantGoto();
		Data.timeoutTicks = timeoutTicks;
	}

	public LordToil_PsychicRitualParticipantGoto(IEnumerable<PsychicRitualParticipant> participants)
	{
		data = new LordToilData_PsychicRitualParticipantGoto();
		SetParticipants(participants);
	}

	public override void UpdateAllDuties()
	{
		if (Data.participants.Count == 0)
		{
			return;
		}
		foreach (PsychicRitualParticipant participant in Participants)
		{
			participant.Deconstruct(out var pawn, out var location);
			Pawn pawn2 = pawn;
			IntVec3 intVec = location;
			DutyDef goto_NoZeroLengthPaths = DutyDefOf.Goto_NoZeroLengthPaths;
			pawn2.mindState.duty = new PawnDuty(goto_NoZeroLengthPaths, intVec)
			{
				tag = "final"
			};
		}
	}

	public override void LordToilTick()
	{
		Data.ticksElapsed++;
	}

	public override void Notify_PawnJobDone(Pawn pawn, JobCondition condition)
	{
		Data?.finalGoto?.Notify_PawnJobDone(pawn, pawn.CurJob, condition);
		if (Data?.finalGoto != null && !Data.finalGoto.WaitingOnPawn(pawn) && pawn.mindState?.duty != null)
		{
			pawn.mindState.duty.def = DutyDefOf.Idle;
		}
	}

	public override void Notify_PawnLost(Pawn victim, PawnLostCondition cond)
	{
		Data.finalGoto.Notify_PawnLost(victim);
	}

	public void SetParticipants(IEnumerable<PsychicRitualParticipant> participants)
	{
		PsychicRitualParticipant[] array = (participants as PsychicRitualParticipant[]) ?? participants.ToArray();
		Data.participants = new List<PsychicRitualParticipant>(array);
		Data.finalGoto = new JobSyncTracker(array.Select((PsychicRitualParticipant participant) => participant.pawn), null, "final");
	}
}
