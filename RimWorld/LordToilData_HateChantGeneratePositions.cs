using System.Collections.Generic;
using Verse;
using Verse.AI.Group;

namespace RimWorld;

public class LordToilData_HateChantGeneratePositions : LordToilData
{
	public List<PsychicRitualParticipant> chanters;

	public HashSet<IntVec3> foundPositions;

	public override void ExposeData()
	{
		Scribe_Collections.Look(ref chanters, "chanters", LookMode.Deep);
		Scribe_Collections.Look(ref foundPositions, "foundPositions", LookMode.Deep);
	}
}
