using RimWorld.Planet;

namespace Verse
{
	public static class LookTargetsUtility
	{
		public static bool IsValid(this LookTargets lookTargets)
		{
			return lookTargets?.IsValid ?? false;
		}

		public static GlobalTargetInfo TryGetPrimaryTarget(this LookTargets lookTargets)
		{
			return lookTargets?.PrimaryTarget ?? GlobalTargetInfo.Invalid;
		}

		public static void TryHighlight(this LookTargets lookTargets, bool arrow = true, bool colonistBar = true, bool circleOverlay = false)
		{
			lookTargets?.Highlight(arrow, colonistBar, circleOverlay);
		}
	}
}
