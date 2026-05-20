using System;
using System.Collections.Generic;
using System.Linq;
using LudeonTK;
using UnityEngine;
using Verse;

namespace RimWorld;

public abstract class StorytellerComp
{
	public StorytellerCompProperties props;

	public static readonly SimpleCurve IncreasesPopChanceByPopIntentCurve = new SimpleCurve
	{
		new CurvePoint(0f, 0.05f),
		new CurvePoint(1f, 0.3f),
		new CurvePoint(3f, 0.45f)
	};

	public virtual IEnumerable<FiringIncident> MakeIntervalIncidents(IIncidentTarget target)
	{
		return Enumerable.Empty<FiringIncident>();
	}

	public virtual void Notify_PawnEvent(Pawn p, AdaptationEvent ev, DamageInfo? dinfo = null)
	{
	}

	public virtual void Notify_DissolutionEvent(Thing thing)
	{
	}

	public virtual IncidentParms GenerateParms(IncidentCategoryDef incCat, IIncidentTarget target)
	{
		return StorytellerUtility.DefaultParmsNow(incCat, target);
	}

	protected IEnumerable<IncidentDef> UsableIncidentsInCategory(IncidentCategoryDef cat, IncidentParms parms)
	{
		IncidentParms parmsCopy = parms.ShallowCopy();
		return UsableIncidentsInCategory(cat, (IncidentDef x) => parmsCopy);
	}

	protected virtual IEnumerable<IncidentDef> UsableIncidentsInCategory(IncidentCategoryDef cat, Func<IncidentDef, IncidentParms> parmsGetter)
	{
		if (!cat.canUseAnomalyChance)
		{
			return DefDatabase<IncidentDef>.AllDefsListForReading.Where(CanUseIncident);
		}
		if (!ModsConfig.AnomalyActive || !Rand.Chance(Find.Storyteller.AnomalyIncidentChanceNow))
		{
			return Storyteller.NonAnomalyIncidents.Where(CanUseIncident);
		}
		return Storyteller.AnomalyIncidents.Where(CanUseIncident);
		bool CanUseIncident(IncidentDef incident)
		{
			if (incident.category != cat)
			{
				return false;
			}
			if (ModsConfig.AnomalyActive && !Find.Storyteller.difficulty.AnomalyPlaystyleDef.enableAnomalyContent && incident.modContentPack != null && incident.modContentPack.IsOfficialMod && ExpansionDefOf.Anomaly.linkedMod == incident.modContentPack.PackageId)
			{
				return false;
			}
			return incident.Worker.CanFireNow(parmsGetter(incident));
		}
	}

	protected float IncidentChanceFactor_CurrentPopulation(IncidentDef def)
	{
		if (def.chanceFactorByPopulationCurve == null)
		{
			return 1f;
		}
		int num = PawnsFinder.AllMapsCaravansAndTravellingTransporters_Alive_Colonists.Count();
		return def.chanceFactorByPopulationCurve.Evaluate(num);
	}

	protected float IncidentChanceFactor_PopulationIntent(IncidentDef def)
	{
		if (def.populationEffect == IncidentPopulationEffect.None)
		{
			return 1f;
		}
		float num = def.populationEffect switch
		{
			IncidentPopulationEffect.IncreaseHard => 0.4f, 
			IncidentPopulationEffect.IncreaseMedium => 0f, 
			IncidentPopulationEffect.IncreaseEasy => -0.4f, 
			_ => throw new Exception(), 
		};
		return Mathf.Max(StorytellerUtilityPopulation.PopulationIntent + num, props.minIncChancePopulationIntentFactor);
	}

	protected bool TrySelectRandomIncident(IEnumerable<IncidentDef> incidents, out IncidentDef foundDef, IIncidentTarget target)
	{
		bool num = Rand.Chance(IncreasesPopChanceByPopIntentCurve.Evaluate(StorytellerUtilityPopulation.PopulationIntent));
		foundDef = null;
		if (num)
		{
			foundDef = incidents.Where((IncidentDef i) => i.populationEffect != IncidentPopulationEffect.None).RandomElementByWeightWithFallback((IncidentDef i) => IncidentChanceFinal(i, target));
		}
		else
		{
			foundDef = incidents.Where((IncidentDef i) => i.populationEffect == IncidentPopulationEffect.None).RandomElementByWeightWithFallback((IncidentDef i) => IncidentChanceFinal(i, target));
		}
		if (foundDef == null)
		{
			foundDef = incidents.RandomElementByWeightWithFallback((IncidentDef i) => IncidentChanceFinal(i, target));
		}
		return foundDef != null;
	}

	protected float IncidentChanceFinal(IncidentDef def, IIncidentTarget target)
	{
		float baseChanceThisGame = def.Worker.BaseChanceThisGame;
		baseChanceThisGame *= IncidentChanceFactor_CurrentPopulation(def);
		baseChanceThisGame *= IncidentChanceFactor_PopulationIntent(def);
		baseChanceThisGame *= def.Worker.ChanceFactorNow(target);
		if (!def.ShouldIgnoreRecentWeighting)
		{
			for (int i = 0; i < target.StoryState.RecentRandomIncidents.Count; i++)
			{
				if (target.StoryState.RecentRandomIncidents[i] == def)
				{
					baseChanceThisGame *= QuestTuning.RecentStoryWeightFactors[i];
				}
			}
		}
		return Mathf.Max(0f, baseChanceThisGame);
	}

	public virtual void Initialize()
	{
	}

