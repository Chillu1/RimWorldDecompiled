using Verse;

namespace RimWorld
{
	public class WorkGiver_ExtractTree : WorkGiver_RemoveBuilding
	{
		protected override DesignationDef Designation => DesignationDefOf.ExtractTree;

		protected override JobDef RemoveBuildingJob => JobDefOf.ExtractTree;
	}
}
