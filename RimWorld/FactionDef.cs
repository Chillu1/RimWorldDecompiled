using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class FactionDef : Def
	{
		public bool isPlayer;

		public RulePackDef factionNameMaker;

		public RulePackDef settlementNameMaker;

		public RulePackDef playerInitialSettlementNameMaker;

		[MustTranslate]
		public string fixedName;

		public bool humanlikeFaction = true;

		public bool hidden;

		public float listOrderPriority;

		public List<PawnGroupMaker> pawnGroupMakers;

		public SimpleCurve raidCommonalityFromPointsCurve;

		public bool autoFlee = true;

		public FloatRange attackersDownPercentageRangeForAutoFlee = new FloatRange(0.4f, 0.7f);

		public bool canSiege;

		public bool canStageAttacks;

		public bool canUseAvoidGrid = true;

		public float earliestRaidDays;

		public FloatRange allowedArrivalTemperatureRange = new FloatRange(-1000f, 1000f);

		public PawnKindDef basicMemberKind;

		public List<ResearchProjectTagDef> startingResearchTags;

		public List<ResearchProjectTagDef> startingTechprintsResearchTags;

		[NoTranslate]
		public List<string> recipePrerequisiteTags;

		public bool rescueesCanJoin;

		[MustTranslate]
		public string pawnSingular = "member";

		[MustTranslate]
		public string pawnsPlural = "members";

		public string leaderTitle = "leader";

		public string leaderTitleFemale;

		public string royalFavorLabel;

		public List<PawnKindDef> fixedLeaderKinds;

		public bool leaderForceGenerateNewPawn;

		public float forageabilityFactor = 1f;

		public SimpleCurve maxPawnCostPerTotalPointsCurve;

		public List<string> royalTitleTags;

		public string categoryTag;

		public bool hostileToFactionlessHumanlikes;

		public int requiredCountAtGameStart;

		public int maxCountAtGameStart = 9999;

		public bool canMakeRandomly;

		public float settlementGenerationWeight;

		public RulePackDef pawnNameMaker;

		public RulePackDef pawnNameMakerFemale;

		public TechLevel techLevel;

		[NoTranslate]
		public List<BackstoryCategoryFilter> backstoryFilters;

		[NoTranslate]
		private List<string> backstoryCategories;

		[NoTranslate]
		public List<string> hairTags = new List<string>();

		public ThingFilter apparelStuffFilter;

		public List<TraderKindDef> caravanTraderKinds = new List<TraderKindDef>();

		public List<TraderKindDef> visitorTraderKinds = new List<TraderKindDef>();

		public List<TraderKindDef> baseTraderKinds = new List<TraderKindDef>();

		public float geneticVariance = 1f;

		public IntRange startingGoodwill = IntRange.zero;

		public bool mustStartOneEnemy;

		public IntRange naturalColonyGoodwill = IntRange.zero;

		public float goodwillDailyGain;

		public float goodwillDailyFall;

		public bool permanentEnemy;

		public bool permanentEnemyToEveryoneExceptPlayer;

		[NoTranslate]
		public string settlementTexturePath;

		[NoTranslate]
		public string factionIconPath;

		public List<Color> colorSpectrum;

		public List<PawnRelationDef> royalTitleInheritanceRelations;

		public Type royalTitleInheritanceWorkerClass;

		public List<RoyalImplantRule> royalImplantRules;

		public RoyalTitleDef minTitleForBladelinkWeapons;

		public string renounceTitleMessage;

		[Unsaved(false)]
		private Texture2D factionIcon;

		[Unsaved(false)]
		private Texture2D settlementTexture;

		[Unsaved(false)]
		private List<RoyalTitleDef> royalTitlesAwardableInSeniorityOrderForReading;

		[Unsaved(false)]
		private List<RoyalTitleDef> royalTitlesAllInSeniorityOrderForReading;

		[Unsaved(false)]
		private RoyalTitleInheritanceWorker royalTitleInheritanceWorker;

		public List<RoyalTitleDef> RoyalTitlesAwardableInSeniorityOrderForReading
		{
			get
			{
				if (royalTitlesAwardableInSeniorityOrderForReading == null)
				{
					royalTitlesAwardableInSeniorityOrderForReading = new List<RoyalTitleDef>();
					if (royalTitleTags != null && ModLister.RoyaltyInstalled)
					{
						foreach (RoyalTitleDef item in DefDatabase<RoyalTitleDef>.AllDefsListForReading)
						{
							if (item.Awardable && item.tags.SharesElementWith(royalTitleTags))
							{
								royalTitlesAwardableInSeniorityOrderForReading.Add(item);
							}
						}
						royalTitlesAwardableInSeniorityOrderForReading.SortBy((RoyalTitleDef t) => t.seniority);
					}
				}
				return royalTitlesAwardableInSeniorityOrderForReading;
			}
		}

		public List<RoyalTitleDef> RoyalTitlesAllInSeniorityOrderForReading
		{
			get
			{
				if (royalTitlesAllInSeniorityOrderForReading == null)
				{
					royalTitlesAllInSeniorityOrderForReading = new List<RoyalTitleDef>();
					if (royalTitleTags != null && ModLister.RoyaltyInstalled)
					{
						foreach (RoyalTitleDef item in DefDatabase<RoyalTitleDef>.AllDefsListForReading)
						{
							if (item.tags.SharesElementWith(royalTitleTags))
							{
								royalTitlesAllInSeniorityOrderForReading.Add(item);
							}
						}
						royalTitlesAllInSeniorityOrderForReading.SortBy((RoyalTitleDef t) => t.seniority);
					}
				}
				return royalTitlesAllInSeniorityOrderForReading;
			}
		}

		public RoyalTitleInheritanceWorker RoyalTitleInheritanceWorker
		{
			get
			{
				if (royalTitleInheritanceWorker == null && royalTitleInheritanceWorkerClass != null)
				{
					royalTitleInheritanceWorker = (RoyalTitleInheritanceWorker)Activator.CreateInstance(royalTitleInheritanceWorkerClass);
				}
				return royalTitleInheritanceWorker;
			}
		}

		public bool CanEverBeNonHostile => !permanentEnemy;

		public Texture2D SettlementTexture
		{
			get
			{
				if (settlementTexture == null)
				{
					if (!settlementTexturePath.NullOrEmpty())
					{
						settlementTexture = ContentFinder<Texture2D>.Get(settlementTexturePath);
					}
					else
					{
						settlementTexture = BaseContent.BadTex;
					}
				}
				return settlementTexture;
			}
		}

		public Texture2D FactionIcon
		{
			get
			{
				if (factionIcon == null)
				{
					if (!factionIconPath.NullOrEmpty())
					{
						factionIcon = ContentFinder<Texture2D>.Get(factionIconPath);
					}
					else
					{
						factionIcon = BaseContent.BadTex;
					}
				}
				return factionIcon;
			}
		}

		public bool HasRoyalTitles => RoyalTitlesAwardableInSeniorityOrderForReading.Count > 0;

		public float MinPointsToGeneratePawnGroup(PawnGroupKindDef groupKind)
		{
			if (pawnGroupMakers == null)
			{
				return 0f;
			}
			IEnumerable<PawnGroupMaker> source = pawnGroupMakers.Where((PawnGroupMaker x) => x.kindDef == groupKind);
			if (!source.Any())
			{
				return 0f;
			}
			return source.Min((PawnGroupMaker pgm) => pgm.MinPointsToGenerateAnything);
		}

		public bool CanUseStuffForApparel(ThingDef stuffDef)
		{
			if (apparelStuffFilter == null)
			{
				return true;
			}
			return apparelStuffFilter.Allows(stuffDef);
		}

		public float RaidCommonalityFromPoints(float points)
		{
			if (points < 0f || raidCommonalityFromPointsCurve == null)
			{
				return 1f;
			}
			return raidCommonalityFromPointsCurve.Evaluate(points);
		}

		public override void ResolveReferences()
		{
			base.ResolveReferences();
			if (apparelStuffFilter != null)
			{
				apparelStuffFilter.ResolveReferences();
			}
		}

		public override void PostLoad()
		{
			if (backstoryCategories != null && backstoryCategories.Count > 0)
			{
				if (backstoryFilters == null)
				{
					backstoryFilters = new List<BackstoryCategoryFilter>();
				}
				backstoryFilters.Add(new BackstoryCategoryFilter
				{
					categories = backstoryCategories
				});
			}
		}

		public override IEnumerable<string> ConfigErrors()
		{
			foreach (string item in base.ConfigErrors())
			{
				yield return item;
			}
			if (pawnGroupMakers != null && maxPawnCostPerTotalPointsCurve == null)
			{
				yield return "has pawnGroupMakers but missing maxPawnCostPerTotalPointsCurve";
			}
			if (!isPlayer && factionNameMaker == null && fixedName == null)
			{
				yield return "FactionTypeDef " + defName + " lacks a factionNameMaker and a fixedName.";
			}
			if (techLevel == TechLevel.Undefined)
			{
				yield return defName + " has no tech level.";
			}
			if (humanlikeFaction)
			{
				if (backstoryFilters.NullOrEmpty())
				{
					yield return defName + " is humanlikeFaction but has no backstory categories.";
				}
				if (hairTags.Count == 0)
				{
					yield return defName + " is humanlikeFaction but has no hairTags.";
				}
			}
			if (isPlayer)
			{
				if (settlementNameMaker == null)
				{
					yield return "isPlayer is true but settlementNameMaker is null";
				}
				if (factionNameMaker == null)
				{
					yield return "isPlayer is true but factionNameMaker is null";
				}
				if (playerInitialSettlementNameMaker == null)
				{
					yield return "isPlayer is true but playerInitialSettlementNameMaker is null";
				}
			}
			if (permanentEnemy)
			{
				if (mustStartOneEnemy)
				{
					yield return "permanentEnemy has mustStartOneEnemy = true, which is redundant";
				}
				if (goodwillDailyFall != 0f || goodwillDailyGain != 0f)
				{
					yield return "permanentEnemy has a goodwillDailyFall or goodwillDailyGain";
				}
				if (startingGoodwill != IntRange.zero)
				{
					yield return "permanentEnemy has a startingGoodwill defined";
				}
				if (naturalColonyGoodwill != IntRange.zero)
				{
					yield return "permanentEnemy has a naturalColonyGoodwill defined";
				}
			}
		}

		public static FactionDef Named(string defName)
		{
			return DefDatabase<FactionDef>.GetNamed(defName);
		}

		public RulePackDef GetNameMaker(Gender gender)
		{
			if (gender == Gender.Female && pawnNameMakerFemale != null)
			{
				return pawnNameMakerFemale;
			}
			return pawnNameMaker;
		}
	}
}
