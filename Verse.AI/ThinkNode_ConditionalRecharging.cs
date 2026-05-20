using RimWorld;

namespace Verse.AI
{
	public class ThinkNode_ConditionalRecharging : ThinkNode_Conditional
	{
		protected override bool Satisfied(Pawn pawn)
		{
			return pawn.IsCharging();
		}
	}
}
