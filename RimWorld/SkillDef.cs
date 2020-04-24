using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld
{
	public class SkillDef : Def
	{
		[MustTranslate]
		public string skillLabel;

		public bool usuallyDefinedInBackstories = true;

		public bool pawnCreatorSummaryVisible;

		public WorkTags disablingWorkTags;

		public float listOrder;

		public override void PostLoad()
		{
			if (label == null)
			{
				label = skillLabel;
			}
		}

		public bool IsDisabled(WorkTags combinedDisabledWorkTags, IEnumerable<WorkTypeDef> disabledWorkTypes)
		{
			if ((combinedDisabledWorkTags & disablingWorkTags) != 0)
			{
				return true;
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
}
