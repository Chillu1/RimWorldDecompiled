using Verse;
using Verse.AI.Group;

namespace RimWorld;

public class LordToilData_EntitySwarm : LordToilData
{
	public IntVec3 pos;

	public IntVec3 dest;

	public int lastMoved;

	public override void ExposeData()
	{
		Scribe_Values.Look(ref pos, "pos");
		Scribe_Values.Look(ref dest, "dest");
		Scribe_Values.Look(ref lastMoved, "lastMoved", 0);
	}
}
