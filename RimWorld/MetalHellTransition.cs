using RimWorld.Planet;
using Verse;

namespace RimWorld;

public class MetalHellTransition : MusicTransition
{
	public override bool IsTransitionSatisfied()
	{
		if (!ModsConfig.AnomalyActive)
		{
			return false;
		}
		if (!base.IsTransitionSatisfied())
		{
			return false;
		}
		foreach (PocketMapParent pocketMap in Find.World.pocketMaps)
		{
			if (pocketMap.Map != null && IsValidPocketMap(pocketMap))
			{
				return true;
			}
		}
		return false;
	}

	private static bool IsValidPocketMap(PocketMapParent pocketMap)
	{
		return pocketMap.Map.generatorDef == MapGeneratorDefOf.MetalHell;
	}
}
