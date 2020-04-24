namespace Verse.AI
{
	public class ThinkNode_ConditionalNoTarget : ThinkNode_Conditional
	{
		protected override bool Satisfied(Pawn pawn)
		{
			return pawn.mindState.enemyTarget == null;
		}
	}
}
