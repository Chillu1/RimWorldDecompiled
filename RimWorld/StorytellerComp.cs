using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public abstract class StorytellerComp
	{
		public StorytellerCompProperties props;

		public virtual IEnumerable<FiringIncident> MakeIntervalIncidents(IIncidentTarget target)
		{
			yield break;
		}

		public virtual void Notify_PawnEvent(Pawn p, AdaptationEvent ev, DamageInfo? dinfo = null)
		{
		}

		public virtual IncidentParms GenerateParms(IncidentCategoryDef incCat, IIncidentTarget target)
		{
			return StorytellerUtility.DefaultParmsNow(incCat, target);
		}

		[Obsolete("Use IncidentParms argument instead")]
		protected IEnumerable<IncidentDef> UsableIncidentsInCategory(IncidentCategoryDef cat, IIncidentTarget target)
		{
			return UsableIncidentsInCategory(cat, (IncidentDef x) => GenerateParms(cat, target));
		}

		protected IEnumerable<IncidentDef> UsableIncidentsInCategory(IncidentCategoryDef cat, IncidentParms parms)
		{
			return UsableIncidentsInCategory(cat, (IncidentDef x) => parms);
		}

		protected virtual IEnumerable<IncidentDef> UsableIncidentsInCategory(IncidentCategoryDef cat, Func<IncidentDef, IncidentParms> parmsGetter)
		{
			return DefDatabase<IncidentDef>.AllDefsListForReading.Where((IncidentDef x) => x.category == cat && x.Worker.CanFireNow(parmsGetter(x)));
		}

		protected float IncidentChanceFactor_CurrentPopulation(IncidentDef def)
		{
			if (def.chanceFactorByPopulationCurve == null)
			{
				return 1f;
			}
			int num = PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive_Colonists.Count();
			return def.chanceFactorByPopulationCurve.Evaluate(num);
		}

		protected float IncidentChanceFactor_PopulationIntent(IncidentDef def)
		{
			if (def.populationEffect == IncidentPopulationEffect.None)
			{
				return 1f;
			}
			float num;
			switch (def.populationEffect)
			{
			case IncidentPopulationEffect.IncreaseHard:
				num = 0.4f;
				break;
			case IncidentPopulationEffect.IncreaseMedium:
				num = 0f;
				break;
			case IncidentPopulationEffect.IncreaseEasy:
				num = -0.4f;
				break;
			default:
				throw new Exception();
			}
			return Mathf.Max(StorytellerUtilityPopulation.PopulationIntent + num, props.minIncChancePopulationIntentFactor);
		}

		protected float IncidentChanceFinal(IncidentDef def)
		{
			float baseChanceThisGame = def.Worker.BaseChanceThisGame;
			baseChanceThisGame *= IncidentChanceFactor_CurrentPopulation(def);
			baseChanceThisGame *= IncidentChanceFactor_PopulationIntent(def);
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
				orderby d.category.defName descending, IncidentChanceFinal(d) descending
				select d, new TableDataGetter<IncidentDef>("defName", (IncidentDef d) => d.defName), new TableDataGetter<IncidentDef>("category", (IncidentDef d) => d.category), new TableDataGetter<IncidentDef>("can fire", (IncidentDef d) => CanFireLocal(d).ToStringCheckBlank()), new TableDataGetter<IncidentDef>("base\nchance", (IncidentDef d) => d.baseChance.ToString("F2")), new TableDataGetter<IncidentDef>("base\nchance\nwith\nRoyalty", (IncidentDef d) => (!(d.baseChanceWithRoyalty >= 0f)) ? "-" : d.baseChanceWithRoyalty.ToString("F2")), new TableDataGetter<IncidentDef>("base\nchance\nthis\ngame", (IncidentDef d) => d.Worker.BaseChanceThisGame.ToString("F2")), new TableDataGetter<IncidentDef>("final\nchance", (IncidentDef d) => IncidentChanceFinal(d).ToString("F2")), new TableDataGetter<IncidentDef>("final\nchance\npossible", (IncidentDef d) => (!CanFireLocal(d)) ? "-" : IncidentChanceFinal(d).ToString("F2")), new TableDataGetter<IncidentDef>("Factor from:\ncurrent pop", (IncidentDef d) => IncidentChanceFactor_CurrentPopulation(d).ToString()), new TableDataGetter<IncidentDef>("Factor from:\npop intent", (IncidentDef d) => IncidentChanceFactor_PopulationIntent(d).ToString()), new TableDataGetter<IncidentDef>("default target", (IncidentDef d) => (GetDefaultTarget(d) == null) ? "-" : GetDefaultTarget(d).ToString()), new TableDataGetter<IncidentDef>("current\npop", (IncidentDef d) => PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive_Colonists.Count().ToString()), new TableDataGetter<IncidentDef>("pop\nintent", (IncidentDef d) => StorytellerUtilityPopulation.PopulationIntent.ToString("F2")), new TableDataGetter<IncidentDef>("cur\npoints", (IncidentDef d) => (GetDefaultTarget(d) == null) ? "-" : StorytellerUtility.DefaultThreatPointsNow(GetDefaultTarget(d)).ToString("F0")));
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
		}
	}
}
