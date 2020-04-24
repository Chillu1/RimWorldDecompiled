using System.Collections.Generic;

namespace Verse.AI
{
	public class ThinkNode_Random : ThinkNode
	{
		private static List<ThinkNode> tempList = new List<ThinkNode>();

		public override ThinkResult TryIssueJobPackage(Pawn pawn, JobIssueParams jobParams)
		{
			tempList.Clear();
			for (int i = 0; i < subNodes.Count; i++)
			{
				tempList.Add(subNodes[i]);
			}
			tempList.Shuffle();
			for (int j = 0; j < tempList.Count; j++)
			{
				ThinkResult result = tempList[j].TryIssueJobPackage(pawn, jobParams);
				if (result.IsValid)
				{
					return result;
				}
			}
			return ThinkResult.NoJob;
		}
	}
}
