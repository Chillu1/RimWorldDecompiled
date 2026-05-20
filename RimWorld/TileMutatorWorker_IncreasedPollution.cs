using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace RimWorld;

public class TileMutatorWorker_IncreasedPollution : TileMutatorWorker
{
	private static readonly FloatRange PollutionOffsetRange = new FloatRange(0.1f, 0.2f);

	public TileMutatorWorker_IncreasedPollution(TileMutatorDef def)
		: base(def)
	{
	}

	public override void OnAddedToTile(PlanetTile tile)
	{
		Tile tile2 = tile.Tile;
		tile2.pollution = Mathf.Clamp01(tile2.pollution + PollutionOffsetRange.RandomInRange);
	}
}
