using System;
using System.Collections.Generic;
using RimWorld;
using UnityEngine;

namespace Verse;

public class HediffDef : Def, IRenderNodePropertiesParent
{
	private class ReportStringOverride
	{
		public JobDef jobDef;

		[MustTranslate]
		public string reportString;
	}

	public Type hediffClass = typeof(Hediff);

	public List<HediffCompProperties> comps;

	[MustTranslate]
	public string descriptionShort;

	[NoTranslate]
	public string debugLabelExtra;

	public float initialSeverity = 0.5f;

	public float lethalSeverity = -1f;

	public List<HediffStage> stages;

	public bool tendable;

	public bool isBad = true;

	public ThingDef spawnThingOnRemoved;

	public float chanceToCauseNoPain;

	public bool canApplyDodChanceForCapacityChanges;

	public bool makesSickThought;

	public bool makesAlert = true;

	public NeedDef chemicalNeed;

	public float minSeverity;

	public float maxSeverity = float.MaxValue;

	public bool scenarioCanAdd;

	public List<HediffGiver> hediffGivers;

	public bool cureAllAtOnceIfCuredByItem;

	public TaleDef taleOnVisible;

	public bool recordDownedTale = true;

	public bool everCurableByItem = true;

	public List<string> tags;

	public bool priceImpact;

	public float priceOffset;

	public bool chronic;

	public bool keepOnBodyPartRestoration;

	public bool countsAsAddedPartOrImplant;

	public bool blocksSocialInteraction;

	public bool blocksSleeping;

	[MustTranslate]
	public string overrideTooltip;

	[MustTranslate]
	public string extraTooltip;

	[MustTranslate]
	public string inspectString;

	public bool levelIsQuantity;

	public bool removeOnDeathrestStart;

	public bool preventsCrawling;

	public bool preventsPregnancy;

	public bool preventsLungRot;

	public bool pregnant;

	public bool allowMothballIfLowPriorityWorldPawn;

	public List<string> removeWithTags;

	public List<BodyPartDef> onlyLifeThreateningTo;

	public bool canAffectBionicOrImplant = true;

	public bool alwaysShowSeverity;

	public bool showGizmosOnCorpse;

	public BodyPartDef defaultInstallPart;

	public Color? hairColorOverride;

	public List<HediffInfectionPathway> possiblePathways;

	public List<InfectionPathwayDef> givesInfectionPathways;

	public bool duplicationAllowed = true;

	public bool preventsDeath;

	public List<MeditationFocusDef> allowedMeditationFocusTypes;

	public List<AbilityDef> abilities;

	public bool isInfection;

	public bool forceRemoveOnResurrection;

	public bool organicAddedBodypart;

	public bool deprioritizeHealing;

	public bool clearsEgo;

	public List<Aptitude> aptitudes;

	public SimpleCurve removeOnRedressChanceByDaysCurve = new SimpleCurve
	{
		new CurvePoint(0f, 0f),
		new CurvePoint(1f, 0f)
	};

	public bool removeOnQuestLodgers;

	public List<PawnKindDef> removeOnRedressIfNotOfKind;

	public bool displayWound;

	public float? woundAnchorRange;

	public Color defaultLabelColor = Color.white;

	private List<PawnRenderNodeProperties> renderNodeProperties;

	public Color? skinColorOverride;

	public Color? skinColorTint;

	public float skinColorTintStrength = 0.5f;

	public ShaderTypeDef skinShader;

	public bool forceRenderTreeRecache;

	public InjuryProps injuryProps;

	public AddedBodyPartProps addedPartProps;

	private List<ReportStringOverride> reportStringOverrides;

	[MustTranslate]
	public string labelNoun;

	[MustTranslate]
	public string battleStateLabel;

	[MustTranslate]
	public string labelNounPretty;

	[MustTranslate]
	public string targetPrefix;

	private bool alwaysAllowMothballCached;

	private bool alwaysAllowMothball;

	private string descriptionCached;

	private Dictionary<JobDef, string> reportStringOverridesDict;

	private Hediff concreteExampleInt;

