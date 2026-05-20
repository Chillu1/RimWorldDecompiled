using Verse;

namespace RimWorld
{
	public class ThoughtWorker_TravelAnticipation : ThoughtWorker
	{
		private const int stage0Hours = 8;

		private const int stage1Hours = 16;

		protected override ThoughtState CurrentStateInternal(Pawn p)
		{
			if (!p.IsColonist || p.jobs == null || p.jobs.StartedFormingCaravanTick < 0)
			{
				return ThoughtState.Inactive;
			}
			int num = (Find.TickManager.TicksGame - p.jobs.StartedFormingCaravanTick) / 2500;
			if (num < 8)
			{
				return ThoughtState.Inactive;
			}
			if (num < 16)
			{
				return ThoughtState.ActiveAtStage(0);
			}
			return ThoughtState.ActiveAtStage(1);
		}
	}
}
