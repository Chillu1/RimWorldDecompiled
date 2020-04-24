using Verse;

namespace RimWorld
{
	public class WorkGiver_Uninstall : WorkGiver_RemoveBuilding
	{
		protected override DesignationDef Designation => DesignationDefOf.Uninstall;

		protected override JobDef RemoveBuildingJob => JobDefOf.Uninstall;

		public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
		{
			if (t.def.Claimable)
			{
				if (t.Faction != pawn.Faction)
				{
					return false;
				}
			}
			else if (pawn.Faction != Faction.OfPlayer)
			{
				return false;
			}
			return base.HasJobOnThing(pawn, t, forced);
		}
	}
}
