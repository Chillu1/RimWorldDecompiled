using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class GenStep_AlphaThrumboSighting : GenStep
{
	public IntRange normalThrumboCountRange = new IntRange(3, 5);

	private int MinRoomCells = 225;

	private static readonly FloatRange ExcludeBiologicalAgeRange = new FloatRange(0f, 250f);

	public override int SeedPart => 792525399;

	public override void Generate(Map map, GenStepParams parms)
	{
		TraverseParms traverseParams = TraverseParms.For(TraverseMode.NoPassClosedDoors).WithFenceblocked(forceFenceblocked: true);
		List<CellRect> usedRects = MapGenerator.GetOrGenerateVar<List<CellRect>>("UsedRects");
		if (RCellFinder.TryFindRandomCellNearTheCenterOfTheMapWith((IntVec3 x) => x.Standable(map) && !x.Fogged(map) && map.reachability.CanReachMapEdge(x, traverseParams) && x.GetRoom(map).CellCount >= MinRoomCells && !usedRects.Any((CellRect ur) => ur.Contains(x)), map, out var result))
		{
			List<Pawn> list = new List<Pawn>();
			PawnGenerationRequest request = new PawnGenerationRequest(PawnKindDefOf.AlphaThrumbo, null, PawnGenerationContext.NonPlayer, map.Tile);
			request.ExcludeBiologicalAgeRange = ExcludeBiologicalAgeRange;
			Pawn item = PawnGenerator.GeneratePawn(request);
			list.Add(item);
			int randomInRange = normalThrumboCountRange.RandomInRange;
			for (int num = 0; num < randomInRange; num++)
			{
				request = new PawnGenerationRequest(PawnKindDefOf.Thrumbo, null, PawnGenerationContext.NonPlayer, map.Tile);
				item = PawnGenerator.GeneratePawn(request);
				list.Add(item);
			}
			for (int num2 = 0; num2 < list.Count; num2++)
			{
				IntVec3 loc = CellFinder.RandomSpawnCellForPawnNear(result, map, 10);
				GenSpawn.Spawn(list[num2], loc, map, Rot4.Random);
			}
		}
	}
}
