using System.Collections.Generic;
using System.Linq;

namespace Verse.AI
{
	public class ThinkNode_SubtreesByTag : ThinkNode
	{
		[NoTranslate]
		public string insertTag;

		[Unsaved(false)]
		private List<ThinkTreeDef> matchedTrees;

		public override ThinkNode DeepCopy(bool resolve = true)
		{
			ThinkNode_SubtreesByTag obj = (ThinkNode_SubtreesByTag)base.DeepCopy(resolve);
			obj.insertTag = insertTag;
			return obj;
		}

		protected override void ResolveSubnodes()
		{
		}

		public override ThinkResult TryIssueJobPackage(Pawn pawn, JobIssueParams jobParams)
		{
			if (matchedTrees == null)
			{
				matchedTrees = new List<ThinkTreeDef>();
				foreach (ThinkTreeDef allDef in DefDatabase<ThinkTreeDef>.AllDefs)
				{
					if (allDef.insertTag == insertTag)
					{
						matchedTrees.Add(allDef);
					}
				}
				matchedTrees = matchedTrees.OrderByDescending((ThinkTreeDef tDef) => tDef.insertPriority).ToList();
			}
			for (int i = 0; i < matchedTrees.Count; i++)
			{
				ThinkResult result = matchedTrees[i].thinkRoot.TryIssueJobPackage(pawn, jobParams);
				if (result.IsValid)
				{
					return result;
				}
			}
			return ThinkResult.NoJob;
		}
	}
}
