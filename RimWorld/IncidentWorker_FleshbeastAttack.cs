using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld;

public class IncidentWorker_FleshbeastAttack : IncidentWorker
{
	private const int MaxIterations = 100;

	private const float FleshbeastPointsFactor = 0.6f;

	private static readonly IntRange FleshbeastSpawnDelayTicks = new IntRange(180, 180);

	private static readonly IntRange PitBurrowEmergenceDelayRangeTicks = new IntRange(420, 420);

	private static readonly LargeBuildingSpawnParms BurrowSpawnParms = new LargeBuildingSpawnParms
	{
		maxDistanceToColonyBuilding = -1f,
		minDistToEdge = 10,
		attemptNotUnderBuildings = true,
		canSpawnOnImpassable = false,
		attemptSpawnLocationType = SpawnLocationType.Outdoors
	};

	protected override bool CanFireNowSub(IncidentParms parms)
	{
		if (!ModsConfig.AnomalyActive)
		{
			return false;
		}
		_ = (Map)parms.target;
		return base.CanFireNowSub(parms);
	}

	protected override bool TryExecuteWorker(IncidentParms parms)
	{
		Map map = (Map)parms.target;
		float num = parms.points * 0.6f;
		List<Thing> list = new List<Thing>();
		int num2 = 0;
		while (num > 0f)
		{
			if (!LargeBuildingCellFinder.TryFindCell(out var cell, map, BurrowSpawnParms.ForThing(ThingDefOf.PitBurrow)))
			{
				return false;
			}
			float num3 = Mathf.Min(num, 500f);
			Thing item = FleshbeastUtility.SpawnFleshbeastsFromPitBurrowEmergence(cell, map, num3, PitBurrowEmergenceDelayRangeTicks, FleshbeastSpawnDelayTicks);
			list.Add(item);
			num -= num3;
			num2++;
			if (num2 > 100)
			{
				break;
			}
		}
		SendStandardLetter(def.letterLabel, (list.Count > 1) ? def.letterTextPlural : def.letterText, def.letterDef, parms, list);
		return true;
	}
}
