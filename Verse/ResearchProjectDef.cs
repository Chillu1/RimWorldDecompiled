using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse.Grammar;

namespace Verse;

public class ResearchProjectDef : Def
{
	public float baseCost;

	public List<ResearchProjectDef> prerequisites;

	public List<ResearchProjectDef> hiddenPrerequisites;

	public TechLevel techLevel;

	public List<ResearchProjectDef> requiredByThis;

	private List<ResearchMod> researchMods;

	public ThingDef requiredResearchBuilding;

	public List<ThingDef> requiredResearchFacilities;

	public List<ResearchProjectTagDef> tags;

	public ResearchTabDef tab;

	public float researchViewX = 1f;

	public float researchViewY = 1f;

	[MustTranslate]
	public string discoveredLetterTitle;

	[MustTranslate]
	public string discoveredLetterText;

	public DifficultyConditionConfig discoveredLetterDisabledWhen = new DifficultyConditionConfig();

	public int techprintCount;

	public float techprintCommonality = 1f;

	public float techprintMarketValue = 1000f;

	[NoTranslate]
	public List<string> heldByFactionCategoryTags;

	public DifficultyConditionConfig hideWhen = new DifficultyConditionConfig();

	public bool requiresMechanitor;

	public List<ThingDef> requiredAnalyzed;

	public bool recalculatePower;

	public KnowledgeCategoryDef knowledgeCategory;

	public float knowledgeCost;

	public ConceptDef teachConcept;

	public RulePack generalRules;

	public bool requireGravEngineInspected;

	[MustTranslate]
	public List<string> customUnlockTexts;

	[Unsaved(false)]
	private float x = 1f;

	[Unsaved(false)]
	private float y = 1f;

	[Unsaved(false)]
	private bool positionModified;

	[Unsaved(false)]
	private ThingDef cachedTechprint;

	[Unsaved(false)]
	private List<Def> cachedUnlockedDefs;

	[Unsaved(false)]
	private List<Dialog_InfoCard.Hyperlink> cachedHyperlinks;

	[Unsaved(false)]
	private string cachedDescription;

	public const TechLevel MaxEffectiveTechLevel = TechLevel.Industrial;

	private const float ResearchCostFactorPerTechLevelDiff = 0.5f;

	public float ResearchViewX => x;

	public float ResearchViewY => y;

	public float CostApparent => Cost * CostFactor(Faction.OfPlayer.def.techLevel);

	public float ProgressReal => Find.ResearchManager.GetProgress(this);

	public float ProgressApparent => ProgressReal * CostFactor(Faction.OfPlayer.def.techLevel);

	public string ProgressApparentString
	{
		get
		{
			string text = ((baseCost > 0f) ? "F0" : "F2");
			return ProgressApparent.ToString(text);
		}
	}

	public float ProgressPercent => Mathf.Clamp01(ProgressReal / Cost);

	public bool IsFinished => ProgressReal >= Cost;

	public float Cost
	{
		get
		{
			if (!(baseCost > 0f))
			{
				return knowledgeCost;
			}
			return baseCost;
		}
	}

	public override TaggedString LabelCap
	{
		get
		{
			if (label.NullOrEmpty())
			{
				return null;
			}
			if (cachedLabelCap.NullOrEmpty())
			{
				cachedLabelCap = label.CapitalizeFirst();
			}
			return cachedLabelCap;
		}
	}

	public string Description => description;

	public bool IsHidden
	{
		get
		{
			if (!IsFinished && ModsConfig.AnomalyActive)
			{
				return Find.EntityCodex.Hidden(this);
			}
			return false;
		}
	}

	public bool CanStartNow
	{
		get
		{
			if (!IsFinished && PrerequisitesCompleted && TechprintRequirementMet && (requiredResearchBuilding == null || PlayerHasAnyAppropriateResearchBench) && PlayerMechanitorRequirementMet && AnalyzedThingsRequirementsMet && !IsHidden)
			{
				return InspectionRequirementsMet;
			}
			return false;
		}
	}

