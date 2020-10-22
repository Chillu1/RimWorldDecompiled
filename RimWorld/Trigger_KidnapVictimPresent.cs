using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace RimWorld
{
	public class Trigger_KidnapVictimPresent : Trigger
	{
		private const int CheckInterval = 120;

		private const int MinTicksSinceDamage = 300;

		private TriggerData_PawnCycleInd Data => (TriggerData_PawnCycleInd)data;

		public Trigger_KidnapVictimPresent()
		{
			data = new TriggerData_PawnCycleInd();
		}

		public override bool ActivateOn(Lord lord, TriggerSignal signal)
		{
			if (signal.type == TriggerSignalType.Tick && Find.TickManager.TicksGame % 120 == 0)
			{
				if (base.data == null || !(base.data is TriggerData_PawnCycleInd))
				{
					BackCompatibility.TriggerDataPawnCycleIndNull(this);
				}
				if (Find.TickManager.TicksGame - lord.lastPawnHarmTick > 300)
				{
					TriggerData_PawnCycleInd data = Data;
					data.pawnCycleInd++;
					if (data.pawnCycleInd >= lord.ownedPawns.Count)
					{
						data.pawnCycleInd = 0;
					}
					if (lord.ownedPawns.Any())
					{
						Pawn pawn = lord.ownedPawns[data.pawnCycleInd];
						if (pawn.Spawned && !pawn.Downed && pawn.MentalStateDef == null && KidnapAIUtility.TryFindGoodKidnapVictim(pawn, 8f, out var _) && !GenAI.InDangerousCombat(pawn))
						{
							return true;
						}
					}
				}
			}
			return false;
		}
	}
}
