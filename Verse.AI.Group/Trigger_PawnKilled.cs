using System.Collections.Generic;

namespace Verse.AI.Group
{
	public class Trigger_PawnKilled : Trigger
	{
		private List<Pawn> exceptions;

		public Trigger_PawnKilled()
		{
		}

		public Trigger_PawnKilled(List<Pawn> exceptions)
		{
			this.exceptions = exceptions;
		}

		public override bool ActivateOn(Lord lord, TriggerSignal signal)
		{
			if (signal.type == TriggerSignalType.PawnLost)
			{
				if ((signal.condition == PawnLostCondition.Killed || signal.condition == PawnLostCondition.Incapped) && signal.Pawn.Dead)
				{
					if (!exceptions.NullOrEmpty())
					{
						return !exceptions.Contains(signal.Pawn);
					}
					return true;
				}
				return false;
			}
			return false;
		}
	}
}
