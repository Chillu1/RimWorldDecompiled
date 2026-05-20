using UnityEngine;
using Verse;

namespace RimWorld;

public class IncidentWorker_Alphabeavers : IncidentWorker
{
	private static readonly FloatRange CountPerColonistRange = new FloatRange(1f, 1.5f);

	private const int MinCount = 1;

	private const int MaxCount = 10;

	protected override bool CanFireNowSub(IncidentParms parms)
	{
		if (!base.CanFireNowSub(parms))
		{
			return false;
		}
		Map map = (Map)parms.target;
		if (!map.mapTemperature.SeasonAndOutdoorTemperatureAcceptableFor(PawnKindDefOf.Alphabeaver.race))
		{
			return false;
		}
		IntVec3 result;
		return RCellFinder.TryFindRandomPawnEntryCell(out result, map, CellFinder.EdgeRoadChance_Animal);
	}

	protected override bool TryExecuteWorker(IncidentParms parms)
	{
		Map map = (Map)parms.target;
		PawnKindDef alphabeaver = PawnKindDefOf.Alphabeaver;
		if (!RCellFinder.TryFindRandomPawnEntryCell(out var result, map, CellFinder.EdgeRoadChance_Animal))
		{
			return false;
		}
		int freeColonistsCount = map.mapPawns.FreeColonistsCount;
		float randomInRange = CountPerColonistRange.RandomInRange;
		int num = Mathf.Clamp(GenMath.RoundRandom((float)freeColonistsCount * randomInRange), 1, 10);
		for (int i = 0; i < num; i++)
		{
			IntVec3 loc = CellFinder.RandomClosewalkCellNear(result, map, 10);
			((Pawn)GenSpawn.Spawn(PawnGenerator.GeneratePawn(alphabeaver), loc, map)).needs.food.CurLevelPercentage = 1f;
		}
		SendStandardLetter("LetterLabelBeaversArrived".Translate(), "BeaversArrived".Translate(), LetterDefOf.ThreatSmall, parms, new TargetInfo(result, map));
		return true;
	}
}
