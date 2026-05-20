using Verse;

namespace RimWorld;

public class WorkGiver_FillIn : WorkGiver_RemoveBuilding
{
	protected override DesignationDef Designation => DesignationDefOf.FillIn;

	protected override JobDef RemoveBuildingJob => JobDefOf.FillIn;
}
