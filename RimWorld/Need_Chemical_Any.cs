using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class Need_Chemical_Any : Need
	{
		public enum MoodBuff
		{
			ExtremelyNegative,
			VeryNegative,
			Negative,
			Neutral,
			Positive,
			VeryPositive
		}

		public struct LevelThresholds
		{
			public float extremelyNegative;

			public float veryNegative;

			public float negative;

			public float positive;

			public float veryPositive;
		}

		public const int InterestTraitDegree = 1;

		public const int FascinationTraitDegree = 2;

		private const float FallPerTickFactorForChemicalFascination = 1.25f;

		public const float GainForHardDrugIngestion = 0.3f;

		public const float GainForSocialDrugIngestion = 0.2f;

		private static readonly SimpleCurve InterestDegreeFallCurve = new SimpleCurve
		{
			new CurvePoint(0f, 0.3f),
			new CurvePoint(FascinationDegreeLevelThresholdsForMood.negative, 0.6f),
			new CurvePoint(FascinationDegreeLevelThresholdsForMood.negative + 0.001f, 1f),
			new CurvePoint(FascinationDegreeLevelThresholdsForMood.positive, 1f),
			new CurvePoint(1f, 1f)
		};

		private static readonly SimpleCurve FascinationDegreeFallCurve = new SimpleCurve
		{
			new CurvePoint(0f, 0.4f),
			new CurvePoint(FascinationDegreeLevelThresholdsForMood.negative, 0.7f),
			new CurvePoint(FascinationDegreeLevelThresholdsForMood.negative + 0.001f, 1f),
			new CurvePoint(FascinationDegreeLevelThresholdsForMood.positive, 1f),
			new CurvePoint(1f, 1.15f)
		};

		private static readonly LevelThresholds FascinationDegreeLevelThresholdsForMood;

		private static readonly LevelThresholds InterestDegreeLevelThresholdsForMood;

		private Trait lastThresholdUpdateTraitRef;

		private Trait TraitDrugDesire => pawn.story.traits.GetTrait(TraitDefOf.DrugDesire);

		private SimpleCurve FallCurve
		{
			get
			{
				if (TraitDrugDesire.Degree == 2)
				{
					return FascinationDegreeFallCurve;
				}
				return InterestDegreeFallCurve;
			}
		}

		private float FallPerNeedIntervalTick
		{
			get
			{
				Trait traitDrugDesire = TraitDrugDesire;
				float num = 1f;
				if (traitDrugDesire.Degree == 2)
				{
					num = 1.25f;
				}
				num *= FallCurve.Evaluate(CurLevel);
				return def.fallPerDay * num / 60000f * 150f;
			}
		}

		private LevelThresholds CurrentLevelThresholds
		{
			get
			{
				if (TraitDrugDesire.Degree == 2)
				{
					return FascinationDegreeLevelThresholdsForMood;
				}
				return InterestDegreeLevelThresholdsForMood;
			}
		}

		public MoodBuff MoodBuffForCurrentLevel
		{
			get
			{
				if (Disabled)
				{
					return MoodBuff.Neutral;
				}
				LevelThresholds currentLevelThresholds = CurrentLevelThresholds;
				float curLevel = CurLevel;
				if (curLevel <= currentLevelThresholds.extremelyNegative)
				{
					return MoodBuff.ExtremelyNegative;
				}
				if (curLevel <= currentLevelThresholds.veryNegative)
				{
					return MoodBuff.VeryNegative;
				}
				if (curLevel <= currentLevelThresholds.negative)
				{
					return MoodBuff.Negative;
				}
				if (curLevel <= currentLevelThresholds.positive)
				{
					return MoodBuff.Neutral;
				}
				if (curLevel <= currentLevelThresholds.veryPositive)
				{
					return MoodBuff.Positive;
				}
				return MoodBuff.VeryPositive;
			}
		}

		public override int GUIChangeArrow => 0;

		public override bool ShowOnNeedList => !Disabled;

		private bool Disabled
		{
			get
			{
				if (TraitDrugDesire != null)
				{
					return TraitDrugDesire.Degree < 1;
				}
				return true;
			}
		}

		public void Notify_IngestedDrug(Thing drug)
		{
			if (!Disabled)
			{
				switch (drug.def.ingestible.drugCategory)
				{
				case DrugCategory.Social:
					CurLevel += 0.2f;
					break;
				case DrugCategory.Hard:
					CurLevel += 0.3f;
					break;
				}
			}
		}

		public Need_Chemical_Any(Pawn pawn)
			: base(pawn)
		{
		}

		public override void SetInitialLevel()
		{
			CurLevel = 0.5f;
		}

		public override void DrawOnGUI(Rect rect, int maxThresholdMarkers = int.MaxValue, float customMargin = -1f, bool drawArrows = true, bool doTooltip = true)
		{
			Trait traitDrugDesire = TraitDrugDesire;
			if (traitDrugDesire != null && lastThresholdUpdateTraitRef != traitDrugDesire)
			{
				lastThresholdUpdateTraitRef = traitDrugDesire;
				threshPercents = new List<float>();
				LevelThresholds currentLevelThresholds = CurrentLevelThresholds;
				threshPercents.Add(currentLevelThresholds.extremelyNegative);
				threshPercents.Add(currentLevelThresholds.veryNegative);
				threshPercents.Add(currentLevelThresholds.negative);
				threshPercents.Add(currentLevelThresholds.positive);
				threshPercents.Add(currentLevelThresholds.veryPositive);
			}
			base.DrawOnGUI(rect, maxThresholdMarkers, customMargin, drawArrows, doTooltip);
		}

		public override void NeedInterval()
		{
			if (Disabled)
			{
				SetInitialLevel();
			}
			else if (!IsFrozen)
			{
				CurLevel -= FallPerNeedIntervalTick;
			}
		}

		static Need_Chemical_Any()
		{
			LevelThresholds levelThresholds = new LevelThresholds
			{
				extremelyNegative = 0.1f,
				veryNegative = 0.25f,
				negative = 0.4f,
				positive = 0.7f,
				veryPositive = 0.85f
			};
			FascinationDegreeLevelThresholdsForMood = levelThresholds;
			levelThresholds = new LevelThresholds
			{
				extremelyNegative = 0.01f,
				veryNegative = 0.15f,
				negative = 0.3f,
				positive = 0.6f,
				veryPositive = 0.75f
			};
			InterestDegreeLevelThresholdsForMood = levelThresholds;
		}
	}
}
