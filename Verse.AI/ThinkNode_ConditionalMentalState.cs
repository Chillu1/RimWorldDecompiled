namespace Verse.AI
{
	public class ThinkNode_ConditionalMentalState : ThinkNode_Conditional
	{
		public MentalStateDef state;

		public override ThinkNode DeepCopy(bool resolve = true)
		{
			ThinkNode_ConditionalMentalState obj = (ThinkNode_ConditionalMentalState)base.DeepCopy(resolve);
			obj.state = state;
			return obj;
		}

		protected override bool Satisfied(Pawn pawn)
		{
			return pawn.MentalStateDef == state;
		}
	}
}
