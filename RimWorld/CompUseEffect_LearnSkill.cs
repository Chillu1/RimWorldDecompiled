using Verse;

namespace RimWorld
{
	public class CompUseEffect_LearnSkill : CompUseEffect
	{
		private const float XPGainAmount = 50000f;

		private SkillDef Skill => parent.GetComp<CompNeurotrainer>().skill;

		public override void DoEffect(Pawn user)
		{
			base.DoEffect(user);
			SkillDef skill = Skill;
			int level = user.skills.GetSkill(skill).Level;
			user.skills.Learn(skill, 50000f, direct: true);
			int level2 = user.skills.GetSkill(skill).Level;
			if (PawnUtility.ShouldSendNotificationAbout(user))
			{
				Messages.Message("SkillNeurotrainerUsed".Translate(user.LabelShort, skill.LabelCap, level, level2, user.Named("USER")), user, MessageTypeDefOf.PositiveEvent);
			}
		}

		public override bool CanBeUsedBy(Pawn p, out string failReason)
		{
			if (p.skills == null)
			{
				failReason = null;
				return false;
			}
			if (p.skills.GetSkill(Skill).TotallyDisabled)
			{
				failReason = "SkillDisabled".Translate();
				return false;
			}
			return base.CanBeUsedBy(p, out failReason);
		}
	}
}