	public bool HasDefinedGraphicProperties
	{
		get
		{
			if (renderNodeProperties.NullOrEmpty())
			{
				return skinShader != null;
			}
			return true;
		}
	}

	public List<PawnRenderNodeProperties> RenderNodeProperties => renderNodeProperties ?? PawnRenderUtility.EmptyRenderNodeProperties;

	public bool IsAddiction => typeof(Hediff_Addiction).IsAssignableFrom(hediffClass);

	public bool AlwaysAllowMothball
	{
		get
		{
			if (!alwaysAllowMothballCached)
			{
				alwaysAllowMothball = true;
				if (comps != null && comps.Count > 0)
				{
					alwaysAllowMothball = false;
				}
				if (stages != null)
				{
					for (int i = 0; i < stages.Count; i++)
					{
						HediffStage hediffStage = stages[i];
						if (hediffStage.deathMtbDays > 0f || (hediffStage.hediffGivers != null && hediffStage.hediffGivers.Count > 0))
						{
							alwaysAllowMothball = false;
						}
					}
				}
				alwaysAllowMothballCached = true;
			}
			return alwaysAllowMothball;
		}
	}

	public Hediff ConcreteExample => concreteExampleInt ?? (concreteExampleInt = HediffMaker.Debug_MakeConcreteExampleHediff(this));

	public string Description
	{
		get
		{
			if (descriptionCached == null)
			{
				if (!descriptionShort.NullOrEmpty())
				{
					descriptionCached = descriptionShort;
				}
				else
				{
					descriptionCached = description;
				}
			}
			return descriptionCached;
		}
	}

	public bool HasComp(Type compClass)
	{
		if (comps != null)
		{
			for (int i = 0; i < comps.Count; i++)
			{
				if (comps[i].compClass == compClass)
				{
					return true;
				}
			}
		}
		return false;
	}

	public HediffCompProperties CompPropsFor(Type compClass)
	{
		if (comps != null)
		{
			for (int i = 0; i < comps.Count; i++)
			{
				if (comps[i].compClass == compClass)
				{
					return comps[i];
				}
			}
		}
		return null;
	}

	public T CompProps<T>() where T : HediffCompProperties
	{
		if (comps != null)
		{
			for (int i = 0; i < comps.Count; i++)
			{
				if (comps[i] is T result)
				{
					return result;
				}
			}
		}
		return null;
	}

	public bool PossibleToDevelopImmunityNaturally()
	{
		HediffCompProperties_Immunizable hediffCompProperties_Immunizable = CompProps<HediffCompProperties_Immunizable>();
		if (hediffCompProperties_Immunizable != null && (hediffCompProperties_Immunizable.immunityPerDayNotSick > 0f || hediffCompProperties_Immunizable.immunityPerDaySick > 0f))
		{
			return true;
		}
		return false;
	}

	public string PrettyTextForPart(BodyPartRecord bodyPart)
	{
		if (labelNounPretty.NullOrEmpty())
		{
			return null;
		}
		return labelNounPretty.Formatted(label, bodyPart.Label);
	}

	public override void ResolveReferences()
	{
		base.ResolveReferences();
		if (comps != null)
		{
			for (int i = 0; i < comps.Count; i++)
			{
				comps[i].ResolveReferences(this);
			}
		}
		if (renderNodeProperties != null)
		{
			for (int j = 0; j < renderNodeProperties.Count; j++)
			{
				renderNodeProperties[j].ResolveReferencesRecursive();
			}
		}
	}

	public int StageAtSeverity(float severity)
	{
		if (stages == null)
		{
			return 0;
		}
		for (int num = stages.Count - 1; num >= 0; num--)
		{
			if (severity >= stages[num].minSeverity)
			{
				return num;
			}
		}
		return 0;
	}

	public int AptitudeFor(SkillDef skill)
	{
		int num = 0;
		if (aptitudes.NullOrEmpty())
		{
			return num;
		}
		for (int i = 0; i < aptitudes.Count; i++)
		{
			if (aptitudes[i].skill == skill)
			{
				num += aptitudes[i].level;
			}
		}
		return num;
	}

