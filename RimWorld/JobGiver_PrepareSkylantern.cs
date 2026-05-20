using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobGiver_PrepareSkylantern : JobGiver_GotoAndStandSociallyActive
	{
		public ThingDef def;

		public int count = 1;

		protected override Job TryGiveJob(Pawn pawn)
		{
			IntVec3 dest = GetDest(pawn);
			Job job = JobMaker.MakeJob(JobDefOf.PrepareSkylantern, dest);
			job.locomotionUrgency = locomotionUrgency;
			job.expiryInterval = expiryInterval;
			job.checkOverrideOnExpire = true;
			job.thingDefToCarry = def;
			job.count = count;
			return job;
		}

		public override ThinkNode DeepCopy(bool resolve = true)
		{
			JobGiver_PrepareSkylantern obj = (JobGiver_PrepareSkylantern)base.DeepCopy(resolve);
			obj.def = def;
			obj.count = count;
			return obj;
		}
	}
}
