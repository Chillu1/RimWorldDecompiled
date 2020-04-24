using Verse;

namespace RimWorld
{
	public class ThoughtWorker_Pain : ThoughtWorker
	{
		protected override ThoughtState CurrentStateInternal(Pawn p)
		{
			float painTotal = p.health.hediffSet.PainTotal;
			if (painTotal < 0.0001f)
			{
				return ThoughtState.Inactive;
			}
			if (painTotal < 0.15f)
			{
				return ThoughtState.ActiveAtStage(0);
			}
			if (painTotal < 0.4f)
			{
				return ThoughtState.ActiveAtStage(1);
			}
			if (painTotal < 0.8f)
			{
				return ThoughtState.ActiveAtStage(2);
			}
			return ThoughtState.ActiveAtStage(3);
		}
	}
}
