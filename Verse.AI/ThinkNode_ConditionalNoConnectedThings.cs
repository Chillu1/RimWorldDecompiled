namespace Verse.AI
{
	public class ThinkNode_ConditionalNoConnectedThings : ThinkNode_Conditional
	{
		protected override bool Satisfied(Pawn pawn)
		{
			if (pawn.connections != null)
			{
				return !pawn.connections.ConnectedThings.Any();
			}
			return true;
		}
	}
}
