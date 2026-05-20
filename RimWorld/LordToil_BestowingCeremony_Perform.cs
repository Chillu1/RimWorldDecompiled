using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class LordToil_BestowingCeremony_Perform : LordToil_Wait
	{
		public Pawn target;

		public Pawn bestower;

		public LordToilData_Gathering Data => (LordToilData_Gathering)data;

		public LordToil_BestowingCeremony_Perform(Pawn target, Pawn bestower)
		{
			this.target = target;
			this.bestower = bestower;
			data = new LordToilData_Gathering();
		}

		public override void Init()
		{
			base.Init();
			if (!target.Awake())
			{
				RestUtility.WakeUp(target);
			}
		}

		public override void LordToilTick()
		{
			List<Pawn> ownedPawns = lord.ownedPawns;
			for (int i = 0; i < ownedPawns.Count; i++)
			{
				if (GatheringsUtility.InGatheringArea(ownedPawns[i].Position, target.Position, base.Map))
				{
					if (!Data.presentForTicks.ContainsKey(ownedPawns[i]))
					{
						Data.presentForTicks.Add(ownedPawns[i], 0);
					}
					Data.presentForTicks[ownedPawns[i]]++;
				}
			}
		}

		public override void UpdateAllDuties()
		{
			IntVec3 spot = ((LordJob_BestowingCeremony)lord.LordJob).GetSpot();
			for (int i = 0; i < lord.ownedPawns.Count; i++)
			{
				Pawn pawn = lord.ownedPawns[i];
				if (!pawn.Awake())
				{
					RestUtility.WakeUp(pawn);
				}
				if (pawn == bestower)
				{
					PawnDuty pawnDuty = new PawnDuty(DutyDefOf.Idle);
					pawnDuty.focus = spot;
					pawn.mindState.duty = pawnDuty;
				}
				else if (pawn == target)
				{
					PawnDuty duty = new PawnDuty(DutyDefOf.Bestow, bestower, spot);
					pawn.mindState.duty = duty;
				}
				else
				{
					PawnDuty pawnDuty2 = new PawnDuty(DutyDefOf.Spectate, spot);
					pawnDuty2.spectateRect = CellRect.CenteredOn(spot, 0);
					pawnDuty2.spectateRectAllowedSides = SpectateRectSide.All;
					pawnDuty2.spectateDistance = new IntRange(2, 2);
					pawn.mindState.duty = pawnDuty2;
				}
				pawn.jobs?.CheckForJobOverride();
			}
		}
	}
}
