namespace Verse.AI
{
	public class ThinkNode_ConditionalPlayerMech : ThinkNode_Conditional
	{
		protected override bool Satisfied(Pawn pawn)
		{
			return pawn.IsColonyMech;
		}
	}
}
