using Verse;

namespace RimWorld
{
	public class ThoughtWorker_Precept_TreeDensity : ThoughtWorker_Precept
	{
		private static readonly int[] treeDestructionThresholds = new int[7] { 2, 5, 8, 10, 13, 16, 20 };

		protected override ThoughtState ShouldHaveThought(Pawn p)
		{
			if (!ModsConfig.IdeologyActive)
			{
				return ThoughtState.Inactive;
			}
			return ThoughtState.ActiveAtStage(ThoughtStageIndex(p));
		}

		private int ThoughtStageIndex(Pawn p)
		{
			int playerResponsibleTreeDestructionCount = p.Map.treeDestructionTracker.PlayerResponsibleTreeDestructionCount;
			for (int num = treeDestructionThresholds.Length - 1; num >= 0; num--)
			{
				if (playerResponsibleTreeDestructionCount >= treeDestructionThresholds[num])
				{
					return num + 1;
				}
			}
			return 0;
		}
	}
}
