using Verse;

namespace RimWorld;

public class CompUseEffect_LearnSkill : CompUseEffect
{
	public CompProperties_UseEffect_LearnSkill Props => (CompProperties_UseEffect_LearnSkill)props;

	public override void DoEffect(Pawn user)
	{
		base.DoEffect(user);
		int level = user.skills.GetSkill(Props.skill).GetLevel();
		user.skills.Learn(Props.skill, Props.xpGainAmount, direct: true);
		int level2 = user.skills.GetSkill(Props.skill).GetLevel();
		if (PawnUtility.ShouldSendNotificationAbout(user))
		{
			Messages.Message("SkillNeurotrainerUsed".Translate(user.LabelShort, Props.skill.LabelCap, level, level2, user.Named("USER")), user, MessageTypeDefOf.PositiveEvent);
		}
	}

	public override AcceptanceReport CanBeUsedBy(Pawn p)
	{
		if (p.skills == null)
		{
			return false;
		}
		if (p.skills.GetSkill(Props.skill).TotallyDisabled)
		{
			return "SkillDisabled".Translate();
		}
		return base.CanBeUsedBy(p);
	}

	public override bool AllowStackWith(Thing other)
	{
		if (!base.AllowStackWith(other))
		{
			return false;
		}
		CompUseEffect_LearnSkill compUseEffect_LearnSkill = other.TryGetComp<CompUseEffect_LearnSkill>();
		if (compUseEffect_LearnSkill == null || compUseEffect_LearnSkill.Props.skill != Props.skill)
		{
			return false;
		}
		return true;
	}
}
