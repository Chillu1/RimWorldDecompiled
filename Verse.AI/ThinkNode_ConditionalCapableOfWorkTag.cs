namespace Verse.AI
{
	public class ThinkNode_ConditionalCapableOfWorkTag : ThinkNode_Conditional
	{
		public WorkTags workTags;

		public override ThinkNode DeepCopy(bool resolve = true)
		{
			ThinkNode_ConditionalCapableOfWorkTag obj = (ThinkNode_ConditionalCapableOfWorkTag)base.DeepCopy(resolve);
			obj.workTags = workTags;
			return obj;
		}

		protected override bool Satisfied(Pawn pawn)
		{
			return !pawn.WorkTagIsDisabled(workTags);
		}
	}
}
