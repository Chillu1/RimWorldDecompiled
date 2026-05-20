namespace RimWorld;

public class CompProperties_AbilityChunkskip : CompProperties_AbilityEffect
{
	public int chunkCount;

	public float scatterRadius;

	public CompProperties_AbilityChunkskip()
	{
		compClass = typeof(CompAbilityEffect_Chunkskip);
	}
}
