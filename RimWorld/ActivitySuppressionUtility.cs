using Verse;

namespace RimWorld;

public static class ActivitySuppressionUtility
{
	private const float MinActivityToSuppress = 0.05f;

	public static bool CanBeSuppressed(Thing thing, bool considerMinActivity = true, bool playerForced = false)
	{
		if (!thing.TryGetComp(out CompActivity comp))
		{
			return false;
		}
		if (thing is Pawn pawn)
		{
			if (pawn.Downed)
			{
				return false;
			}
			if (comp.Props.requiresHoldingPlatform && !pawn.IsOnHoldingPlatform)
			{
				return false;
			}
		}
		if (comp.IsActive)
		{
			return false;
		}
		if (!comp.CanBeSuppressed)
		{
			return false;
		}
		if (!playerForced && !comp.suppressionEnabled)
		{
			return false;
		}
		if (considerMinActivity && comp.ActivityLevel < 0.05f)
		{
			return false;
		}
		return true;
	}
}
