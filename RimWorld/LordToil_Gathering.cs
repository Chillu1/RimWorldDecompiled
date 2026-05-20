using System.Collections.Generic;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace RimWorld
{
	public abstract class LordToil_Gathering : LordToil
	{
		protected IntVec3 spot;

		protected GatheringDef gatheringDef;

		public LordToilData_Gathering Data => (LordToilData_Gathering)data;

		public LordToil_Gathering(IntVec3 spot, GatheringDef gatheringDef)
		{
			this.spot = spot;
			this.gatheringDef = gatheringDef;
			data = new LordToilData_Gathering();
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

		public override void LordToilTick()
		{
			List<Pawn> ownedPawns = lord.ownedPawns;
			for (int i = 0; i < ownedPawns.Count; i++)
			{
				if (GatheringsUtility.InGatheringArea(ownedPawns[i].Position, spot, base.Map))
				{
					if (!Data.presentForTicks.ContainsKey(ownedPawns[i]))
					{
						Data.presentForTicks.Add(ownedPawns[i], 0);
					}
					Data.presentForTicks[ownedPawns[i]]++;
				}
			}
		}
	}
}
