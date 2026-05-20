using Verse;
using Verse.AI;

namespace RimWorld;

public class JobGiver_AITrashDutyFocus : ThinkNode_JobGiver
{
	public override ThinkNode DeepCopy(bool resolve = true)
	{
		return (JobGiver_AITrashDutyFocus)base.DeepCopy(resolve);
	}

	protected override Job TryGiveJob(Pawn pawn)
	{
		if (pawn.mindState.duty == null || !pawn.mindState.duty.focus.IsValid)
		{
			return null;
		}
		LocalTargetInfo focus = pawn.mindState.duty.focus;
		if (!focus.Thing.Spawned || focus.ThingDestroyed || !pawn.HostileTo(focus.Thing) || !pawn.CanReach(focus, PathEndMode.Touch, Danger.Deadly))
		{
			return null;
		}
		Job job = TrashUtility.TrashJob(pawn, focus.Thing, allowPunchingInert: false, killIncappedTarget: true);
		if (job != null)
		{
			return job;
		}
		return null;
	}
}