	public bool PrerequisitesCompleted
	{
		get
		{
			if (prerequisites != null)
			{
				for (int i = 0; i < prerequisites.Count; i++)
				{
					if (!prerequisites[i].IsFinished)
					{
						return false;
					}
				}
			}
			if (hiddenPrerequisites != null)
			{
				for (int j = 0; j < hiddenPrerequisites.Count; j++)
				{
					if (!hiddenPrerequisites[j].IsFinished)
					{
						return false;
					}
				}
			}
			return true;
		}
	}

	public int TechprintCount
	{
		get
		{
			if (!ModLister.RoyaltyInstalled)
			{
				return 0;
			}
			return techprintCount;
		}
	}

	public int TechprintsApplied => Find.ResearchManager.GetTechprints(this);

	public bool TechprintRequirementMet
	{
		get
		{
			if (TechprintCount > 0 && Find.ResearchManager.GetTechprints(this) < TechprintCount)
			{
				return false;
			}
			return true;
		}
	}

	public ThingDef Techprint
	{
		get
		{
			if (TechprintCount <= 0)
			{
				return null;
			}
			if (cachedTechprint == null)
			{
				cachedTechprint = DefDatabase<ThingDef>.AllDefs.FirstOrDefault(delegate(ThingDef x)
				{
					CompProperties_Techprint compProperties = x.GetCompProperties<CompProperties_Techprint>();
					return compProperties != null && compProperties.project == this;
				});
				if (cachedTechprint == null)
				{
					Log.ErrorOnce("Could not find techprint for research project " + this, shortHash ^ 0x340C745A);
				}
			}
			return cachedTechprint;
		}
	}

	public int RequiredAnalyzedThingCount => requiredAnalyzed?.Count ?? 0;

	public int AnalyzedThingsCompleted
	{
		get
		{
			if (requiredAnalyzed.NullOrEmpty())
			{
				return 0;
			}
			int num = 0;
			for (int i = 0; i < requiredAnalyzed.Count; i++)
			{
				Find.AnalysisManager.TryGetAnalysisProgress(requiredAnalyzed[i].GetCompProperties<CompProperties_CompAnalyzableUnlockResearch>()?.analysisID ?? (-1), out var details);
				if (details != null && details.Satisfied)
				{
					num++;
				}
			}
			return num;
		}
	}

	public bool AnalyzedThingsRequirementsMet
	{
		get
		{
			if (RequiredAnalyzedThingCount > 0)
			{
				return AnalyzedThingsCompleted >= RequiredAnalyzedThingCount;
			}
			return true;
		}
	}

	public bool InspectionRequirementsMet
	{
		get
		{
			if (ModsConfig.OdysseyActive && requireGravEngineInspected)
			{
				return Find.ResearchManager.gravEngineInspected;
			}
			return true;
		}
	}

	public List<Def> UnlockedDefs
	{
		get
		{
			if (cachedUnlockedDefs == null)
			{
				cachedUnlockedDefs = (from x in (from x in DefDatabase<RecipeDef>.AllDefs.Where((RecipeDef x) => x.researchPrerequisite == this || (x.researchPrerequisites != null && x.researchPrerequisites.Contains(this))).SelectMany((RecipeDef x) => ((IEnumerable<ThingDefCountClass>)x.products).Select((Func<ThingDefCountClass, Def>)((ThingDefCountClass y) => y.thingDef)))
						orderby x.label
						select x).Concat(from x in DefDatabase<ThingDef>.AllDefs
						where x.researchPrerequisites != null && x.researchPrerequisites.Contains(this)
						orderby x.label
						select x).Concat(from x in DefDatabase<ThingDef>.AllDefs
						where x.plant != null && x.plant.sowResearchPrerequisites != null && x.plant.sowResearchPrerequisites.Contains(this)
						orderby x.label
						select x).Concat(from x in DefDatabase<TerrainDef>.AllDefs
						where x.researchPrerequisites != null && x.researchPrerequisites.Contains(this)
						orderby x.label
						select x)
						.Concat(from x in DefDatabase<RecipeDef>.AllDefs
							where x.IsSurgery && x.researchPrerequisites != null && x.researchPrerequisites.Contains(this)
							orderby x.label
							select x)
						.Concat(DefDatabase<PsychicRitualDef>.AllDefs.Where((PsychicRitualDef x) => x.researchPrerequisite == this))
					orderby x.label
					select x).Distinct().ToList();
			}
			return cachedUnlockedDefs;
		}
	}

