using Verse;

namespace RimWorld.QuestGen
{
	public class QuestNode_IsFactionLeader : QuestNode
	{
		public SlateRef<Pawn> pawn;

		public QuestNode node;

		public QuestNode elseNode;

		protected override bool TestRunInt(Slate slate)
		{
			if (IsFactionLeader(slate))
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
			if (IsFactionLeader(QuestGen.slate))
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

		private bool IsFactionLeader(Slate slate)
		{
			if (pawn.GetValue(slate) != null && pawn.GetValue(slate).Faction != null)
			{
				return pawn.GetValue(slate).Faction.leader == pawn.GetValue(slate);
			}
			return false;
		}
	}
}
