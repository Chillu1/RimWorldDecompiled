namespace Verse.AI
{
	public class ThinkNode_FilterPriority : ThinkNode
	{
		public float minPriority = 0.5f;

		public override ThinkNode DeepCopy(bool resolve = true)
		{
			ThinkNode_FilterPriority obj = (ThinkNode_FilterPriority)base.DeepCopy(resolve);
			obj.minPriority = minPriority;
			return obj;
		}

		public override ThinkResult TryIssueJobPackage(Pawn pawn, JobIssueParams jobParams)
		{
			int count = subNodes.Count;
			for (int i = 0; i < count; i++)
			{
				if (subNodes[i].GetPriority(pawn) > minPriority)
				{
					ThinkResult result = subNodes[i].TryIssueJobPackage(pawn, jobParams);
					if (result.IsValid)
					{
						return result;
					}
				}
			}
			return ThinkResult.NoJob;
		}
	}
}
