using Verse;

namespace RimWorld.QuestGen
{
	public class QuestNode_IsFactionHostileToPlayer : QuestNode
	{
		public SlateRef<Faction> faction;

		public SlateRef<Thing> factionOf;

		public QuestNode node;

		public QuestNode elseNode;

		protected override bool TestRunInt(Slate slate)
		{
			if (IsHostile(slate))
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
			if (IsHostile(QuestGen.slate))
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

		private bool IsHostile(Slate slate)
		{
			Faction value = faction.GetValue(slate);
			if (value != null)
			{
				return value.HostileTo(Faction.OfPlayer);
			}
			Thing value2 = factionOf.GetValue(slate);
			if (value2 != null && value2.Faction != null)
			{
				return value2.Faction.HostileTo(Faction.OfPlayer);
			}
			return false;
		}
	}
}
