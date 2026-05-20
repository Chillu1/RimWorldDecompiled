using Verse;
using Verse.AI;

namespace RimWorld
{
	public class ThinkNode_ConditionalCanDoLordJobNow : ThinkNode_Conditional
	{
		protected override bool Satisfied(Pawn pawn)
		{
			if (pawn.IsBurning())
			{
				return false;
			}
			if (pawn.InMentalState)
			{
				return false;
			}
			if (pawn.Drafted)
			{
				return false;
			}
			if (!pawn.Awake())
			{
				return false;
			}
			if (pawn.Downed && !pawn.DutyActiveWhenDown())
			{
				return false;
			}
			return true;
		}
	}
}
