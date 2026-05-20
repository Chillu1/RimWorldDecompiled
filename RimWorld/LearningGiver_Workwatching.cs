using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld;

public class LearningGiver_Workwatching : LearningGiver
{
	private const float MaxDistanceToAdult = 100f;

	public override bool CanDo(Pawn pawn)
	{
		if (base.CanDo(pawn))
		{
			return AdultToWorkwatch(pawn) != null;
		}
		return false;
	}

	public static Pawn AdultToWorkwatch(Pawn child)
	{
		Pawn pawn = null;
		float num = 0f;
		foreach (Pawn item in child.Map.mapPawns.FreeColonistsSpawned)
		{
			float lengthHorizontal = (item.Position - child.Position).LengthHorizontal;
			if (!(lengthHorizontal > 100f) && !item.DevelopmentalStage.Juvenile() && ChildCanLearnFromAdultJob(child, item) && !item.IsForbidden(child) && child.CanReach(item, PathEndMode.Touch, Danger.Some) && (pawn == null || lengthHorizontal < num))
			{
				num = lengthHorizontal;
				pawn = item;
			}
		}
		return pawn;
	}

	public static bool ChildCanLearnFromAdultJob(Pawn child, Pawn adult)
	{
		SkillDef skillDef = adult.CurJob?.RecipeDef?.workSkill;
		if (skillDef != null)
		{
			return !child.skills.GetSkill(skillDef).TotallyDisabled;
		}
		List<SkillDef> list = adult.CurJob?.workGiverDef?.workType?.relevantSkills;
		if (list.NullOrEmpty())
		{
			return false;
		}
		foreach (SkillDef item in list)
		{
			if (child.skills.GetSkill(item).TotallyDisabled)
			{
				return false;
			}
		}
		return true;
	}

	public override Job TryGiveJob(Pawn pawn)
	{
		Pawn pawn2 = AdultToWorkwatch(pawn);
		if (pawn2 == null)
		{
			return null;
		}
		return JobMaker.MakeJob(def.jobDef, pawn2);
	}
}
