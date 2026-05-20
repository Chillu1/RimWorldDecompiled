using Verse;

namespace RimWorld
{
	public class LifeStageWorker
	{
		public LifeStageDef def;

		public virtual void Notify_LifeStageStarted(Pawn pawn, LifeStageDef previousLifeStage)
		{
			pawn.health.capacities.Notify_CapacityLevelsDirty();
		}
	}
}
