using RimWorld;

namespace Verse
{
	public class HediffComp_SkillDecay : HediffComp
	{
		public HediffCompProperties_SkillDecay Props => (HediffCompProperties_SkillDecay)props;

		public override void CompPostTick(ref float severityAdjustment)
		{
			Pawn_SkillTracker skills = base.Pawn.skills;
			if (skills != null)
			{
				for (int i = 0; i < skills.skills.Count; i++)
				{
					SkillRecord skillRecord = skills.skills[i];
					float num = parent.Severity * Props.decayPerDayPercentageLevelCurve.Evaluate(skillRecord.Level);
					float num2 = skillRecord.XpRequiredForLevelUp * num / 60000f;
					skillRecord.Learn(0f - num2);
				}
			}
		}
	}
}
