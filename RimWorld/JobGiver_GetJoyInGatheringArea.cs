using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobGiver_GetJoyInGatheringArea : JobGiver_GetJoy
	{
		public float desiredRadius = -1f;

		protected override Job TryGiveJobFromJoyGiverDefDirect(JoyGiverDef def, Pawn pawn)
		{
			if (pawn.mindState.duty == null)
			{
				return null;
			}
			if (pawn.needs.joy == null)
			{
				return null;
			}
			if (pawn.needs.joy.CurLevelPercentage > 0.92f)
			{
				return null;
			}
			IntVec3 cell = pawn.mindState.duty.focus.Cell;
			return def.Worker.TryGiveJobInGatheringArea(pawn, cell, desiredRadius);
		}

		protected override Job TryGiveJob(Pawn pawn)
		{
			if (pawn.needs.joy == null)
			{
				return null;
			}
			return base.TryGiveJob(pawn);
		}

		public override ThinkNode DeepCopy(bool resolve = true)
		{
			JobGiver_GetJoyInGatheringArea obj = (JobGiver_GetJoyInGatheringArea)base.DeepCopy(resolve);
			obj.desiredRadius = desiredRadius;
			return obj;
		}
	}
}
