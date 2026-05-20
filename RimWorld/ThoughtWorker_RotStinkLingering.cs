using Verse;

namespace RimWorld
{
	public class ThoughtWorker_RotStinkLingering : ThoughtWorker
	{
		private const int LingerDuration = 1800;

		protected override ThoughtState CurrentStateInternal(Pawn p)
		{
			if (!p.Spawned || p.mindState == null)
			{
				return ThoughtState.Inactive;
			}
			if (p.Map.gasGrid.DensityAt(p.Position, GasType.RotStink) > 0)
			{
				return ThoughtState.Inactive;
			}
			if (Find.TickManager.TicksGame > p.mindState.lastRotStinkTick + 1800)
			{
				return ThoughtState.Inactive;
			}
			return ThoughtState.ActiveAtStage(0);
		}
	}
}
