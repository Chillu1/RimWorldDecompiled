using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.Grammar;

namespace RimWorld;

public class SkillDef : Def
{
	[MustTranslate]
	public string skillLabel;

	public RulePack generalRules;

	public bool usuallyDefinedInBackstories = true;

	public bool pawnCreatorSummaryVisible;

	public WorkTags disablingWorkTags;

	public float listOrder;

	public bool neverDisabledBasedOnWorkTypes;

	public InteractionDef lessonInteraction;

	public override void PostLoad()
	{
		if (label == null)
		{
			label = skillLabel;
		}
	}

	public override void ResolveReferences()
	{
		base.ResolveReferences();
		if (lessonInteraction == null)
		{
			lessonInteraction = InteractionDefOf.LessonGeneric;
		}
	}

	public bool IsDisabled(WorkTags combinedDisabledWorkTags, IEnumerable<WorkTypeDef> disabledWorkTypes)
	{
		if ((combinedDisabledWorkTags & disablingWorkTags) != WorkTags.None)
		{
			return true;
		}
		if (neverDisabledBasedOnWorkTypes)
		{
			return false;
		}
		List<WorkTypeDef> allDefsListForReading = DefDatabase<WorkTypeDef>.AllDefsListForReading;
		bool flag = false;
		for (int i = 0; i < allDefsListForReading.Count; i++)
		{
			WorkTypeDef workTypeDef = allDefsListForReading[i];
			for (int j = 0; j < workTypeDef.relevantSkills.Count; j++)
			{
				if (workTypeDef.relevantSkills[j] == this)
				{
					if (!disabledWorkTypes.Contains(workTypeDef))
					{
						return false;
					}
					flag = true;
				}
			}
		}
		if (!flag)
		{
			return false;
		}
		return true;
	}
}
