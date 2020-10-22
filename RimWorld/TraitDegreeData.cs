using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class TraitDegreeData
	{
		[MustTranslate]
		public string label;

		[MustTranslate]
		public string labelMale;

		[MustTranslate]
		public string labelFemale;

		[Unsaved(false)]
		[TranslationHandle]
		public string untranslatedLabel;

		[MustTranslate]
		public string description;

		public int degree;

		public float commonality = 1f;

		public List<StatModifier> statOffsets;

		public List<StatModifier> statFactors;

		public ThinkTreeDef thinkTree;

		public MentalStateDef randomMentalState;

		public SimpleCurve randomMentalStateMtbDaysMoodCurve;

		public List<MentalStateDef> disallowedMentalStates;

		public List<InspirationDef> disallowedInspirations;

		public List<InspirationDef> mentalBreakInspirationGainSet;

		public string mentalBreakInspirationGainReasonText;

		public List<MeditationFocusDef> allowedMeditationFocusTypes;

		public List<MeditationFocusDef> disallowedMeditationFocusTypes;

		public float mentalBreakInspirationGainChance;

		public List<MentalBreakDef> theOnlyAllowedMentalBreaks;

		public Dictionary<SkillDef, int> skillGains = new Dictionary<SkillDef, int>();

		public float socialFightChanceFactor = 1f;

		public float marketValueFactorOffset;

		public float randomDiseaseMtbDays;

		public float hungerRateFactor = 1f;

		public Type mentalStateGiverClass = typeof(TraitMentalStateGiver);

		[Unsaved(false)]
		private TraitMentalStateGiver mentalStateGiverInt;

		[Unsaved(false)]
		private string cachedLabelCap;

		[Unsaved(false)]
		private string cachedLabelMaleCap;

		[Unsaved(false)]
		private string cachedLabelFemaleCap;

		public string LabelCap
		{
			get
			{
				if (cachedLabelCap == null)
				{
					cachedLabelCap = label.CapitalizeFirst();
				}
				return cachedLabelCap;
			}
		}

		public TraitMentalStateGiver MentalStateGiver
		{
			get
			{
				if (mentalStateGiverInt == null)
				{
					mentalStateGiverInt = (TraitMentalStateGiver)Activator.CreateInstance(mentalStateGiverClass);
					mentalStateGiverInt.traitDegreeData = this;
				}
				return mentalStateGiverInt;
			}
		}

		public string GetLabelFor(Pawn pawn)
		{
			return GetLabelFor(pawn?.gender ?? Gender.None);
		}

		public string GetLabelCapFor(Pawn pawn)
		{
			return GetLabelCapFor(pawn?.gender ?? Gender.None);
		}

		public string GetLabelFor(Gender gender)
		{
			switch (gender)
			{
			case Gender.Male:
				if (!labelMale.NullOrEmpty())
				{
					return labelMale;
				}
				return label;
			case Gender.Female:
				if (!labelFemale.NullOrEmpty())
				{
					return labelFemale;
				}
				return label;
			default:
				return label;
			}
		}

		public string GetLabelCapFor(Gender gender)
		{
			switch (gender)
			{
			case Gender.Male:
				if (labelMale.NullOrEmpty())
				{
					return LabelCap;
				}
				if (cachedLabelMaleCap == null)
				{
					cachedLabelMaleCap = labelMale.CapitalizeFirst();
				}
				return cachedLabelMaleCap;
			case Gender.Female:
				if (labelFemale.NullOrEmpty())
				{
					return LabelCap;
				}
				if (cachedLabelFemaleCap == null)
				{
					cachedLabelFemaleCap = labelFemale.CapitalizeFirst();
				}
				return cachedLabelFemaleCap;
			default:
				return LabelCap;
			}
		}

		public void PostLoad()
		{
			untranslatedLabel = label;
		}
	}
}
