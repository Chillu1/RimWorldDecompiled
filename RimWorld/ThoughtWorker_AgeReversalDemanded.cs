using Verse;

namespace RimWorld
{
	public class ThoughtWorker_AgeReversalDemanded : ThoughtWorker_Precept
	{
		protected override ThoughtState ShouldHaveThought(Pawn p)
		{
			if (p.Faction != Faction.OfPlayer)
			{
				return ThoughtState.Inactive;
			}
			if (p.IsSlave)
			{
				return ThoughtState.Inactive;
			}
			if (p.ageTracker == null)
			{
				return ThoughtState.Inactive;
			}
			if (p.ageTracker.AgeBiologicalYears < 25)
			{
				return ThoughtState.Inactive;
			}
			long ageReversalDemandedDeadlineTicks = p.ageTracker.AgeReversalDemandedDeadlineTicks;
			if (ageReversalDemandedDeadlineTicks > 0)
			{
				return ThoughtState.ActiveAtStage(3);
			}
			long num = -ageReversalDemandedDeadlineTicks / 60000;
			int stageIndex = ((num > 15) ? ((num <= 30) ? 1 : 2) : 0);
			return ThoughtState.ActiveAtStage(stageIndex);
		}

		public static bool CanHaveThought(Pawn pawn)
		{
			if (!ModLister.CheckIdeology("Age Reversal"))
			{
				return false;
			}
			return ThoughtDefOf.AgeReversalDemanded.Worker.CurrentState(pawn).Active;
		}
	}
}
