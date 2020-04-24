using Verse;
using Verse.AI;

namespace RimWorld
{
	public class WorkGiver_ConstructSmoothFloor : WorkGiver_ConstructAffectFloor
	{
		protected override DesignationDef DesDef => DesignationDefOf.SmoothFloor;

		public override Job JobOnCell(Pawn pawn, IntVec3 c, bool forced = false)
		{
			return JobMaker.MakeJob(JobDefOf.SmoothFloor, c);
		}
	}
}
