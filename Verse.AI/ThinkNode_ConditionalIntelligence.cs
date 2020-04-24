namespace Verse.AI
{
	public class ThinkNode_ConditionalIntelligence : ThinkNode_Conditional
	{
		public Intelligence minIntelligence = Intelligence.ToolUser;

		public override ThinkNode DeepCopy(bool resolve = true)
		{
			ThinkNode_ConditionalIntelligence obj = (ThinkNode_ConditionalIntelligence)base.DeepCopy(resolve);
			obj.minIntelligence = minIntelligence;
			return obj;
		}

		protected override bool Satisfied(Pawn pawn)
		{
			return (int)pawn.RaceProps.intelligence >= (int)minIntelligence;
		}
	}
}
