namespace Verse.AI
{
	public class ThinkNode_ConditionalHasFallbackLocation : ThinkNode_Conditional
	{
		protected override bool Satisfied(Pawn pawn)
		{
			if (pawn.mindState.duty != null)
			{
				return pawn.mindState.duty.focusSecond.IsValid;
			}
			return false;
		}
	}
}
