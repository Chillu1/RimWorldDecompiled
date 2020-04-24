namespace Verse.AI.Group
{
	public class Trigger_PawnCanReachMapEdge : Trigger
	{
		public override bool ActivateOn(Lord lord, TriggerSignal signal)
		{
			if (signal.type == TriggerSignalType.Tick && Find.TickManager.TicksGame % 193 == 0)
			{
				for (int i = 0; i < lord.ownedPawns.Count; i++)
				{
					Pawn pawn = lord.ownedPawns[i];
					if (pawn.Spawned && !pawn.Dead && !pawn.Downed && !pawn.CanReachMapEdge())
					{
						return false;
					}
				}
				return true;
			}
			return false;
		}
	}
}
