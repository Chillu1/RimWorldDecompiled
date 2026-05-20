using Verse;
using Verse.AI.Group;

namespace RimWorld
{
	public class Trigger_WoundedGuestPresent : Trigger
	{
		private const int CheckInterval = 800;

		private TriggerData_PawnCycleInd Data => (TriggerData_PawnCycleInd)data;

		public Trigger_WoundedGuestPresent()
		{
			data = new TriggerData_PawnCycleInd();
		}

		public override bool ActivateOn(Lord lord, TriggerSignal signal)
		{
			if (signal.type == TriggerSignalType.Tick && Find.TickManager.TicksGame % 800 == 0)
			{
				TriggerData_PawnCycleInd triggerData_PawnCycleInd = Data;
				triggerData_PawnCycleInd.pawnCycleInd++;
				if (triggerData_PawnCycleInd.pawnCycleInd >= lord.ownedPawns.Count)
				{
					triggerData_PawnCycleInd.pawnCycleInd = 0;
				}
				if (lord.ownedPawns.Any())
				{
					Pawn pawn = lord.ownedPawns[triggerData_PawnCycleInd.pawnCycleInd];
					if (pawn.Spawned && !pawn.Downed && !pawn.InMentalState && KidnapAIUtility.ReachableWoundedGuest(pawn) != null)
					{
						return true;
					}
				}
			}
			return false;
		}
	}
}
