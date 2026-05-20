using System.Linq;
using Verse;

namespace RimWorld;

public class GenStep_PlaceCaveExit : GenStep
{
	public const float ClearRadius = 4.5f;

	public override int SeedPart => 12412314;

	public override void Generate(Map map, GenStepParams parms)
	{
		CellFinder.TryFindRandomCell(map, (IntVec3 cell) => cell.Standable(map) && (float)cell.DistanceToEdge(map) > 5.5f, out var result);
		foreach (IntVec3 item in GenRadial.RadialCellsAround(result, 4.5f, useCenter: true))
		{
			foreach (Thing item2 in from t in item.GetThingList(map).ToList()
				where t.def.destroyable
				select t)
			{
				item2.Destroy();
			}
		}
		GenSpawn.Spawn(ThingMaker.MakeThing(ThingDefOf.CaveExit), result, map);
		MapGenerator.PlayerStartSpot = result;
	}
}
