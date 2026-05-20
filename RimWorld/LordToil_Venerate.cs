using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace RimWorld
{
	public class LordToil_Venerate : LordToil
	{
		private Thing target;

		private int FarSpectateRectExpansion = 2;

		private IntRange VenerateCloseTicksRange = new IntRange(750, 1500);

		private IntRange VenerateHandoverTicksRange = new IntRange(60, 120);

		private LordToilData_Venerate Data => (LordToilData_Venerate)data;

		public LordToil_Venerate(Thing target)
		{
			this.target = target;
			data = new LordToilData_Venerate();
		}

		public override void UpdateAllDuties()
		{
			LordToilData_Venerate lordToilData_Venerate = Data;
			CellRect cellRect = CellRect.CenteredOn(target.Position, target.def.size.x, target.def.size.z);
			CellRect cellRect2 = cellRect.ExpandedBy(FarSpectateRectExpansion);
			SpectateRectSide preferredSpectateSide = GetPreferredSpectateSide();
			int num = -1;
			for (int i = 0; i < lord.ownedPawns.Count; i++)
			{
				Pawn pawn = lord.ownedPawns[i];
				if (pawn.mindState.duty != null && pawn.mindState.duty.def == DutyDefOf.Pilgrims_Spectate && pawn.mindState.duty.spectateRect == cellRect)
				{
					num = i;
					break;
				}
			}
			if (num >= 0 && lord.ownedPawns.Count > 1)
			{
				MakePawnSpectate(lord.ownedPawns[num], cellRect2, preferredSpectateSide);
				lordToilData_Venerate.currentNearVeneratorTicks = VenerateHandoverTicksRange.RandomInRange;
				return;
			}
			num = (lordToilData_Venerate.lastNearVeneratorIndex + 1) % lord.ownedPawns.Count;
			for (int j = 0; j < lord.ownedPawns.Count; j++)
			{
				Pawn pawn2 = lord.ownedPawns[j];
				CellRect cellRect3 = ((num == j) ? cellRect : cellRect2);
				if (pawn2.mindState.duty == null || pawn2.mindState.duty.def != DutyDefOf.Pilgrims_Spectate || pawn2.mindState.duty.spectateRect != cellRect3)
				{
					MakePawnSpectate(pawn2, cellRect3, preferredSpectateSide);
				}
			}
			lordToilData_Venerate.currentNearVeneratorTicks = VenerateCloseTicksRange.RandomInRange;
			lordToilData_Venerate.lastNearVeneratorIndex = num;
		}

		private void MakePawnSpectate(Pawn p, CellRect rect, SpectateRectSide side)
		{
			PawnDuty pawnDuty = new PawnDuty(DutyDefOf.Pilgrims_Spectate);
			pawnDuty.spectateRect = rect;
			pawnDuty.spectateRectAllowedSides = side;
			pawnDuty.locomotion = LocomotionUrgency.Amble;
			pawnDuty.focus = target;
			p.mindState.duty = pawnDuty;
			if (p.jobs.curJob != null)
			{
				p.jobs.EndCurrentJob(JobCondition.InterruptForced);
			}
		}

		public override void LordToilTick()
		{
			LordToilData_Venerate lordToilData_Venerate = Data;
			if (lordToilData_Venerate.currentNearVeneratorTicks < 0)
			{
				UpdateAllDuties();
			}
			lordToilData_Venerate.currentNearVeneratorTicks--;
		}

		private SpectateRectSide GetPreferredSpectateSide()
		{
			IntVec3 interactionCell = target.InteractionCell;
			IntVec3 intVec = target.Position - interactionCell;
			if (intVec.x > 0)
			{
				return SpectateRectSide.Left;
			}
			if (intVec.x < 0)
			{
				return SpectateRectSide.Right;
			}
			if (intVec.z > 0)
			{
				return SpectateRectSide.Down;
			}
			if (intVec.z < 0)
			{
				return SpectateRectSide.Up;
			}
			return SpectateRectSide.All;
		}
	}
}
