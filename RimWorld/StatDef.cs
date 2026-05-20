using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld;

public class StatDef : Def
{
	private static HashSet<StatDef> mutableStats;

	public StatCategoryDef category;

	public Type workerClass = typeof(StatWorker);

	public string labelForFullStatList;

	public bool forInformationOnly;

	[MustTranslate]
	private string offsetLabel;

	public float hideAtValue = -2.1474836E+09f;

	public bool alwaysHide;

	public bool showNonAbstract = true;

	public bool showIfUndefined = true;

	public bool showOnPawns = true;

	public bool showOnHumanlikes = true;

	public bool showOnNonWildManHumanlikes = true;

	public bool showOnAnimals = true;

	public bool showOnMechanoids = true;

	public bool showOnNonWorkTables = true;

	public bool showOnEntities = true;

	public bool showOnDrones = true;

	public bool showOnNonPowerPlants = true;

	public bool showOnDefaultValue = true;

	public bool showOnUnhaulables = true;

	public bool showOnUntradeables = true;

	public List<string> showIfModsLoaded;

	public List<string> showIfModsLoadedAny;

	public List<HediffDef> showIfHediffsPresent;

	public bool neverDisabled;

	public bool showZeroBaseValue;

	public bool showOnSlavesOnly;

	public bool showOnPlayerMechanoids;

	public DevelopmentalStage showDevelopmentalStageFilter = DevelopmentalStage.Baby | DevelopmentalStage.Child | DevelopmentalStage.Adult;

	public bool hideInClassicMode;

	public List<PawnKindDef> showOnPawnKind;

	public bool overridesHideStats;

	public int displayPriorityInCategory;

	public ToStringNumberSense toStringNumberSense = ToStringNumberSense.Absolute;

	public ToStringStyle toStringStyle;

	private ToStringStyle? toStringStyleUnfinalized;

	[MustTranslate]
	public string formatString;

	[MustTranslate]
	public string formatStringUnfinalized;

	public bool finalizeEquippedStatOffset = true;

	[MustTranslate]
	public string statFactorsExplanationHeader;

	public float defaultBaseValue = 1f;

	public List<SkillNeed> skillNeedOffsets;

	public float noSkillOffset;

	public List<PawnCapacityOffset> capacityOffsets;

	public List<StatDef> statFactors;

	public bool applyFactorsIfNegative = true;

	public List<SkillNeed> skillNeedFactors;

	public float noSkillFactor = 1f;

	public List<PawnCapacityFactor> capacityFactors;

	public SimpleCurve postProcessCurve;

	public List<StatDef> postProcessStatFactors;

	public float minValue = -9999999f;

	public float maxValue = 9999999f;

	public float valueIfMissing;

	public bool roundValue;

	public float roundToFiveOver = float.MaxValue;

	public bool minifiedThingInherits;

	public bool supressDisabledError;

	public bool cacheable;

	public bool displayMaxWhenAboveOrEqual;

	public bool scenarioRandomizable;

	public SkillDef disableIfSkillDisabled;

	public List<StatPart> parts;

	[Unsaved(false)]
	private StatWorker workerInt;

	[Unsaved(false)]
	public bool immutable;

	public StatWorker Worker
	{
		get
		{
			if (workerInt == null)
			{
				if (parts != null)
				{
					for (int i = 0; i < parts.Count; i++)
					{
						parts[i].parentStat = this;
					}
				}
				workerInt = (StatWorker)Activator.CreateInstance(workerClass);
				workerInt.InitSetStat(this);
			}
			return workerInt;
		}
	}

	public ToStringStyle ToStringStyleUnfinalized
	{
		get
		{
			if (!toStringStyleUnfinalized.HasValue)
			{
				return toStringStyle;
			}
			return toStringStyleUnfinalized.Value;
		}
	}

	public string LabelForFullStatList
	{
		get
		{
			if (!labelForFullStatList.NullOrEmpty())
			{
				return labelForFullStatList;
			}
			return label;
		}
	}

