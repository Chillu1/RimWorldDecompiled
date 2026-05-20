using System.Collections.Generic;
using Verse;
using Verse.AI.Group;

namespace RimWorld;

public class LordToilData_HateChant : LordToilData
{
	public List<PsychicRitualParticipant> chanters;

	public GameCondition_HateChantDrone condition;

	public int lastDroneUpdate;

	public override void ExposeData()
	{
		Scribe_References.Look(ref condition, "condition");
		Scribe_Values.Look(ref lastDroneUpdate, "lastDroneUpdate", 0);
		Scribe_Collections.Look(ref chanters, "chanters", LookMode.Deep);
	}
}
