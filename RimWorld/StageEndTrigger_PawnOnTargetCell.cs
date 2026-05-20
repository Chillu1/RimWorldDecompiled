using System.Collections.Generic;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace RimWorld
{
	public class StageEndTrigger_PawnOnTargetCell : StageEndTrigger
	{
		public string roleId;

		public int waitTicks = 50;

		public override Trigger MakeTrigger(LordJob_Ritual ritual, TargetInfo spot, IEnumerable<TargetInfo> foci, RitualStage stage)
		{
			if (ritual.Ritual.behavior.def.roles.FirstOrDefault((RitualRole r) => r.id == roleId) == null)
			{
				return null;
			}
			Pawn pawn = ritual.assignments.FirstAssignedPawn(roleId);
			if (pawn == null)
			{
				return null;
			}
			PawnStagePosition dest = ritual.PawnPositionForStage(pawn, stage);
			return new Trigger_TicksPassedAfterConditionMet(waitTicks, delegate
			{
				if (pawn.CanReachImmediate(dest.cell, PathEndMode.ClosestTouch))
				{
					return true;
				}
				return pawn.Dead ? true : false;
			});
		}

		public override void ExposeData()
		{
			Scribe_Values.Look(ref roleId, "roleId");
			Scribe_Values.Look(ref waitTicks, "waitTicks", 0);
		}
	}
}
