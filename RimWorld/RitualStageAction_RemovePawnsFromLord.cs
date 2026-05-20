using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class RitualStageAction_RemovePawnsFromLord : RitualStageAction
	{
		public List<string> roleIds = new List<string>();

		public override void Apply(LordJob_Ritual ritual)
		{
			foreach (string roleId in roleIds)
			{
				foreach (Pawn item in ritual.assignments.AssignedPawns(roleId))
				{
					ritual.lord.RemovePawn(item);
				}
			}
		}

		public override void ExposeData()
		{
			Scribe_Collections.Look(ref roleIds, "roleIds", LookMode.Undefined);
		}
	}
}
