namespace RimWorld;

public class CompProperties_ConsumeLeap : CompProperties_AbilityEffect
{
	public float maxBodySize = 2f;

	public CompProperties_ConsumeLeap()
	{
		compClass = typeof(CompAbilityEffect_ConsumeLeap);
	}
}
