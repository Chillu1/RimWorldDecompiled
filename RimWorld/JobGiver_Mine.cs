using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld;

public class JobGiver_Mine : ThinkNode_JobGiver
{
	protected override Job TryGiveJob(Pawn pawn)
	{
		IEnumerable<Thing> enumerable = MineAIUtility.PotentialMineables(pawn);
		Thing thing = GenClosest.ClosestThingReachable(pawn.Position, pawn.Map, ThingRequest.ForGroup(ThingRequestGroup.Undefined), PathEndMode.Touch, TraverseParms.For(pawn, pawn.NormalMaxDanger()), 9999f, Validator, enumerable, 0, -1, enumerable != null);
		if (thing == null)
		{
			return null;
		}
		return MineAIUtility.JobOnThing(pawn, thing);
		bool Validator(Thing t)
		{
			if (!t.IsForbidden(pawn))
			{
				return MineAIUtility.JobOnThing(pawn, t) != null;
			}
			return false;
		}
	}
}
