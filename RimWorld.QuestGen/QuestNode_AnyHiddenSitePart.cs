using System.Collections.Generic;
using System.Linq;

namespace RimWorld.QuestGen
{
	public class QuestNode_AnyHiddenSitePart : QuestNode
	{
		public SlateRef<IEnumerable<SitePartDef>> sitePartDefs;

		public QuestNode node;

		public QuestNode elseNode;

		protected override bool TestRunInt(Slate slate)
		{
			if (AnyHiddenSitePart(slate))
			{
				if (node != null)
				{
					return node.TestRun(slate);
				}
				return true;
			}
			if (elseNode != null)
			{
				return elseNode.TestRun(slate);
			}
			return true;
		}

		protected override void RunInt()
		{
			if (AnyHiddenSitePart(QuestGen.slate))
			{
				if (node != null)
				{
					node.Run();
				}
			}
			else if (elseNode != null)
			{
				elseNode.Run();
			}
		}

		private bool AnyHiddenSitePart(Slate slate)
		{
			return sitePartDefs.GetValue(slate)?.Any((SitePartDef x) => x.defaultHidden) ?? false;
		}
	}
}
