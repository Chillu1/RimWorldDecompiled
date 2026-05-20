using Verse;

namespace RimWorld;

public class CompProperties_Pushable : CompProperties
{
	public float offsetDistance;

	public float smoothTime = 0.3f;

	public bool givePushOption;

	public CompProperties_Pushable()
	{
		compClass = typeof(CompPushable);
	}
}
