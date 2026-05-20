using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobGiver_AIFollowOverseer : JobGiver_AIFollowPawn
	{
		protected override int FollowJobExpireInterval => 200;

		protected override Pawn GetFollowee(Pawn pawn)
		{
			return pawn.GetOverseer();
		}

		protected override float GetRadius(Pawn pawn)
		{
			return 5f;
		}

		protected override Job TryGiveJob(Pawn pawn)
		{
			if (pawn.GetOverseer() == null)
			{
				return null;
			}
			return base.TryGiveJob(pawn);
		}
	}
}
