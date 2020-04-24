using Verse;

namespace RimWorld.QuestGen
{
	public class QuestNode_HasRoyalTitleInCurrentFaction : QuestNode
	{
		public SlateRef<Pawn> pawn;

		public QuestNode node;

		public QuestNode elseNode;

		protected override bool TestRunInt(Slate slate)
		{
			if (HasRoyalTitleInCurrentFaction(slate))
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
			if (HasRoyalTitleInCurrentFaction(QuestGen.slate))
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

		private bool HasRoyalTitleInCurrentFaction(Slate slate)
		{
			Pawn value = pawn.GetValue(slate);
			if (value != null && value.Faction != null && value.royalty != null)
			{
				return value.royalty.HasAnyTitleIn(value.Faction);
			}
			return false;
		}
	}
}
