using System.Collections.Generic;
using Verse;

namespace RimWorld;

public static class TraitUtility
{
	public static void ApplySkillGainFromTrait(Pawn pawn, Trait trait)
	{
		List<SkillGain> skillGains = trait.CurrentData.skillGains;
		if (trait.Suppressed || skillGains == null || pawn.skills == null)
		{
			return;
		}
		foreach (SkillGain item in skillGains)
		{
			SkillRecord skill = pawn.skills.GetSkill(item.skill);
			if (skill != null && !skill.PermanentlyDisabled)
			{
				skill.Level = skill.GetLevel(includeAptitudes: false) + item.amount;
			}
		}
	}
}
