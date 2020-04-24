namespace Verse.AI
{
	public abstract class ThinkNode_Conditional : ThinkNode_Priority
	{
		public bool invert;

		public override ThinkNode DeepCopy(bool resolve = true)
		{
			ThinkNode_Conditional obj = (ThinkNode_Conditional)base.DeepCopy(resolve);
			obj.invert = invert;
			return obj;
		}

		public override ThinkResult TryIssueJobPackage(Pawn pawn, JobIssueParams jobParams)
		{
			if (Satisfied(pawn) == !invert)
			{
				return base.TryIssueJobPackage(pawn, jobParams);
			}
			return ThinkResult.NoJob;
		}

		protected abstract bool Satisfied(Pawn pawn);
	}
}
