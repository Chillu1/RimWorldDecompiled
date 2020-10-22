using System.Collections.Generic;
using RimWorld;

namespace Verse
{
	public class RecipeMakerProperties
	{
		public int productCount = 1;

		public int targetCountAdjustment = 1;

		public int bulkRecipeCount = -1;

		public bool useIngredientsForColor = true;

		public int workAmount = -1;

		public StatDef workSpeedStat;

		public StatDef efficiencyStat;

		public ThingDef unfinishedThingDef;

		public ThingFilter defaultIngredientFilter;

		public List<SkillRequirement> skillRequirements;

		public SkillDef workSkill;

		public float workSkillLearnPerTick = 1f;

		public WorkTypeDef requiredGiverWorkType;

		public EffecterDef effectWorking;

		public SoundDef soundWorking;

		public List<ThingDef> recipeUsers;

		public ResearchProjectDef researchPrerequisite;

		public List<ResearchProjectDef> researchPrerequisites;

		[NoTranslate]
		public List<string> factionPrerequisiteTags;
	}
}
