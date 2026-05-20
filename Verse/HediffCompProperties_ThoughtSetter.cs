using RimWorld;

namespace Verse;

public class HediffCompProperties_ThoughtSetter : HediffCompProperties
{
	public ThoughtDef thought;

	public int moodOffset;

	public FloatRange moodOffsetRange = FloatRange.Zero;

	public HediffCompProperties_ThoughtSetter()
	{
		compClass = typeof(HediffComp_ThoughtSetter);
	}
}
