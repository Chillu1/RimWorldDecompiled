using UnityEngine;
using Verse;

namespace RimWorld;

public class RitualRoleColonist : RitualRole
{
	public WorkTypeDef requiredWorkType;

	public SkillDef usedSkill;

	public StatDef usedStat;

	public override bool AppliesToPawn(Pawn p, out string reason, TargetInfo selectedTarget, LordJob_Ritual ritual = null, RitualRoleAssignments assignments = null, Precept_Ritual precept = null, bool skipReason = false)
	{
		if (!AppliesIfChild(p, out reason, skipReason))
		{
			return false;
		}
		if (!p.RaceProps.Humanlike)
		{
			if (!skipReason)
			{
				reason = "MessageRitualRoleMustBeHumanlike".Translate(base.Label);
			}
			return false;
		}
		if (requiredWorkType != null && p.WorkTypeIsDisabled(requiredWorkType))
		{
			if (!skipReason)
			{
				reason = "MessageRitualRoleMustBeCapableOfGeneric".Translate(base.LabelCap, requiredWorkType.gerundLabel);
			}
			return false;
		}
		if (usedSkill != null && p.skills.GetSkill(usedSkill).TotallyDisabled)
		{
			if (!skipReason)
			{
				reason = "MessageRitualRoleMustBeCapableOfGeneric".Translate(base.LabelCap, usedSkill.label);
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
		if (p.GuestStatus == GuestStatus.Prisoner)
		{
			if (!skipReason)
			{
				reason = "MessageRitualRoleMustBeFree".Translate(base.Label);
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

	protected override int PawnDesirability(Pawn pawn)
	{
		int num = base.PawnDesirability(pawn);
		if (usedStat != null)
		{
			if (!usedStat.Worker.IsDisabledFor(pawn))
			{
				num += Mathf.RoundToInt(100f * pawn.GetStatValue(usedStat));
			}
		}
		else if (usedSkill != null)
		{
			num += 2 * pawn.skills.GetSkill(usedSkill).Level;
		}
		return num;
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Defs.Look(ref requiredWorkType, "requiredWorkType");
	}
}
