using Verse;
using Verse.AI;

namespace RimWorld
{
	public class ThinkNode_ConditionalPackAnimalHasColonistToFollowWhilePacking : ThinkNode_Conditional
	{
		protected override bool Satisfied(Pawn pawn)
		{
			return JobGiver_PackAnimalFollowColonists.GetPawnToFollow(pawn) != null;
		}
	}
}
