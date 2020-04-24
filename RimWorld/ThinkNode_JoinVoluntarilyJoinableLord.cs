using System.Collections.Generic;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace RimWorld
{
	public class ThinkNode_JoinVoluntarilyJoinableLord : ThinkNode_Priority
	{
		public ThinkTreeDutyHook dutyHook;

		public override ThinkNode DeepCopy(bool resolve = true)
		{
			ThinkNode_JoinVoluntarilyJoinableLord obj = (ThinkNode_JoinVoluntarilyJoinableLord)base.DeepCopy(resolve);
			obj.dutyHook = dutyHook;
			return obj;
		}

		public override ThinkResult TryIssueJobPackage(Pawn pawn, JobIssueParams jobParams)
		{
			CheckLeaveCurrentVoluntarilyJoinableLord(pawn);
			JoinVoluntarilyJoinableLord(pawn);
			if (pawn.GetLord() != null && (pawn.mindState.duty == null || pawn.mindState.duty.def.hook == dutyHook))
			{
				return base.TryIssueJobPackage(pawn, jobParams);
			}
			return ThinkResult.NoJob;
		}

		private void CheckLeaveCurrentVoluntarilyJoinableLord(Pawn pawn)
		{
			Lord lord = pawn.GetLord();
			if (lord != null)
			{
				LordJob_VoluntarilyJoinable lordJob_VoluntarilyJoinable = lord.LordJob as LordJob_VoluntarilyJoinable;
				if (lordJob_VoluntarilyJoinable != null && lordJob_VoluntarilyJoinable.VoluntaryJoinPriorityFor(pawn) <= 0f)
				{
					lord.Notify_PawnLost(pawn, PawnLostCondition.LeftVoluntarily);
				}
			}
		}

		private void JoinVoluntarilyJoinableLord(Pawn pawn)
		{
			Lord lord = pawn.GetLord();
			Lord lord2 = null;
			float num = 0f;
			if (lord != null)
			{
				LordJob_VoluntarilyJoinable lordJob_VoluntarilyJoinable = lord.LordJob as LordJob_VoluntarilyJoinable;
				if (lordJob_VoluntarilyJoinable == null)
				{
					return;
				}
				lord2 = lord;
				num = lordJob_VoluntarilyJoinable.VoluntaryJoinPriorityFor(pawn);
			}
			List<Lord> lords = pawn.Map.lordManager.lords;
			for (int i = 0; i < lords.Count; i++)
			{
				LordJob_VoluntarilyJoinable lordJob_VoluntarilyJoinable2 = lords[i].LordJob as LordJob_VoluntarilyJoinable;
				if (lordJob_VoluntarilyJoinable2 != null && lords[i].CurLordToil.VoluntaryJoinDutyHookFor(pawn) == dutyHook)
				{
					float num2 = lordJob_VoluntarilyJoinable2.VoluntaryJoinPriorityFor(pawn);
					if (!(num2 <= 0f) && (lord2 == null || num2 > num))
					{
						lord2 = lords[i];
						num = num2;
					}
				}
			}
			if (lord2 != null && lord != lord2)
			{
				lord?.Notify_PawnLost(pawn, PawnLostCondition.LeftVoluntarily);
				lord2.AddPawn(pawn);
			}
		}
	}
}
