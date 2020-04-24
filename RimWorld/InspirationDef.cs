using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class InspirationDef : Def
	{
		public Type inspirationClass = typeof(Inspiration);

		public Type workerClass = typeof(InspirationWorker);

		public float baseCommonality = 1f;

		public float baseDurationDays = 1f;

		public bool allowedOnAnimals;

		public bool allowedOnNonColonists;

		public bool allowedOnDownedPawns = true;

		public List<StatDef> requiredNonDisabledStats;

		public List<SkillRequirement> requiredSkills;

		public List<SkillRequirement> requiredAnySkill;

		public List<WorkTypeDef> requiredNonDisabledWorkTypes;

		public List<WorkTypeDef> requiredAnyNonDisabledWorkType;

		public List<PawnCapacityDef> requiredCapacities;

		public List<SkillDef> associatedSkills;

		public List<StatModifier> statOffsets;

		public List<StatModifier> statFactors;

		[MustTranslate]
		public string beginLetter;

		[MustTranslate]
		public string beginLetterLabel;

		public LetterDef beginLetterDef;

		[MustTranslate]
		public string endMessage;

		[MustTranslate]
		public string baseInspectLine;

		[Unsaved(false)]
		private InspirationWorker workerInt;

		public InspirationWorker Worker
		{
			get
			{
				if (workerInt == null)
				{
					workerInt = (InspirationWorker)Activator.CreateInstance(workerClass);
					workerInt.def = this;
				}
				return workerInt;
			}
		}
	}
}
