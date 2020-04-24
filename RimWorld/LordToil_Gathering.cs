using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace RimWorld
{
	public abstract class LordToil_Gathering : LordToil
	{
		protected IntVec3 spot;

		protected GatheringDef gatheringDef;

		public LordToil_Gathering(IntVec3 spot, GatheringDef gatheringDef)
		{
			this.spot = spot;
			this.gatheringDef = gatheringDef;
		}

		public override ThinkTreeDutyHook VoluntaryJoinDutyHookFor(Pawn p)
		{
			return gatheringDef.duty.hook;
		}

		public override void UpdateAllDuties()
		{
			for (int i = 0; i < lord.ownedPawns.Count; i++)
			{
				lord.ownedPawns[i].mindState.duty = new PawnDuty(gatheringDef.duty, spot);
			}
		}
	}
}
