using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobGiver_ExtinguishSelf : ThinkNode_JobGiver
	{
		private const float ActivateChance = 0.1f;

		protected override Job TryGiveJob(Pawn pawn)
		{
			if (Rand.Value < 0.1f)
			{
				Fire fire = (Fire)pawn.GetAttachment(ThingDefOf.Fire);
				if (fire != null)
				{
					return JobMaker.MakeJob(JobDefOf.ExtinguishSelf, fire);
				}
			}
			return null;
		}
	}
}
