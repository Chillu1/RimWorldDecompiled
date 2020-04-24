using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public static class ThreatsGenerator
	{
		public static IEnumerable<FiringIncident> MakeIntervalIncidents(ThreatsGeneratorParams parms, IIncidentTarget target, int startTick)
		{
			float threatsGeneratorThreatCountFactor = Find.Storyteller.difficulty.threatsGeneratorThreatCountFactor;
			int incCount = IncidentCycleUtility.IncidentCountThisInterval(target, parms.randSeed, (float)startTick / 60000f, parms.onDays, parms.offDays, parms.minSpacingDays, parms.numIncidentsRange.min * threatsGeneratorThreatCountFactor, parms.numIncidentsRange.max * threatsGeneratorThreatCountFactor);
			for (int i = 0; i < incCount; i++)
			{
				FiringIncident firingIncident = MakeThreat(parms, target);
				if (firingIncident != null)
				{
					yield return firingIncident;
				}
			}
		}

		private static FiringIncident MakeThreat(ThreatsGeneratorParams parms, IIncidentTarget target)
		{
			IncidentParms incParms = GetIncidentParms(parms, target);
			if (!(from x in GetPossibleIncidents(parms.allowedThreats)
				where x.Worker.CanFireNow(incParms)
				select x).TryRandomElementByWeight((IncidentDef x) => x.Worker.BaseChanceThisGame, out IncidentDef result))
			{
				return null;
			}
			return new FiringIncident
			{
				def = result,
				parms = incParms
			};
		}

		public static bool AnyIncidentPossible(ThreatsGeneratorParams parms, IIncidentTarget target)
		{
			IncidentParms incParms = GetIncidentParms(parms, target);
			return GetPossibleIncidents(parms.allowedThreats).Any((IncidentDef x) => x.Worker.BaseChanceThisGame > 0f && x.Worker.CanFireNow(incParms));
		}

		private static IncidentParms GetIncidentParms(ThreatsGeneratorParams parms, IIncidentTarget target)
		{
			IncidentParms incidentParms = new IncidentParms();
			incidentParms.target = target;
			incidentParms.points = (parms.threatPoints ?? (StorytellerUtility.DefaultThreatPointsNow(target) * parms.currentThreatPointsFactor));
			if (parms.minThreatPoints.HasValue)
			{
				incidentParms.points = Mathf.Max(incidentParms.points, parms.minThreatPoints.Value);
			}
			incidentParms.faction = parms.faction;
			incidentParms.forced = true;
			return incidentParms;
		}

		private static IEnumerable<IncidentDef> GetPossibleIncidents(AllowedThreatsGeneratorThreats allowedThreats)
		{
			if ((allowedThreats & AllowedThreatsGeneratorThreats.Raids) != 0)
			{
				yield return IncidentDefOf.RaidEnemy;
			}
			if ((allowedThreats & AllowedThreatsGeneratorThreats.MechClusters) != 0)
			{
				yield return IncidentDefOf.MechCluster;
			}
		}
	}
}
