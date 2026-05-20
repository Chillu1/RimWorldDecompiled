using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.AI.Group;

namespace RimWorld;

public class RaidStrategyWorker_ImmediateAttackBreaching : RaidStrategyWorker_WithRequiredPawnKinds
{
	private static SimpleCurve MinGoodBreachersFromPointCurve = new SimpleCurve
	{
		new CurvePoint(0f, 1f),
		new CurvePoint(200f, 1f),
		new CurvePoint(1000f, 3f),
		new CurvePoint(4000f, 4f)
	};

	private const int MaxGoodBreachesForMechanoids = 1;

	protected bool useAvoidGridSmart;

	protected override bool MatchesRequiredPawnKind(PawnKindDef kind)
	{
		return kind.isGoodBreacher;
	}

	protected override int MinRequiredPawnsForPoints(float pointsTotal, Faction faction)
	{
		if (faction == null || faction != Faction.OfMechanoids)
		{
			return Mathf.RoundToInt(MinGoodBreachersFromPointCurve.Evaluate(pointsTotal));
		}
		return 1;
	}

	protected override LordJob MakeLordJob(IncidentParms parms, Map map, List<Pawn> pawns, int raidSeed)
	{
		Faction faction = parms.faction;
		bool canTimeoutOrFlee = parms.canTimeoutOrFlee;
		return new LordJob_AssaultColony(canKidnap: parms.canKidnap, canTimeoutOrFlee: canTimeoutOrFlee, sappers: false, canSteal: parms.canSteal, assaulterFaction: faction, useAvoidGridSmart: useAvoidGridSmart, breachers: true);
	}
}
