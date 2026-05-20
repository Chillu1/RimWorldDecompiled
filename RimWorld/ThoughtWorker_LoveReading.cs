using Verse;

namespace RimWorld;

public class ThoughtWorker_LoveReading : ThoughtWorker
{
	protected override ThoughtState CurrentStateInternal(Pawn p)
	{
		if (p.CurJobDef != JobDefOf.Reading)
		{
			return ThoughtState.Inactive;
		}
		if (!(p.jobs.curDriver is JobDriver_Reading { IsReading: not false }))
		{
			return ThoughtState.Inactive;
		}
		SkillRecord skill = p.skills.GetSkill(SkillDefOf.Intellectual);
		if (skill.passion == Passion.Minor)
		{
			return ThoughtState.ActiveAtStage(0);
		}
		if (skill.passion == Passion.Major)
		{
			return ThoughtState.ActiveAtStage(1);
		}
		return ThoughtState.Inactive;
	}
}
