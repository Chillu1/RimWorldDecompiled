using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class IncidentWorker_FarmAnimalsWanderIn : IncidentWorker
	{
		private const float MaxWildness = 0.35f;

		private const float TotalBodySizeToSpawn = 2.5f;

		protected override bool CanFireNowSub(IncidentParms parms)
		{
			if (!base.CanFireNowSub(parms))
			{
				return false;
			}
			Map map = (Map)parms.target;
			PawnKindDef kind;
			if (RCellFinder.TryFindRandomPawnEntryCell(out IntVec3 _, map, CellFinder.EdgeRoadChance_Animal))
			{
				return TryFindRandomPawnKind(map, out kind);
			}
			return false;
		}

		protected override bool TryExecuteWorker(IncidentParms parms)
		{
			Map map = (Map)parms.target;
			if (!RCellFinder.TryFindRandomPawnEntryCell(out IntVec3 result, map, CellFinder.EdgeRoadChance_Animal))
			{
				return false;
			}
			if (!TryFindRandomPawnKind(map, out PawnKindDef kind))
			{
				return false;
			}
			int num = Mathf.Clamp(GenMath.RoundRandom(2.5f / kind.RaceProps.baseBodySize), 2, 10);
			for (int i = 0; i < num; i++)
			{
				IntVec3 loc = CellFinder.RandomClosewalkCellNear(result, map, 12);
				Pawn pawn = PawnGenerator.GeneratePawn(kind);
				GenSpawn.Spawn(pawn, loc, map, Rot4.Random);
				pawn.SetFaction(Faction.OfPlayer);
			}
			SendStandardLetter("LetterLabelFarmAnimalsWanderIn".Translate(kind.GetLabelPlural()).CapitalizeFirst(), "LetterFarmAnimalsWanderIn".Translate(kind.GetLabelPlural()), LetterDefOf.PositiveEvent, parms, new TargetInfo(result, map));
			return true;
		}

		private bool TryFindRandomPawnKind(Map map, out PawnKindDef kind)
		{
			return DefDatabase<PawnKindDef>.AllDefs.Where((PawnKindDef x) => x.RaceProps.Animal && x.RaceProps.wildness < 0.35f && map.mapTemperature.SeasonAndOutdoorTemperatureAcceptableFor(x.race)).TryRandomElementByWeight((PawnKindDef k) => 0.420000017f - k.RaceProps.wildness, out kind);
		}
	}
}
