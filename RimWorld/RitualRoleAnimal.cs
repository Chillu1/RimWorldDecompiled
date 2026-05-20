using Verse;

namespace RimWorld;

public class RitualRoleAnimal : RitualRole
{
	private float minBodySize;

	public override bool Animal => true;

	public override bool AppliesToPawn(Pawn p, out string reason, TargetInfo selectedTarget, LordJob_Ritual ritual = null, RitualRoleAssignments assignments = null, Precept_Ritual precept = null, bool skipReason = false)
	{
		reason = null;
		if (!p.IsAnimal)
		{
			if (!skipReason)
			{
				reason = "MessageRitualRoleMustBeAnimal".Translate(base.LabelCap);
			}
			return false;
		}
		if (p.BodySize < minBodySize)
		{
			if (!skipReason)
			{
				reason = "MessageRitualRoleMustHaveLargerBodySize".Translate(base.LabelCap, minBodySize.ToString("0.00"));
			}
			return false;
		}
		if (!p.Faction.IsPlayerSafe())
		{
			if (!skipReason)
			{
				reason = "MessageRitualRoleMustBeColonist".Translate(base.Label);
			}
			return false;
		}
		return true;
	}

	public override bool AppliesToRole(Precept_Role role, out string reason, Precept_Ritual ritual = null, Pawn p = null, bool skipReason = false)
	{
		reason = null;
		return false;
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Values.Look(ref minBodySize, "minBodySize", 0f);
	}
}