	public string LabelForFullStatListCap => LabelForFullStatList.CapitalizeFirst(this);

	public string OffsetLabel => offsetLabel ?? label;

	public string OffsetLabelCap => OffsetLabel.CapitalizeFirst();

	public override IEnumerable<string> ConfigErrors()
	{
		foreach (string item in base.ConfigErrors())
		{
			yield return item;
		}
		if (capacityFactors != null)
		{
			foreach (PawnCapacityFactor capacityFactor in capacityFactors)
			{
				if (capacityFactor.weight > 1f)
				{
					yield return defName + " has activity factor with weight > 1";
				}
			}
		}
		if (parts == null)
		{
			yield break;
		}
		for (int i = 0; i < parts.Count; i++)
		{
			foreach (string item2 in parts[i].ConfigErrors())
			{
				yield return defName + " has error in StatPart " + parts[i].ToString() + ": " + item2;
			}
		}
	}

	public string ValueToString(float val, ToStringNumberSense numberSense = ToStringNumberSense.Absolute, bool finalized = true)
	{
		return Worker.ValueToString(val, finalized, numberSense);
	}

	public static StatDef Named(string defName)
	{
		return DefDatabase<StatDef>.GetNamed(defName);
	}

	public override void PostLoad()
	{
		base.PostLoad();
		if (parts != null)
		{
			List<StatPart> partsCopy = parts.ToList();
			parts.SortBy((StatPart x) => 0f - x.priority, (StatPart x) => partsCopy.IndexOf(x));
		}
	}

	public T GetStatPart<T>() where T : StatPart
	{
		return parts.OfType<T>().FirstOrDefault();
	}

	public bool CanShowWithLoadedMods()
	{
		if (!showIfModsLoaded.NullOrEmpty())
		{
			for (int i = 0; i < showIfModsLoaded.Count; i++)
			{
				if (!ModsConfig.IsActive(showIfModsLoaded[i]))
				{
					return false;
				}
			}
		}
		if (!showIfModsLoadedAny.NullOrEmpty())
		{
			bool result = false;
			for (int j = 0; j < showIfModsLoadedAny.Count; j++)
			{
				if (ModsConfig.IsActive(showIfModsLoadedAny[j]))
				{
					result = true;
					break;
				}
			}
			return result;
		}
		return true;
	}

