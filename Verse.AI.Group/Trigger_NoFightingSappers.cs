using RimWorld;

namespace Verse.AI.Group
{
	public class Trigger_NoFightingSappers : Trigger
	{
		public override bool ActivateOn(Lord lord, TriggerSignal signal)
		{
			if (signal.type == TriggerSignalType.PawnLost)
			{
				for (int i = 0; i < lord.ownedPawns.Count; i++)
				{
					Pawn p = lord.ownedPawns[i];
					if (IsFightingSapper(p))
					{
						return false;
					}
				}
				return true;
			}
			return false;
		}

		private bool IsFightingSapper(Pawn p)
		{
			if (p.Downed || p.InMentalState)
			{
				return false;
			}
			if (!SappersUtility.IsGoodSapper(p))
			{
				return SappersUtility.IsGoodBackupSapper(p);
			}
			return true;
		}
	}
}
