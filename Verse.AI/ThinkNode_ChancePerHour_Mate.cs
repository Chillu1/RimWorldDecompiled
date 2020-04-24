namespace Verse.AI
{
	public class ThinkNode_ChancePerHour_Mate : ThinkNode_ChancePerHour
	{
		protected override float MtbHours(Pawn pawn)
		{
			return pawn.RaceProps.mateMtbHours;
		}
	}
}