	public List<Dialog_InfoCard.Hyperlink> InfoCardHyperlinks
	{
		get
		{
			if (cachedHyperlinks == null)
			{
				cachedHyperlinks = new List<Dialog_InfoCard.Hyperlink>();
				List<Def> unlockedDefs = UnlockedDefs;
				if (unlockedDefs != null)
				{
					for (int i = 0; i < unlockedDefs.Count; i++)
					{
						cachedHyperlinks.Add(new Dialog_InfoCard.Hyperlink(unlockedDefs[i]));
					}
				}
			}
			return cachedHyperlinks;
		}
	}

	public bool PlayerHasAnyAppropriateResearchBench
	{
		get
		{
			List<Map> maps = Find.Maps;
			for (int i = 0; i < maps.Count; i++)
			{
				List<Building> allBuildingsColonist = maps[i].listerBuildings.allBuildingsColonist;
				for (int j = 0; j < allBuildingsColonist.Count; j++)
				{
					if (allBuildingsColonist[j] is Building_ResearchBench bench && CanBeResearchedAt(bench, ignoreResearchBenchPowerStatus: true))
					{
						return true;
					}
				}
			}
			return false;
		}
	}

	public bool PlayerMechanitorRequirementMet
	{
		get
		{
			if (!ModsConfig.BiotechActive || !requiresMechanitor)
			{
				return true;
			}
			return MechanitorUtility.AnyMechanitorInPlayerFaction();
		}
	}

	public override void ResolveReferences()
	{
		base.ResolveReferences();
		if (tab == null)
		{
			tab = ResearchTabDefOf.Main;
		}
	}

	public override IEnumerable<string> ConfigErrors()
	{
		foreach (string item in base.ConfigErrors())
		{
			yield return item;
		}
		if (techLevel == TechLevel.Undefined && knowledgeCategory == null)
		{
			yield return "techLevel is Undefined";
		}
		if (ResearchViewX < 0f || ResearchViewY < 0f)
		{
			yield return "researchViewX and/or researchViewY not set";
		}
		if (techprintCount == 0 && !heldByFactionCategoryTags.NullOrEmpty())
		{
			yield return "requires no techprints but has heldByFactionCategoryTags.";
		}
		if (techprintCount > 0 && heldByFactionCategoryTags.NullOrEmpty())
		{
			yield return "requires techprints but has no heldByFactionCategoryTags.";
		}
		List<ResearchProjectDef> rpDefs = DefDatabase<ResearchProjectDef>.AllDefsListForReading;
		for (int i = 0; i < rpDefs.Count; i++)
		{
			if (rpDefs[i] != this && rpDefs[i].tab == tab && rpDefs[i].ResearchViewX == ResearchViewX && rpDefs[i].ResearchViewY == ResearchViewY)
			{
				yield return "same research view coords and tab as " + rpDefs[i]?.ToString() + ": " + ResearchViewX + ", " + ResearchViewY + "(" + tab?.ToString() + ")";
			}
		}
		if (!ModLister.RoyaltyInstalled && techprintCount > 0)
		{
			yield return "defines techprintCount, but techprints are a Royalty-specific game system and only work with Royalty installed.";
		}
		if (!requiredAnalyzed.NullOrEmpty())
		{
			foreach (ThingDef item2 in requiredAnalyzed)
			{
				if (!item2.HasAssignableCompFrom(typeof(CompAnalyzable)))
				{
					yield return "requires analyzing " + item2.label + " but " + item2.label + " cannot be analyzed.";
				}
			}
		}
		if (knowledgeCost > 0f && knowledgeCategory == null)
		{
			yield return "has knowledge cost, but no knowledge category.";
		}
		if (baseCost > 0f && knowledgeCost > 0f)
		{
			yield return "baseCost and knowledgeCost are both defined. This is unsupported.";
		}
	}

