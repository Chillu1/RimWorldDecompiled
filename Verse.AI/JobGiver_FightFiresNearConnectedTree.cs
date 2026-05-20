using RimWorld;

namespace Verse.AI
{
	public class JobGiver_FightFiresNearConnectedTree : ThinkNode_JobGiver
	{
		private const float FightFireDistance = 10f;

		protected override Job TryGiveJob(Pawn pawn)
		{
			Thing thing = pawn.connections.ConnectedThings.FirstOrDefault((Thing x) => x.Spawned && x.Map == pawn.Map && pawn.CanReach(x, PathEndMode.Touch, Danger.Deadly));
			if (thing == null)
			{
				return null;
			}
			Thing thing2 = GenClosest.ClosestThingReachable(thing.Position, pawn.Map, ThingRequest.ForDef(ThingDefOf.Fire), PathEndMode.Touch, TraverseParms.For(pawn), 10f, JobGiver_FightFiresNearPoint.FireValidator(pawn));
			if (thing2 != null)
			{
				return JobMaker.MakeJob(JobDefOf.BeatFire, thing2);
			}
			return null;
		}
	}
}
