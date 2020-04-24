namespace Verse.AI.Group
{
	public class Trigger_PawnKilled : Trigger
	{
		public override bool ActivateOn(Lord lord, TriggerSignal signal)
		{
			if (signal.type == TriggerSignalType.PawnLost)
			{
				if (signal.condition == PawnLostCondition.IncappedOrKilled)
				{
					return signal.Pawn.Dead;
				}
				return false;
			}
			return false;
		}
	}
}
