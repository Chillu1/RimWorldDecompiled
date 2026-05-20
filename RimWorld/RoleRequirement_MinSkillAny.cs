using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld;

public class RoleRequirement_MinSkillAny : RoleRequirement
{
	public List<SkillRequirement> skills;

	[NoTranslate]
	private string labelCached;

	public override string GetLabel(Precept_Role role)
	{
		if (labelCached == null)
		{
			if (skills.Count == 1)
			{
				labelCached = "RoleRequirementSkill".Translate() + ": " + GetSkillStr(skills[0]);
			}
			else
			{
				labelCached = "RoleRequirementSkillAny".Translate() + ": " + skills.Select((SkillRequirement s) => GetSkillStr(s)).ToCommaList();
			}
		}
		return labelCached;
		static string GetSkillStr(SkillRequirement requirement)
		{
			return string.Concat(requirement.skill.LabelCap + " ", requirement.minLevel.ToString());
		}
	}

	public override bool Met(Pawn p, Precept_Role role)
	{
		foreach (SkillRequirement skill in skills)
		{
			if (p.skills.GetSkill(skill.skill).Level >= skill.minLevel)
			{
				return true;
			}
		}
		return false;
	}
}
