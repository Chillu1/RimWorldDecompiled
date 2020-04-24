using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace RimWorld
{
	public class LordToil_HuntEnemies : LordToil
	{
		private LordToilData_HuntEnemies Data => (LordToilData_HuntEnemies)data;

		public override bool ForceHighStoryDanger => true;

		public LordToil_HuntEnemies(IntVec3 fallbackLocation)
		{
			data = new LordToilData_HuntEnemies();
			Data.fallbackLocation = fallbackLocation;
		}

		public override void UpdateAllDuties()
		{
			LordToilData_HuntEnemies data = Data;
			if (!data.fallbackLocation.IsValid)
			{
				for (int i = 0; i < lord.ownedPawns.Count; i++)
				{
					Pawn pawn = lord.ownedPawns[i];
					if (pawn.Spawned && RCellFinder.TryFindRandomSpotJustOutsideColony(pawn, out data.fallbackLocation) && data.fallbackLocation.IsValid)
					{
						break;
					}
				}
			}
			for (int j = 0; j < lord.ownedPawns.Count; j++)
			{
				Pawn pawn2 = lord.ownedPawns[j];
				pawn2.mindState.duty = new PawnDuty(DutyDefOf.HuntEnemiesIndividual);
				pawn2.mindState.duty.focusSecond = data.fallbackLocation;
			}
		}
	}
}
