using Verse;

namespace RimWorld.QuestGen
{
	public class QuestNode_IsPermanentEnemy : QuestNode
	{
		public SlateRef<Thing> thing;

		public QuestNode node;

		public QuestNode elseNode;

		protected override bool TestRunInt(Slate slate)
		{
			if (IsPermanentEnemy(slate))
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
			if (IsPermanentEnemy(QuestGen.slate))
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

		private bool IsPermanentEnemy(Slate slate)
		{
			Thing value = thing.GetValue(slate);
			if (value != null && value.Faction != null)
			{
				return value.Faction.def.permanentEnemy;
			}
			return false;
		}
	}
}