	public override IEnumerable<string> ConfigErrors()
	{
		foreach (string item in base.ConfigErrors())
		{
			yield return item;
		}
		if (hediffClass == null)
		{
			yield return "hediffClass is null";
		}
		if (!comps.NullOrEmpty() && !typeof(HediffWithComps).IsAssignableFrom(hediffClass))
		{
			yield return "has comps but hediffClass is not HediffWithComps or subclass thereof";
		}
		if (minSeverity > initialSeverity)
		{
			yield return "minSeverity is greater than initialSeverity";
		}
		if (maxSeverity < initialSeverity)
		{
			yield return "maxSeverity is lower than initialSeverity";
		}
		if (!tendable && HasComp(typeof(HediffComp_TendDuration)))
		{
			yield return "has HediffComp_TendDuration but tendable = false";
		}
		if (string.IsNullOrEmpty(description))
		{
			yield return "Hediff with defName " + defName + " has no description!";
		}
		if (possiblePathways != null)
		{
			for (int i = 0; i < possiblePathways.Count - 1; i++)
			{
				HediffInfectionPathway vector = possiblePathways[i];
				for (int j = i + 1; j < possiblePathways.Count; j++)
				{
					HediffInfectionPathway hediffInfectionPathway = possiblePathways[j];
					if (vector.PathwayDef == hediffInfectionPathway.PathwayDef)
					{
						yield return "Multiple possible infection vectors of type: " + vector.PathwayDef.defName;
						break;
					}
				}
				if (string.IsNullOrEmpty(vector.Explanation))
				{
					yield return "Missing explanation for possible infection vector: " + vector.PathwayDef.defName;
				}
			}
		}
		if (comps != null)
		{
			for (int i = 0; i < comps.Count; i++)
			{
				foreach (string item2 in comps[i].ConfigErrors(this))
				{
					yield return $"{comps[i]}: {item2}";
				}
			}
		}
		if (stages != null)
		{
			if (!typeof(Hediff_Addiction).IsAssignableFrom(hediffClass))
			{
				for (int i = 0; i < stages.Count; i++)
				{
					if (i >= 1 && stages[i].minSeverity <= stages[i - 1].minSeverity)
					{
						yield return "stages are not in order of minSeverity";
					}
				}
			}
			for (int i = 0; i < stages.Count; i++)
			{
				if (stages[i].hediffGivers != null)
				{
					for (int k = 0; k < stages[i].hediffGivers.Count; k++)
					{
						foreach (string item3 in stages[i].hediffGivers[k].ConfigErrors())
						{
							yield return item3;
						}
					}
				}
				if (stages[i].minSeverity > maxSeverity)
				{
					yield return $"minSeverity of stage {i} is greater than maxSeverity.";
				}
			}
		}
		if (hediffGivers == null)
		{
			yield break;
		}
		for (int i = 0; i < hediffGivers.Count; i++)
		{
			foreach (string item4 in hediffGivers[i].ConfigErrors())
			{
				yield return item4;
			}
		}
	}

	public override IEnumerable<StatDrawEntry> SpecialDisplayStats(StatRequest req)
	{
		if (stages == null || stages.Count != 1)
		{
			yield break;
		}
		foreach (StatDrawEntry item in stages[0].SpecialDisplayStats())
		{
			yield return item;
		}
	}

	public bool TryGetReportStringOverrideFor(JobDef def, out string str)
	{
		if (reportStringOverrides.NullOrEmpty())
		{
			str = null;
			return false;
		}
		if (reportStringOverridesDict == null)
		{
			reportStringOverridesDict = new Dictionary<JobDef, string>();
			if (reportStringOverrides != null)
			{
				foreach (ReportStringOverride reportStringOverride in reportStringOverrides)
				{
					reportStringOverridesDict[reportStringOverride.jobDef] = reportStringOverride.reportString;
				}
			}
		}
		return reportStringOverridesDict.TryGetValue(def, out str);
	}

	public static HediffDef Named(string defName)
	{
		return DefDatabase<HediffDef>.GetNamed(defName);
	}
}
