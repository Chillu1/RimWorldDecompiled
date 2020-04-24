using Verse;

namespace RimWorld.QuestGen
{
	public class QuestNode_IsOfRoyalFaction : QuestNode
	{
		public SlateRef<Thing> thing;

		public QuestNode node;

		public QuestNode elseNode;

		protected override bool TestRunInt(Slate slate)
		{
			if (IsOfRoyalFaction(slate))
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
			if (IsOfRoyalFaction(QuestGen.slate))
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

		private bool IsOfRoyalFaction(Slate slate)
		{
			if (thing.GetValue(slate) != null && thing.GetValue(slate).Faction != null)
			{
				return thing.GetValue(slate).Faction.def.HasRoyalTitles;
			}
			return false;
		}
	}
}
