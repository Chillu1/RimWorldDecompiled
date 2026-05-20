using Verse;

namespace RimWorld
{
	public class JobGiver_AIJumpToJobRescueTarget : JobGiver_AIJumpToJobTarget
	{
		public override bool CanJumpToTarget(Pawn pawn, LocalTargetInfo target)
		{
			if (!base.CanJumpToTarget(pawn, target))
			{
				return false;
			}
			if (target.HasThing)
			{
				Thing thing = target.Thing;
				if (thing is Pawn)
				{
					return true;
				}
				if (thing is Building_Bed && pawn.carryTracker != null && pawn.carryTracker.CarriedThing is Pawn)
				{
					return true;
				}
			}
			return false;
		}
	}
}
