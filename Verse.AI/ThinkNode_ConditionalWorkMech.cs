namespace Verse.AI
{
	public class ThinkNode_ConditionalWorkMech : ThinkNode_Conditional
	{
		protected override bool Satisfied(Pawn pawn)
		{
			return pawn.RaceProps.IsWorkMech;
		}
	}
}
