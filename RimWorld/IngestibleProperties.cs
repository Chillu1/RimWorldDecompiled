using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld;

public class IngestibleProperties
{
	[Unsaved(false)]
	public ThingDef parent;

	public int maxNumToIngestAtOnce;

	public int defaultNumToIngestAtOnce = 20;

	public List<IngestionOutcomeDoer> outcomeDoers;

	public int baseIngestTicks = 500;

	public float chairSearchRadius = 32f;

	public bool useEatingSpeedStat = true;

	public bool babiesCanIngest;

	public bool humanlikeOnly;

	public bool nonDrugIngestibleWithoutFoodNeed;

	public ThoughtDef tasteThought;

	public ThoughtDef specialThoughtDirect;

	public ThoughtDef specialThoughtAsIngredient;

	public HistoryEventDef ateEvent;

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

	public bool tableDesired = true;

	public bool showIngestFloatOption = true;

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

	public bool lowPriorityCaravanFood;

	[Unsaved(false)]
	private float cachedNutrition = -1f;

	public JoyKindDef JoyKind => joyKind ?? JoyKindDefOf.Gluttonous;

	public bool HumanEdible => (FoodTypeFlags.OmnivoreHuman & foodType) != 0;

	public bool IsMeal
	{
		get
		{
			if ((int)preferability >= 7)
			{
				return (int)preferability <= 10;
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
			yield return "Nutrition == 0 but preferability is " + preferability.ToString() + " instead of " + FoodPreferability.NeverForNutrition;
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
			StatCategoryDef category = ((drugCategory != DrugCategory.None) ? StatCategoryDefOf.Drug : StatCategoryDefOf.Basics);
			yield return new StatDrawEntry(category, "Joy".Translate(), joy.ToStringPercent("F0") + " (" + JoyKind.label + ")", "Stat_Thing_Ingestible_Joy_Desc".Translate(), 4751);
		}
		if (HumanEdible && parent.GetStatValueAbstract(StatDefOf.Nutrition) > 0f)
		{
			RoyalTitleDef royalTitleDef = MaxSatisfiedTitle();
			if (royalTitleDef != null)
			{
				yield return new StatDrawEntry(StatCategoryDefOf.Basics, "Stat_Thing_Ingestible_MaxSatisfiedTitle".Translate(), royalTitleDef.GetLabelCapForBothGenders(), "Stat_Thing_Ingestible_MaxSatisfiedTitle_Desc".Translate(), 4752);
			}
		}
		if (drugCategory != DrugCategory.None)
		{
			yield return new StatDrawEntry(StatCategoryDefOf.Drug, "DrugCategory".Translate().CapitalizeFirst(), drugCategory.GetLabel().CapitalizeFirst(), "Stat_Thing_Drug_Category_Desc".Translate(), 2485);
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

	private bool TryGetOutcomeDoer<T>(out T doer) where T : IngestionOutcomeDoer
	{
		doer = null;
		if (outcomeDoers == null)
		{
			return false;
		}
		foreach (IngestionOutcomeDoer outcomeDoer in outcomeDoers)
		{
			if (outcomeDoer is T val)
			{
				doer = val;
				return true;
			}
		}
		return false;
	}
}
