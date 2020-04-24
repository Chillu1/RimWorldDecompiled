using System;
using Verse;

namespace RimWorld.QuestGen
{
	public class QuestNode_ViolentQuestsAllowed : QuestNode
	{
		public QuestNode node;

		public QuestNode elseNode;

		protected override bool TestRunInt(Slate slate)
		{
			return DoWork(slate, (QuestNode n) => n.TestRun(slate));
		}

		protected override void RunInt()
		{
			DoWork(QuestGen.slate, delegate(QuestNode n)
			{
				n.Run();
				return true;
			});
		}

		private bool DoWork(Slate slate, Func<QuestNode, bool> func)
		{
			bool allowViolentQuests = Find.Storyteller.difficulty.allowViolentQuests;
			slate.Set("allowViolentQuests", allowViolentQuests);
			if (allowViolentQuests)
			{
				if (node != null)
				{
					return func(node);
				}
			}
			else if (elseNode != null)
			{
				return func(elseNode);
			}
			return true;
		}
	}
}
