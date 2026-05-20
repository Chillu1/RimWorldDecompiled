using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class ThoughtWorker_SkillBookPassion : ThoughtWorker
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
		BookOutcomeDoerGainSkillExp doer = ((Book)p.CurJob.targetA.Thing).BookComp.GetDoer<BookOutcomeDoerGainSkillExp>();
		bool flag = false;
		if (doer != null)
		{
			foreach (KeyValuePair<SkillDef, float> value2 in doer.Values)
			{
				value2.Deconstruct(out var key, out var _);
				SkillDef skillDef = key;
				SkillRecord skill = p.skills.GetSkill(skillDef);
				if (skill.passion == Passion.Major)
				{
					return ThoughtState.ActiveAtStage(1);
				}
				if (skill.passion == Passion.Minor)
				{
					flag = true;
				}
			}
		}
		if (flag)
		{
			return ThoughtState.ActiveAtStage(0);
		}
		return ThoughtState.Inactive;
	}
}
