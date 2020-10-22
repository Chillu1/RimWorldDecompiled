using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace RimWorld
{
	public class IncidentWorker_HerdMigration : IncidentWorker
	{
		private static readonly IntRange AnimalsCount = new IntRange(3, 5);

		private const float MinTotalBodySize = 4f;

		protected override bool CanFireNowSub(IncidentParms parms)
		{
			Map map = (Map)parms.target;
			IntVec3 start;
			IntVec3 end;
			if (TryFindAnimalKind(map.Tile, out var _))
			{
				return TryFindStartAndEndCells(map, out start, out end);
			}
			return false;
		}

		protected override bool TryExecuteWorker(IncidentParms parms)
		{
			Map map = (Map)parms.target;
			if (!TryFindAnimalKind(map.Tile, out var animalKind))
			{
				return false;
			}
			if (!TryFindStartAndEndCells(map, out var start, out var end))
			{
				return false;
			}
			Rot4 rot = Rot4.FromAngleFlat((map.Center - start).AngleFlat);
			List<Pawn> list = GenerateAnimals(animalKind, map.Tile);
			for (int i = 0; i < list.Count; i++)
			{
				Pawn newThing = list[i];
				IntVec3 loc = CellFinder.RandomClosewalkCellNear(start, map, 10);
				GenSpawn.Spawn(newThing, loc, map, rot);
			}
			LordMaker.MakeNewLord(null, new LordJob_ExitMapNear(end, LocomotionUrgency.Walk), map, list);
			string str = string.Format(def.letterText, animalKind.GetLabelPlural()).CapitalizeFirst();
			string str2 = string.Format(def.letterLabel, animalKind.GetLabelPlural().CapitalizeFirst());
			SendStandardLetter(str2, str, def.letterDef, parms, list[0]);
			return true;
		}

		private bool TryFindAnimalKind(int tile, out PawnKindDef animalKind)
		{
			return DefDatabase<PawnKindDef>.AllDefs.Where((PawnKindDef k) => k.RaceProps.CanDoHerdMigration && Find.World.tileTemperatures.SeasonAndOutdoorTemperatureAcceptableFor(tile, k.race)).TryRandomElementByWeight((PawnKindDef x) => Mathf.Lerp(0.2f, 1f, x.RaceProps.wildness), out animalKind);
		}

		private bool TryFindStartAndEndCells(Map map, out IntVec3 start, out IntVec3 end)
		{
			if (!RCellFinder.TryFindRandomPawnEntryCell(out start, map, CellFinder.EdgeRoadChance_Animal))
			{
				end = IntVec3.Invalid;
				return false;
			}
			end = IntVec3.Invalid;
			for (int i = 0; i < 8; i++)
			{
				IntVec3 startLocal = start;
				if (!CellFinder.TryFindRandomEdgeCellWith((IntVec3 x) => map.reachability.CanReach(startLocal, x, PathEndMode.OnCell, TraverseMode.NoPassClosedDoors, Danger.Deadly), map, CellFinder.EdgeRoadChance_Ignore, out var result))
				{
					break;
				}
				if (!end.IsValid || result.DistanceToSquared(start) > end.DistanceToSquared(start))
				{
					end = result;
				}
			}
			return end.IsValid;
		}

		private List<Pawn> GenerateAnimals(PawnKindDef animalKind, int tile)
		{
			int randomInRange = AnimalsCount.RandomInRange;
			randomInRange = Mathf.Max(randomInRange, Mathf.CeilToInt(4f / animalKind.RaceProps.baseBodySize));
			List<Pawn> list = new List<Pawn>();
			for (int i = 0; i < randomInRange; i++)
			{
				Pawn item = PawnGenerator.GeneratePawn(new PawnGenerationRequest(animalKind, null, PawnGenerationContext.NonPlayer, tile));
				list.Add(item);
			}
			return list;
		}
	}
}
