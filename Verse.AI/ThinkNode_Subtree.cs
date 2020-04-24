namespace Verse.AI
{
	public class ThinkNode_Subtree : ThinkNode
	{
		private ThinkTreeDef treeDef;

		[Unsaved(false)]
		public ThinkNode subtreeNode;

		public override ThinkNode DeepCopy(bool resolve = true)
		{
			ThinkNode_Subtree thinkNode_Subtree = (ThinkNode_Subtree)base.DeepCopy(resolve: false);
			thinkNode_Subtree.treeDef = treeDef;
			if (resolve)
			{
				thinkNode_Subtree.ResolveSubnodesAndRecur();
				thinkNode_Subtree.subtreeNode = thinkNode_Subtree.subNodes[subNodes.IndexOf(subtreeNode)];
			}
			return thinkNode_Subtree;
		}

		protected override void ResolveSubnodes()
		{
			subtreeNode = treeDef.thinkRoot.DeepCopy();
			subNodes.Add(subtreeNode);
		}

		public override ThinkResult TryIssueJobPackage(Pawn pawn, JobIssueParams jobParams)
		{
			return subtreeNode.TryIssueJobPackage(pawn, jobParams);
		}
	}
}
