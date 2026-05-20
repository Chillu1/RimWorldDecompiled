using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class GenStep_ManhunterPack : GenStep
	{
		public FloatRange defaultPointsRange = new FloatRange(300f, 500f);

		private int MinRoomCells = 225;

		public override int SeedPart => 457293335;

		public override void Generate(Map map, GenStepParams parms)
		{
			TraverseParms traverseParams = TraverseParms.For(TraverseMode.NoPassClosedDoors).WithFenceblocked(forceFenceblocked: true);
			if (RCellFinder.TryFindRandomCellNearTheCenterOfTheMapWith((IntVec3 x) => CellValidator(x) && !x.Fogged(map) && map.reachability.CanReachMapEdge(x, traverseParams) && x.GetRoom(map).CellCount >= MinRoomCells, map, out var result))
			{
				float points = ((parms.sitePart != null) ? parms.sitePart.parms.threatPoints : defaultPointsRange.RandomInRange);
				PawnKindDef animalKind;
				if (parms.sitePart != null && parms.sitePart.parms.animalKind != null)
				{
					animalKind = parms.sitePart.parms.animalKind;
				}
				else if (!ManhunterPackGenStepUtility.TryGetAnimalsKind(points, map.Tile, out animalKind))
				{
					return;
				}
				List<Pawn> list = AggressiveAnimalIncidentUtility.GenerateAnimals(animalKind, map.Tile, points);
				for (int num = 0; num < list.Count; num++)
				{
					CellFinder.TryFindRandomSpawnCellForPawnNear(result, map, out var result2, 10, CellValidator);
					GenSpawn.Spawn(list[num], result2, map, Rot4.Random);
					list[num].health.AddHediff(HediffDefOf.Scaria);
					list[num].mindState.mentalStateHandler.TryStartMentalState(MentalStateDefOf.ManhunterPermanent);
				}
			}
			bool CellValidator(IntVec3 x)
			{
				if (!x.Standable(map))
				{
					return false;
				}
				if (MapGenerator.UsedRects.Any((CellRect r) => r.Contains(x)))
				{
					return false;
				}
				return true;
			}
		}
	}
}
