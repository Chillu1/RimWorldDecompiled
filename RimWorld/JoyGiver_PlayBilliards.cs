using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JoyGiver_PlayBilliards : JoyGiver_InteractBuilding
	{
		protected override bool CanDoDuringGathering => true;

		protected override Job TryGivePlayJob(Pawn pawn, Thing t)
		{
			if (!ThingHasStandableSpaceOnAllSides(t))
			{
				return null;
			}
			return JobMaker.MakeJob(def.jobDef, t);
		}

		public static bool ThingHasStandableSpaceOnAllSides(Thing t)
		{
			CellRect cellRect = t.OccupiedRect();
			foreach (IntVec3 item in cellRect.ExpandedBy(1))
			{
				if (!cellRect.Contains(item) && !item.Standable(t.Map))
				{
					return false;
				}
			}
			return true;
		}
	}
}
