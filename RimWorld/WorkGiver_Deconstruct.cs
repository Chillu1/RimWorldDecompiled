using Verse;

namespace RimWorld
{
	public class WorkGiver_Deconstruct : WorkGiver_RemoveBuilding
	{
		protected override DesignationDef Designation => DesignationDefOf.Deconstruct;

		protected override JobDef RemoveBuildingJob => JobDefOf.Deconstruct;

		public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
		{
			Building building = t.GetInnerIfMinified() as Building;
			if (building == null)
			{
				return false;
			}
			if (!building.DeconstructibleBy(pawn.Faction))
			{
				return false;
			}
			return base.HasJobOnThing(pawn, t, forced);
		}
	}
}
