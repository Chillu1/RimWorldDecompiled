using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace RimWorld
{
	public class ThinkNode_ConditionalHasVoluntarilyJoinableLord : ThinkNode_Conditional
	{
		protected override bool Satisfied(Pawn pawn)
		{
			Lord lord = pawn.GetLord();
			if (lord != null)
			{
				return lord.LordJob is LordJob_VoluntarilyJoinable;
			}
			return false;
		}
	}
}
