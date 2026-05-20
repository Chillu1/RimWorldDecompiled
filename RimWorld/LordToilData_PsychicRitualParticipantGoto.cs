using System.Collections.Generic;
using Verse;
using Verse.AI.Group;

namespace RimWorld;

public class LordToilData_PsychicRitualParticipantGoto : LordToilData
{
	public List<PsychicRitualParticipant> participants;

	public JobSyncTracker finalGoto;

	public int ticksElapsed;

	public int timeoutTicks;

	public override void ExposeData()
	{
		Scribe_Deep.Look(ref finalGoto, "finalGoto");
		Scribe_Collections.Look(ref participants, "participants", LookMode.Deep);
		Scribe_Values.Look(ref ticksElapsed, "ticksElapsed", 0);
		Scribe_Values.Look(ref timeoutTicks, "timeoutTicks", 0);
	}
}
