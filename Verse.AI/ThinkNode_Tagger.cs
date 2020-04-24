namespace Verse.AI
{
	public class ThinkNode_Tagger : ThinkNode_Priority
	{
		private JobTag tagToGive;

		public override ThinkNode DeepCopy(bool resolve = true)
		{
			ThinkNode_Tagger obj = (ThinkNode_Tagger)base.DeepCopy(resolve);
			obj.tagToGive = tagToGive;
			return obj;
		}

		public override float GetPriority(Pawn pawn)
		{
			if (priority >= 0f)
			{
				return priority;
			}
			if (subNodes.Any())
			{
				return subNodes[0].GetPriority(pawn);
			}
			Log.ErrorOnce("ThinkNode_PrioritySorter has child node which didn't give a priority: " + this, GetHashCode());
			return 0f;
		}

		public override ThinkResult TryIssueJobPackage(Pawn pawn, JobIssueParams jobParams)
		{
			ThinkResult result = base.TryIssueJobPackage(pawn, jobParams);
			if (result.IsValid && !result.Tag.HasValue)
			{
				result = new ThinkResult(result.Job, result.SourceNode, tagToGive);
			}
			return result;
		}
	}
}
