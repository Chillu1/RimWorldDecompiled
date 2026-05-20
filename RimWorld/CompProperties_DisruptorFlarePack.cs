using Verse;

namespace RimWorld;

public class CompProperties_DisruptorFlarePack : CompProperties_AIUSablePack
{
	public float fieldRadius;

	public float durationSeconds;

	public CompProperties_DisruptorFlarePack()
	{
		compClass = typeof(CompDisruptorFlarePack);
	}
}
