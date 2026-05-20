using RimWorld;
using UnityEngine;

namespace Verse;

public class PawnWaterRippleMaker
{
	private Pawn pawn;

	private const int RippleSpawnIntervalTicks = 25;

	private const int RippleSpawnIntervalTicksSwimming = 20;

	private const float RippleStartingSize = 0.6f;

	private const float MinPawnSizeFactor = 0.5f;

	private const float MaxPawnSizeFactor = 2f;

	public PawnWaterRippleMaker(Pawn pawn)
	{
		this.pawn = pawn;
	}

	public void ProcessPostTickVisuals(int ticksPassed)
	{
		if (!pawn.Flying && ((pawn.Swimming && GenTicks.IsTickInterval(pawn.GetHashCode(), 20)) || (pawn.pather.Moving && GenTicks.IsTickInterval(pawn.GetHashCode(), 25) && pawn.Map.terrainGrid.TerrainAt(pawn.Position).IsWater)))
		{
			SpawnRippleFleck(pawn);
		}
	}

	private void SpawnRippleFleck(Pawn pawn)
	{
		float size = Mathf.Clamp(pawn.BodySize, 0.5f, 2f) * 0.6f;
		FleckMaker.WaterRipple(pawn.DrawPos.WithY(AltitudeLayer.MoteLow.AltitudeFor()), pawn.Map, size);
	}
}