	public override string ToString()
	{
		string text = GetType().Name;
		string text2 = typeof(StorytellerComp).Name + "_";
		if (text.StartsWith(text2))
		{
			text = text.Substring(text2.Length);
		}
		if (!props.allowedTargetTags.NullOrEmpty())
		{
			text = text + " (" + props.allowedTargetTags.Select((IncidentTargetTagDef x) => x.ToString()).ToCommaList() + ")";
		}
		return text;
	}

	public virtual void DebugTablesIncidentChances()
	{
		DebugTables.MakeTablesDialog(from d in DefDatabase<IncidentDef>.AllDefs
			orderby d.category.defName descending, IncidentChanceFinal(d, Find.CurrentMap) descending
			select d, new TableDataGetter<IncidentDef>("defName", (IncidentDef d) => d.defName), new TableDataGetter<IncidentDef>("category", (IncidentDef d) => d.category), new TableDataGetter<IncidentDef>("can fire", (IncidentDef d) => CanFireLocal(d).ToStringCheckBlank()), new TableDataGetter<IncidentDef>("base\nchance", (IncidentDef d) => d.baseChance.ToString("F2")), new TableDataGetter<IncidentDef>("base\nchance\nwith\nRoyalty", (IncidentDef d) => (!(d.baseChanceWithRoyalty >= 0f)) ? "-" : d.baseChanceWithRoyalty.ToString("F2")), new TableDataGetter<IncidentDef>("base\nchance\nthis\ngame", (IncidentDef d) => d.Worker.BaseChanceThisGame.ToString("F2")), new TableDataGetter<IncidentDef>("recently\noccured\nfactor", (IncidentDef d) => (!d.ShouldIgnoreRecentWeighting) ? RecentChanceFactor(d).ToString("F2") : "-"), new TableDataGetter<IncidentDef>("Factor from:\ncurrent pop", (IncidentDef d) => IncidentChanceFactor_CurrentPopulation(d).ToString()), new TableDataGetter<IncidentDef>("Factor from:\npop intent", (IncidentDef d) => IncidentChanceFactor_PopulationIntent(d).ToString()), new TableDataGetter<IncidentDef>("final\nchance", (IncidentDef d) => IncidentChanceFinal(d, Find.CurrentMap).ToString("F2")), new TableDataGetter<IncidentDef>("final\nchance\npossible", (IncidentDef d) => (!CanFireLocal(d)) ? "-" : IncidentChanceFinal(d, Find.CurrentMap).ToString("F2")), new TableDataGetter<IncidentDef>("default target", (IncidentDef d) => (GetDefaultTarget(d) == null) ? "-" : GetDefaultTarget(d).ToString()), new TableDataGetter<IncidentDef>("current\npop", (IncidentDef d) => PawnsFinder.AllMapsCaravansAndTravellingTransporters_Alive_Colonists.Count().ToString()), new TableDataGetter<IncidentDef>("pop\nintent", (IncidentDef d) => StorytellerUtilityPopulation.PopulationIntent.ToString("F2")), new TableDataGetter<IncidentDef>("cur\npoints", (IncidentDef d) => (GetDefaultTarget(d) == null) ? "-" : StorytellerUtility.DefaultThreatPointsNow(GetDefaultTarget(d)).ToString("F0")), new TableDataGetter<IncidentDef>("DLC", (IncidentDef d) => (!d.modContentPack.IsCoreMod) ? ((!d.modContentPack.IsOfficialMod) ? "-" : d.modContentPack.Name.Split('.').Last()) : "Core"), new TableDataGetter<IncidentDef>("earliestDay", (IncidentDef d) => d.earliestDay), new TableDataGetter<IncidentDef>("minRefireDays", (IncidentDef d) => d.minRefireDays), new TableDataGetter<IncidentDef>("minAnomalyThreatLevel", (IncidentDef d) => d.minAnomalyThreatLevel), new TableDataGetter<IncidentDef>("minPopulation", (IncidentDef d) => d.minPopulation), new TableDataGetter<IncidentDef>("min/max threatpoints", (IncidentDef d) => ((d.minThreatPoints > float.MinValue) ? d.minThreatPoints.ToString("F2") : "minUnset") + "/" + ((d.maxThreatPoints < float.MaxValue) ? d.maxThreatPoints.ToString("F2") : "maxUnset")));
		static bool CanFireLocal(IncidentDef d)
		{
			IIncidentTarget incidentTarget = GetDefaultTarget(d);
			if (incidentTarget == null)
			{
				return false;
			}
			IncidentParms parms = StorytellerUtility.DefaultParmsNow(d.category, incidentTarget);
			return d.Worker.CanFireNow(parms);
		}
		static IIncidentTarget GetDefaultTarget(IncidentDef d)
		{
			if (d.TargetAllowed(Find.CurrentMap))
			{
				return Find.CurrentMap;
			}
			if (d.TargetAllowed(Find.World))
			{
				return Find.World;
			}
			return null;
		}
		static float RecentChanceFactor(IncidentDef def)
		{
			IIncidentTarget incidentTarget = GetDefaultTarget(def);
			if (incidentTarget == null)
			{
				return 1f;
			}
			for (int i = 0; i < incidentTarget.StoryState.RecentRandomIncidents.Count; i++)
			{
				if (incidentTarget.StoryState.RecentRandomIncidents[i] == def)
				{
					return QuestTuning.RecentStoryWeightFactors[i];
				}
			}
			return 1f;
		}
	}
}
