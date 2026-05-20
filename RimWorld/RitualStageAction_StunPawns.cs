using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class RitualStageAction_StunPawns : RitualStageAction
	{
		public List<string> roleIds = new List<string>();

		public int stunDurationTicks = 10;

		public override void Apply(LordJob_Ritual ritual)
		{
			foreach (string roleId in roleIds)
			{
				foreach (Pawn item in ritual.assignments.AssignedPawns(roleId))
				{
					item.stances.stunner.StunFor(stunDurationTicks, null, addBattleLog: false, showMote: false);
				}
			}
		}

		public override void ExposeData()
		{
			Scribe_Collections.Look(ref roleIds, "roleIds", LookMode.Undefined);
			Scribe_Values.Look(ref stunDurationTicks, "stunDurationTicks", 0);
		}
	}
}
