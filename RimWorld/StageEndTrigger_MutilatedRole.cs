using System.Collections.Generic;
using Verse;
using Verse.AI.Group;

namespace RimWorld;

public class StageEndTrigger_MutilatedRole : StageEndTrigger
{
	[NoTranslate]
	public string roleId;

	public override Trigger MakeTrigger(LordJob_Ritual ritual, TargetInfo spot, IEnumerable<TargetInfo> foci, RitualStage stage)
	{
		LordJob_Ritual_Mutilation r = ritual as LordJob_Ritual_Mutilation;
		if (r != null)
		{
			return new Trigger_Custom(delegate
			{
				foreach (Pawn mutilatedPawn in r.mutilatedPawns)
				{
					RitualRole ritualRole = r.RoleFor(mutilatedPawn, includeForced: true);
					if (ritualRole != null && ritualRole.id == roleId)
					{
						return true;
					}
				}
				return false;
			});
		}
		Log.Error("Used StageEndTrigger_MutilatedRole on non LordJob_Ritual_Mutilation ritual job");
		return null;
	}

	public override void ExposeData()
	{
		Scribe_Values.Look(ref roleId, "roleId");
	}
}
