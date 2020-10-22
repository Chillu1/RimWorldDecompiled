using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld
{
	public class IngestibleProperties
	{
		[Unsaved(false)]
		public ThingDef parent;

		public int maxNumToIngestAtOnce = 20;

		public List<IngestionOutcomeDoer> outcomeDoers;

		public int baseIngestTicks = 500;

		public float chairSearchRadius = 32f;

		public bool useEatingSpeedStat = true;

		public ThoughtDef tasteThought;

		public ThoughtDef specialThoughtDirect;

		public ThoughtDef specialThoughtAsIngredient;

		public EffecterDef ingestEffect;

		public EffecterDef ingestEffectEat;

		public SoundDef ingestSound;

		[MustTranslate]
		public string ingestCommandString;

		[MustTranslate]
		public string ingestReportString;

		[MustTranslate]
		public string ingestReportStringEat;

		public HoldOffsetSet ingestHoldOffsetStanding;

		public bool ingestHoldUsesTable = true;

		public FoodTypeFlags foodType;

		public float joy;

		public JoyKindDef joyKind;

		public ThingDef sourceDef;

		public FoodPreferability preferability;

		public bool nurseable;

		public float optimalityOffsetHumanlikes;

		public float optimalityOffsetFeedingAnimals;

		public DrugCategory drugCategory;

		public bool canAutoSelectAsFoodForCaravan = true;

		[Unsaved(false)]
		private float cachedNutrition = -1f;

		public JoyKindDef JoyKind
		{
			get
			{
				if (joyKind == null)
				{
					return JoyKindDefOf.Gluttonous;
				}
				return joyKind;
			}
		}

		public bool HumanEdible => (FoodTypeFlags.OmnivoreHuman & foodType) != 0;

		public bool IsMeal
		{
			get
			{
				if ((int)preferability >= 6)
				{
					return (int)preferability <= 9;
				}
				return false;
			}
		}

		public float CachedNutrition
		{
			get
			{
				if (cachedNutrition == -1f)
				{
					cachedNutrition = parent.GetStatValueAbstract(StatDefOf.Nutrition);
				}
				return cachedNutrition;
			}
		}

		public IEnumerable<string> ConfigErrors()
		{
			if (preferability == FoodPreferability.Undefined)
			{
				yield return "undefined preferability";
			}
			if (foodType == FoodTypeFlags.None)
			{
				yield return "no foodType";
			}
			if (parent.GetStatValueAbstract(StatDefOf.Nutrition) == 0f && preferability != FoodPreferability.NeverForNutrition)
			{
				yield return string.Concat("Nutrition == 0 but preferability is ", preferability, " instead of ", FoodPreferability.NeverForNutrition);
			}
			if (!parent.IsCorpse && (int)preferability > 3 && !parent.socialPropernessMatters && parent.EverHaulable)
			{
				yield return "ingestible preferability > DesperateOnlyForHumanlikes but socialPropernessMatters=false. This will cause bugs wherein wardens will look in prison cells for food to give to prisoners and so will repeatedly pick up and drop food inside the cell.";
			}
			if (joy > 0f && joyKind == null)
			{
				yield return "joy > 0 with no joy kind";
			}
			if (joy == 0f && joyKind != null)
			{
				yield return "joy is 0 but joyKind is " + joyKind;
			}
		}

		public RoyalTitleDef MaxSatisfiedTitle()
		{
			return (from t in DefDatabase<FactionDef>.AllDefsListForReading.SelectMany((FactionDef f) => f.RoyalTitlesAwardableInSeniorityOrderForReading)
				where t.foodRequirement.Defined && t.foodRequirement.Acceptable(parent)
				orderby t.seniority descending
				select t).FirstOrDefault();
		}

		internal IEnumerable<StatDrawEntry> SpecialDisplayStats()
		{
			if (joy > 0f)
			{
				StatCategoryDef category = ((drugCategory != 0) ? StatCategoryDefOf.Drug : StatCategoryDefOf.Basics);
				yield return new StatDrawEntry(category, "Joy".Translate(), joy.ToStringPercent("F0") + " (" + JoyKind.label + ")", "Stat_Thing_Ingestible_Joy_Desc".Translate(), 4751);
			}
			if (HumanEdible)
			{
				RoyalTitleDef royalTitleDef = MaxSatisfiedTitle();
				if (royalTitleDef != null)
				{
					yield return new StatDrawEntry(StatCategoryDefOf.Basics, "Stat_Thing_Ingestible_MaxSatisfiedTitle".Translate(), royalTitleDef.GetLabelCapForBothGenders(), "Stat_Thing_Ingestible_MaxSatisfiedTitle_Desc".Translate(), 4752);
				}
			}
			if (outcomeDoers == null)
			{
				yield break;
			}
			for (int i = 0; i < outcomeDoers.Count; i++)
			{
				foreach (StatDrawEntry item in outcomeDoers[i].SpecialDisplayStats(parent))
				{
					yield return item;
				}
			}
		}
	}
}
