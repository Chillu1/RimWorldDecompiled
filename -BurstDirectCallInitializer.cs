using RimWorld;
using RimWorld.Planet;
using UnityEngine;

internal static class _0024BurstDirectCallInitializer
{
	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
	private static void Initialize()
	{
		MapGenUtility.ComputeLargestRects_0000B717_0024BurstDirectCall.Initialize();
		MapGenUtility.RectsComputeSpaces_0000B718_0024BurstDirectCall.Initialize();
		FastTileFinder.Initialize_0024ComputeQueryJob_SphericalDistance_00014EFD_0024BurstDirectCall();
		PlanetLayer.CalculateAverageTileSize_000153C0_0024BurstDirectCall.Initialize();
		PlanetLayer.IntGetTileSize_000153C2_0024BurstDirectCall.Initialize();
		PlanetLayer.IntGetTileCenter_000153C5_0024BurstDirectCall.Initialize();
	}
}
