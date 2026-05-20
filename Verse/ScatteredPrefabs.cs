using System.Collections.Generic;

namespace Verse;

public class ScatteredPrefabs
{
	public List<GenStep_ScatterGroupPrefabs.ScatterGroup> prefabs = new List<GenStep_ScatterGroupPrefabs.ScatterGroup>();

	public FloatRange countPer10kCellsRange = FloatRange.Zero;

	public int minSpacing;

	public float maxDistFromStructure;
}
