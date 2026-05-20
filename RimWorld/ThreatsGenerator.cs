using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld;

public static class ThreatsGenerator
{
	private static readonly SimpleCurve ThreatScaleToCountFactorCurve = new SimpleCurve
	{
		new CurvePoint(0f, 0.1f),
		new CurvePoint(0.3f, 0.5f),
		new CurvePoint(0.6f, 0.8f),
		new CurvePoint(1f, 1f),
		new CurvePoint(1.55f, 1.1f),
		new CurvePoint(2.2f, 1.2f),
		new CurvePoint(10f, 2f)
	};

	public static IEnumerable<FiringIncident> MakeIntervalIncidents(ThreatsGeneratorParams parms, IIncidentTarget target, int startTick)
	{
		float num = ThreatScaleToCountFactorCurve.Evaluate(Find.Storyteller.difficulty.threatScale);
		int incCount = IncidentCycleUtility.IncidentCountThisInterval(target, parms.randSeed, (float)GenDate.TickGameToSettled(startTick) / 60000f, parms.onDays, parms.offDays, parms.minSpacingDays, parms.numIncidentsRange.min * num, parms.numIncidentsRange.max * num);
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
			select x).TryRandomElementByWeight((IncidentDef x) => x.Worker.BaseChanceThisGame, out var result))
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
		IncidentParms incidentParms = new IncidentParms
		{
			target = target,
			points = (parms.threatPoints ?? (StorytellerUtility.DefaultThreatPointsNow(target) * parms.currentThreatPointsFactor))
		};
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
		if ((allowedThreats & AllowedThreatsGeneratorThreats.Raids) != AllowedThreatsGeneratorThreats.None)
		{
			yield return IncidentDefOf.RaidEnemy;
		}
		if ((allowedThreats & AllowedThreatsGeneratorThreats.MechClusters) != AllowedThreatsGeneratorThreats.None)
		{
			yield return IncidentDefOf.MechCluster;
		}
	}
}
