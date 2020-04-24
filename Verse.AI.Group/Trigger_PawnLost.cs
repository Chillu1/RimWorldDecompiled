namespace Verse.AI.Group
{
	public class Trigger_PawnLost : Trigger
	{
		private Pawn pawn;

		private PawnLostCondition condition;

		public Trigger_PawnLost(PawnLostCondition condition = PawnLostCondition.Undefined, Pawn pawn = null)
		{
			this.condition = condition;
			this.pawn = pawn;
		}

		public override bool ActivateOn(Lord lord, TriggerSignal signal)
		{
			if (signal.type == TriggerSignalType.PawnLost && (condition == PawnLostCondition.Undefined || signal.condition == condition))
			{
				if (pawn != null)
				{
					return pawn == signal.Pawn;
				}
				return true;
			}
			return false;
		}
	}
}