	public override void PostLoad()
	{
		base.PostLoad();
		if (!ModLister.RoyaltyInstalled)
		{
			techprintCount = 0;
		}
		if (!ModLister.BiotechInstalled)
		{
			requiredAnalyzed = null;
		}
	}

	public string GetTip()
	{
		string text = LabelCap.Colorize(ColoredText.TipSectionTitleColor);
		if (ProgressReal > 0f && !IsFinished)
		{
			text = text + " (" + ProgressApparentString + " / " + CostApparent.ToString("F0") + ")";
		}
		if (cachedDescription == null)
		{
			cachedDescription = Description;
			if (TechprintCount > 0)
			{
				cachedDescription = cachedDescription + "\n\n" + ("RequiredTechprintTip".Translate() + ": " + Techprint.LabelCap).ToString();
			}
			if (RequiredAnalyzedThingCount > 0)
			{
				cachedDescription = cachedDescription + "\n\n" + "StudyRequirementTip".Translate().ToString() + ": " + requiredAnalyzed.Select((ThingDef t) => t.label).ToCommaList(useAnd: true).CapitalizeFirst();
			}
			if (modContentPack != null && !modContentPack.IsCoreMod)
			{
				Color color = (modContentPack.IsOfficialMod ? ModLister.GetExpansionWithIdentifier(modContentPack.PackageId.ToLower()).primaryColor : ColoredText.SubtleGrayColor);
				cachedDescription = cachedDescription + "\n\n" + ("Stat_Source_Label".Translate().ToString() + ": " + modContentPack.Name).Colorize(color);
			}
		}
		return text + "\n" + cachedDescription;
	}

	public float CostFactor(TechLevel researcherTechLevel)
	{
		if (this.techLevel == TechLevel.Undefined)
		{
			return 1f;
		}
		TechLevel techLevel = (TechLevel)Mathf.Min((int)this.techLevel, 4);
		if ((int)researcherTechLevel >= (int)techLevel)
		{
			return 1f;
		}
		int num = techLevel - researcherTechLevel;
		return 1f + (float)num * 0.5f;
	}

	public bool HasTag(ResearchProjectTagDef tag)
	{
		if (tags == null)
		{
			return false;
		}
		return tags.Contains(tag);
	}

	public bool CanBeResearchedAt(Building_ResearchBench bench, bool ignoreResearchBenchPowerStatus)
	{
		if (requiredResearchBuilding != null && bench.def != requiredResearchBuilding)
		{
			return false;
		}
		if (!ignoreResearchBenchPowerStatus)
		{
			CompPowerTrader comp = bench.GetComp<CompPowerTrader>();
			if (comp != null && !comp.PowerOn)
			{
				return false;
			}
		}
		if (!requiredResearchFacilities.NullOrEmpty())
		{
			CompAffectedByFacilities affectedByFacilities = bench.TryGetComp<CompAffectedByFacilities>();
			if (affectedByFacilities == null)
			{
				return false;
			}
			List<Thing> linkedFacilitiesListForReading = affectedByFacilities.LinkedFacilitiesListForReading;
			int i;
			for (i = 0; i < requiredResearchFacilities.Count; i++)
			{
				if (linkedFacilitiesListForReading.Find((Thing x) => x.def == requiredResearchFacilities[i] && affectedByFacilities.IsFacilityActive(x)) == null)
				{
					return false;
				}
			}
		}
		return true;
	}

