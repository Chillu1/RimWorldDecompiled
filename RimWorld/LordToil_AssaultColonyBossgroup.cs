using Verse.AI;
using Verse.AI.Group;

namespace RimWorld
{
	public class LordToil_AssaultColonyBossgroup : LordToil
	{
		public override bool AllowSatisfyLongNeeds => false;

		public override bool ForceHighStoryDanger => true;

		public override void UpdateAllDuties()
		{
			for (int i = 0; i < lord.ownedPawns.Count; i++)
			{
				if (lord.ownedPawns[i].RaceProps.dutyBoss != null)
				{
					lord.ownedPawns[i].mindState.duty = new PawnDuty(lord.ownedPawns[i].RaceProps.dutyBoss);
				}
				else
				{
					lord.ownedPawns[i].mindState.duty = new PawnDuty(DutyDefOf.AssaultColony);
				}
			}
		}
	}
}
