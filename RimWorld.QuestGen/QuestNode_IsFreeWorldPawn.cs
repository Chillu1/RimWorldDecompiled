using RimWorld.Planet;
using Verse;

namespace RimWorld.QuestGen
{
	public class QuestNode_IsFreeWorldPawn : QuestNode
	{
		public SlateRef<Pawn> pawn;

		public QuestNode node;

		public QuestNode elseNode;

		protected override bool TestRunInt(Slate slate)
		{
			if (IsFreeWorldPawn(slate))
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
			if (IsFreeWorldPawn(QuestGen.slate))
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

		private bool IsFreeWorldPawn(Slate slate)
		{
			if (pawn.GetValue(slate) != null)
			{
				return Find.WorldPawns.GetSituation(pawn.GetValue(slate)) == WorldPawnSituation.Free;
			}
			return false;
		}
	}
}
