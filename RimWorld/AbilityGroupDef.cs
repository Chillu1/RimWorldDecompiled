using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class AbilityGroupDef : Def
{
	public int cooldownTicks;

	public bool sendMessageOnCooldownComplete;

	public List<string> ritualRoleIds;
}
