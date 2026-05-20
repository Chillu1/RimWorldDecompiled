using Verse;
using Verse.AI;

namespace RimWorld
{
	public class ThinkNode_Priority_Learn : ThinkNode_Priority
	{
		private const int GameStartNoLearningTicks = 5000;

		public override float GetPriority(Pawn pawn)
		{
			if (!ModsConfig.BiotechActive)
			{
				return 0f;
			}
			if (!(pawn.timetable?.CurrentAssignment ?? TimeAssignmentDefOf.Anything).allowJoy)
			{
				return 0f;
			}
			if (pawn.learning == null)
			{
				return 0f;
			}
			if (Find.TickManager.TicksGame < 5000)
			{
				return 0f;
			}
			if (!pawn.DevelopmentalStage.Child())
			{
				return 0f;
			}
			if (LearningUtility.LearningSatisfied(pawn))
			{
				return 0f;
			}
			return 9.1f;
		}
	}
}
