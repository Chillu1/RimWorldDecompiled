using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace RimWorld
{
	public class LordToil_PrisonerEscape : LordToil_Travel
	{
		private int sapperThingID;

		public override IntVec3 FlagLoc => Data.dest;

		private LordToilData_Travel Data => (LordToilData_Travel)data;

		public override bool AllowSatisfyLongNeeds => false;

		protected override float AllArrivedCheckRadius => 14f;

		public LordToil_PrisonerEscape(IntVec3 dest, int sapperThingID)
			: base(dest)
		{
			this.sapperThingID = sapperThingID;
		}

		public override void UpdateAllDuties()
		{
			LordToilData_Travel data = Data;
			Pawn leader = GetLeader();
			for (int i = 0; i < lord.ownedPawns.Count; i++)
			{
				Pawn pawn = lord.ownedPawns[i];
				if (IsSapper(pawn))
				{
					pawn.mindState.duty = new PawnDuty(DutyDefOf.PrisonerEscapeSapper, data.dest);
				}
				else if (leader == null || pawn == leader)
				{
					pawn.mindState.duty = new PawnDuty(DutyDefOf.PrisonerEscape, data.dest);
				}
				else
				{
					pawn.mindState.duty = new PawnDuty(DutyDefOf.PrisonerEscape, leader, 10f);
				}
			}
		}

		public override void LordToilTick()
		{
			base.LordToilTick();
			for (int i = 0; i < lord.ownedPawns.Count; i++)
			{
				lord.ownedPawns[i].guilt.Notify_Guilty();
			}
		}

		private Pawn GetLeader()
		{
			for (int i = 0; i < lord.ownedPawns.Count; i++)
			{
				if (!lord.ownedPawns[i].Downed && IsSapper(lord.ownedPawns[i]))
				{
					return lord.ownedPawns[i];
				}
			}
			for (int j = 0; j < lord.ownedPawns.Count; j++)
			{
				if (!lord.ownedPawns[j].Downed)
				{
					return lord.ownedPawns[j];
				}
			}
			return null;
		}

		private bool IsSapper(Pawn p)
		{
			return p.thingIDNumber == sapperThingID;
		}
	}
}
