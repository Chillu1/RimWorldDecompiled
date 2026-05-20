using Verse;

namespace RimWorld.Planet;

public static class MapParentTuning
{
	public const float ShortDetectionCountdownDays = 4f;

	public const float DefaultDetectionCountdownDays = 4f;

	public static readonly FloatRange SiteDetectionCountdownMultiplier = new FloatRange(0.85f, 1.15f);
}
