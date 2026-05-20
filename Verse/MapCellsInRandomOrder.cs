using System.Collections.Generic;

namespace Verse;

public class MapCellsInRandomOrder
{
	private Map map;

	private List<IntVec3> randomizedCells;

	public MapCellsInRandomOrder(Map map)
	{
		this.map = map;
	}

	public List<IntVec3> GetAll()
	{
		CreateListIfShould();
		return randomizedCells;
	}

	public IntVec3 Get(int index)
	{
		CreateListIfShould();
		return randomizedCells[index];
	}

	private void CreateListIfShould()
	{
		if (randomizedCells != null)
		{
			return;
		}
		randomizedCells = new List<IntVec3>(map.Area);
		foreach (IntVec3 allCell in map.AllCells)
		{
			randomizedCells.Add(allCell);
		}
		Rand.PushState();
		Rand.Seed = Find.World.info.Seed ^ map.Tile.GetHashCode();
		randomizedCells.Shuffle();
		Rand.PopState();
	}
}