	public bool AnyOtherVisiblePrerequisitesCompleted(ResearchProjectDef prerequisite)
	{
		if (prerequisites == null)
		{
			return false;
		}
		for (int i = 0; i < prerequisites.Count; i++)
		{
			if (prerequisites[i] != prerequisite && prerequisites[i].IsFinished)
			{
				return true;
			}
		}
		return false;
	}

	public void ReapplyAllMods()
	{
		if (researchMods == null)
		{
			return;
		}
		for (int i = 0; i < researchMods.Count; i++)
		{
			try
			{
				researchMods[i].Apply();
			}
			catch (Exception ex)
			{
				Log.Error("Exception applying research mod for project " + this?.ToString() + ": " + ex);
			}
		}
	}

	public static ResearchProjectDef Named(string defName)
	{
		return DefDatabase<ResearchProjectDef>.GetNamed(defName);
	}

	public static void GenerateNonOverlappingCoordinates()
	{
		foreach (ResearchProjectDef item in DefDatabase<ResearchProjectDef>.AllDefsListForReading)
		{
			item.x = item.researchViewX;
			item.y = item.researchViewY;
		}
		int num = 0;
		while (true)
		{
			bool flag = false;
			foreach (ResearchProjectDef item2 in DefDatabase<ResearchProjectDef>.AllDefsListForReading)
			{
				foreach (ResearchProjectDef item3 in DefDatabase<ResearchProjectDef>.AllDefsListForReading)
				{
					if (item2 == item3 || item2.tab != item3.tab)
					{
						continue;
					}
					bool num2 = Mathf.Abs(item2.x - item3.x) < 0.5f;
					bool flag2 = Mathf.Abs(item2.y - item3.y) < 0.25f;
					if (num2 && flag2)
					{
						flag = true;
						if (item2.x <= item3.x)
						{
							item2.x -= 0.1f;
							item3.x += 0.1f;
						}
						else
						{
							item2.x += 0.1f;
							item3.x -= 0.1f;
						}
						if (item2.y <= item3.y)
						{
							item2.y -= 0.1f;
							item3.y += 0.1f;
						}
						else
						{
							item2.y += 0.1f;
							item3.y -= 0.1f;
						}
						item2.x += 0.001f;
						item2.y += 0.001f;
						item3.x -= 0.001f;
						item3.y -= 0.001f;
						ClampInCoordinateLimits(item2);
						ClampInCoordinateLimits(item3);
					}
				}
			}
			if (flag)
			{
				num++;
				if (num > 200)
				{
					Log.Error("Couldn't relax research project coordinates apart after " + 200 + " passes.");
					break;
				}
				continue;
			}
			break;
		}
	}

	private static void ClampInCoordinateLimits(ResearchProjectDef rp)
	{
		if (rp.x < 0f)
		{
			rp.x = 0f;
		}
		if (rp.y < 0f)
		{
			rp.y = 0f;
		}
		if (rp.y > 6.5f)
		{
			rp.y = 6.5f;
		}
	}

	public void Debug_ApplyPositionDelta(Vector2 delta)
	{
		bool num = Mathf.Abs(delta.x) > 0.01f;
		bool flag = Mathf.Abs(delta.y) > 0.01f;
		if (num)
		{
			x += delta.x;
		}
		if (flag)
		{
			y += delta.y;
		}
		if (num || flag)
		{
			positionModified = true;
		}
	}

	public void Debug_SnapPositionData()
	{
		x = Mathf.Round(x * 2f) / 2f;
		y = Mathf.Round(y * 10f) / 10f;
		ClampInCoordinateLimits(this);
	}

	public bool Debug_IsPositionModified()
	{
		return positionModified;
	}
}