	private static void PopulateMutableStats()
	{
		mutableStats = new HashSet<StatDef>();
		foreach (TraitDef item in DefDatabase<TraitDef>.AllDefsListForReading)
		{
			foreach (TraitDegreeData degreeData in item.degreeDatas)
			{
				AddStatsFromModifiers(degreeData.statOffsets);
				AddStatsFromModifiers(degreeData.statFactors);
			}
		}
		foreach (HediffDef item2 in DefDatabase<HediffDef>.AllDefsListForReading)
		{
			if (item2.stages == null)
			{
				continue;
			}
			foreach (HediffStage stage in item2.stages)
			{
				AddStatsFromModifiers(stage.statOffsets);
				AddStatsFromModifiers(stage.statFactors);
				if (stage.statOffsetEffectMultiplier != null)
				{
					mutableStats.Add(stage.statOffsetEffectMultiplier);
				}
				if (stage.statFactorEffectMultiplier != null)
				{
					mutableStats.Add(stage.statFactorEffectMultiplier);
				}
				if (stage.capacityFactorEffectMultiplier != null)
				{
					mutableStats.Add(stage.capacityFactorEffectMultiplier);
				}
			}
		}
		foreach (PreceptDef item3 in DefDatabase<PreceptDef>.AllDefsListForReading)
		{
			AddStatsFromModifiers(item3.statOffsets);
			AddStatsFromModifiers(item3.statFactors);
			if (item3.roleEffects != null)
			{
				foreach (RoleEffect roleEffect in item3.roleEffects)
				{
					if (roleEffect is RoleEffect_PawnStatOffset roleEffect_PawnStatOffset)
					{
						mutableStats.Add(roleEffect_PawnStatOffset.statDef);
					}
					if (roleEffect is RoleEffect_PawnStatFactor roleEffect_PawnStatFactor)
					{
						mutableStats.Add(roleEffect_PawnStatFactor.statDef);
					}
				}
			}
			if (item3.abilityStatFactors == null)
			{
				continue;
			}
			foreach (AbilityStatModifiers abilityStatFactor in item3.abilityStatFactors)
			{
				AddStatsFromModifiers(abilityStatFactor.modifiers);
			}
		}
		foreach (GeneDef item4 in DefDatabase<GeneDef>.AllDefsListForReading)
		{
			AddStatsFromModifiers(item4.statOffsets);
			AddStatsFromModifiers(item4.statFactors);
			if (item4.conditionalStatAffecters == null)
			{
				continue;
			}
			foreach (ConditionalStatAffecter conditionalStatAffecter in item4.conditionalStatAffecters)
			{
				AddStatsFromModifiers(conditionalStatAffecter.statOffsets);
				AddStatsFromModifiers(conditionalStatAffecter.statFactors);
			}
		}
		foreach (ThingDef item5 in DefDatabase<ThingDef>.AllDefsListForReading)
		{
			AddStatsFromModifiers(item5.equippedStatOffsets);
			if (!item5.HasAssignableCompFrom(typeof(CompFacility)))
			{
				continue;
			}
			CompProperties_Facility compProperties = item5.GetCompProperties<CompProperties_Facility>();
			if (compProperties?.statOffsets != null)
			{
				AddStatsFromModifiers(compProperties.statOffsets);
			}
			if (!(compProperties is CompProperties_FacilityQualityBased compProperties_FacilityQualityBased))
			{
				continue;
			}
			foreach (StatDef key in compProperties_FacilityQualityBased.statOffsetsPerQuality.Keys)
			{
				mutableStats.Add(key);
			}
		}
		foreach (WeaponTraitDef item6 in DefDatabase<WeaponTraitDef>.AllDefsListForReading)
		{
			AddStatsFromModifiers(item6.equippedStatOffsets);
		}
		foreach (LifeStageDef item7 in DefDatabase<LifeStageDef>.AllDefsListForReading)
		{
			AddStatsFromModifiers(item7.statFactors);
			AddStatsFromModifiers(item7.statOffsets);
		}
		foreach (InspirationDef item8 in DefDatabase<InspirationDef>.AllDefsListForReading)
		{
			AddStatsFromModifiers(item8.statOffsets);
			AddStatsFromModifiers(item8.statFactors);
		}
		static void AddStatsFromModifiers(List<StatModifier> mods)
		{
			if (mods != null)
			{
				mutableStats.AddRange(mods.Select((StatModifier mod) => mod.stat));
			}
		}
	}

	public bool IsImmutable()
	{
		if (workerClass != typeof(StatWorker))
		{
			return false;
		}
		if (!skillNeedOffsets.NullOrEmpty() || !skillNeedFactors.NullOrEmpty())
		{
			return false;
		}
		if (!capacityOffsets.NullOrEmpty() || !capacityFactors.NullOrEmpty())
		{
			return false;
		}
		if (!statFactors.NullOrEmpty())
		{
			return false;
		}
		if (!parts.NullOrEmpty())
		{
			return false;
		}
		if (!postProcessStatFactors.NullOrEmpty())
		{
			return false;
		}
		if (mutableStats.Contains(this))
		{
			return false;
		}
		return true;
	}

	public static void SetImmutability()
	{
		PopulateMutableStats();
		foreach (StatDef item in DefDatabase<StatDef>.AllDefsListForReading)
		{
			item.immutable = item.IsImmutable();
			item.Worker.SetCacheability(item.immutable);
		}
	}

	public static void ResetStaticData()
	{
		mutableStats = null;
		foreach (StatDef item in DefDatabase<StatDef>.AllDefsListForReading)
		{
			item.immutable = false;
			item.Worker.DeleteStatCache();
		}
	}
}
