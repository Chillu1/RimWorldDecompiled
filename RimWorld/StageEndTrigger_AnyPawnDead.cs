using System.Collections.Generic;
using Verse;
using Verse.AI.Group;

namespace RimWorld
{
	public class StageEndTrigger_AnyPawnDead : StageEndTrigger
	{
		[NoTranslate]
		public List<string> roleIds;

		protected virtual bool Trigger(LordJob_Ritual ritual)
		{
			foreach (string roleId in roleIds)
			{
				foreach (Pawn item in ritual.assignments.AssignedPawns(roleId))
				{
					if (item.Dead)
					{
						return true;
					}
				}
			}
			return false;
		}

		public override Trigger MakeTrigger(LordJob_Ritual ritual, TargetInfo spot, IEnumerable<TargetInfo> foci, RitualStage stage)
		{
			return new Trigger_Custom((TriggerSignal signal) => Trigger(ritual));
		}

		public override void ExposeData()
		{
			Scribe_Collections.Look(ref roleIds, "roleIds", LookMode.Value);
		}
	}
}
