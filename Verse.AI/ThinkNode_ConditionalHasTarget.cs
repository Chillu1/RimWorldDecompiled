namespace Verse.AI
{
	public class ThinkNode_ConditionalHasTarget : ThinkNode_Conditional
	{
		protected override bool Satisfied(Pawn pawn)
		{
			return pawn.mindState.enemyTarget != null;
		}
	}
}
