using Verse;

namespace RimWorld
{
	public class ThoughtWorker_NeedNeuralSupercharge : ThoughtWorker_Precept
	{
		public const int TicksUntilNeed = 30000;

		protected override ThoughtState ShouldHaveThought(Pawn p)
		{
			if (p.health == null)
			{
				return false;
			}
			if (!ResearchProjectDefOf.MicroelectronicsBasics.IsFinished)
			{
				return false;
			}
			int lastReceivedNeuralSuperchargeTick = p.health.lastReceivedNeuralSuperchargeTick;
			return Find.TickManager.TicksGame - lastReceivedNeuralSuperchargeTick >= 30000 || lastReceivedNeuralSuperchargeTick == -1;
		}
	}
}
