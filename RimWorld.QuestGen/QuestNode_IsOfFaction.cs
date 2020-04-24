using Verse;

namespace RimWorld.QuestGen
{
	public class QuestNode_IsOfFaction : QuestNode
	{
		public SlateRef<Thing> thing;

		public SlateRef<Faction> faction;

		public QuestNode node;

		public QuestNode elseNode;

		protected override bool TestRunInt(Slate slate)
		{
			if (IsOfFaction(slate))
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
			if (IsOfFaction(QuestGen.slate))
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

		private bool IsOfFaction(Slate slate)
		{
			if (thing.GetValue(slate) != null)
			{
				return thing.GetValue(slate).Faction == faction.GetValue(slate);
			}
			return false;
		}
	}
}
