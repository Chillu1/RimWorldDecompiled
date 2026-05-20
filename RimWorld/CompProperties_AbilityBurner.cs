using Verse;

namespace RimWorld;

public class CompProperties_AbilityBurner : CompProperties_AbilityEffect
{
	public ThingDef moteDef;

	public int numStreams;

	public float coneSizeDegrees;

	public float range;

	public float rangeNoise;

	public float barrelOffsetDistance;

	public int lifespanNoise;

	public float sizeReductionDistanceThreshold;

	public EffecterDef effecterDef;

	public CompProperties_AbilityBurner()
	{
		compClass = typeof(CompAbilityEffect_Burner);
	}
}
