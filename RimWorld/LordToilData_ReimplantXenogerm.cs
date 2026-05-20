using Verse;
using Verse.AI.Group;

namespace RimWorld
{
	public class LordToilData_ReimplantXenogerm : LordToilData
	{
		public IntVec3 waitSpot;

		public Pawn target;

		public override void ExposeData()
		{
			Scribe_Values.Look(ref waitSpot, "waitSpot");
			Scribe_References.Look(ref target, "target");
		}
	}
}
