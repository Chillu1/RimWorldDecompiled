using System.Collections.Generic;
using Verse;
using Verse.AI.Group;

namespace RimWorld
{
	public class StageEndTrigger_PawnDead : StageEndTrigger
	{
		[NoTranslate]
		public string roleId;

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
			return new Trigger_TickCondition(() => pawn.Dead);
		}

		public override void ExposeData()
		{
			Scribe_Values.Look(ref roleId, "roleId");
		}
	}
}
