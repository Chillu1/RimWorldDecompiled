using UnityEngine;
using Verse;

namespace RimWorld;

public class GenStep_ScatterGeysers : GenStep_ScatterThings
{
	protected override int CalculateFinalCount(Map map)
	{
		float num = map.Biome.geyserCountFactor;
		foreach (TileMutatorDef mutator in map.TileInfo.Mutators)
		{
			num *= mutator.geyserCountFactor;
		}
		return Mathf.RoundToInt((float)base.CalculateFinalCount(map) * num);
	}
}
