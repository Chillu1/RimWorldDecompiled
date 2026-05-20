using Verse;

namespace RimWorld;

public struct RitualTargetUseReport
{
	public bool canUse;

	public string failReason;

	public bool ShouldShowGizmo
	{
		get
		{
			if (!canUse)
			{
				return !failReason.NullOrEmpty();
			}
			return true;
		}
	}

	public static implicit operator RitualTargetUseReport(bool canUse)
	{
		return new RitualTargetUseReport
		{
			canUse = canUse,
			failReason = null
		};
	}

	public static implicit operator RitualTargetUseReport(string failReason)
	{
		return new RitualTargetUseReport
		{
			canUse = false,
			failReason = failReason
		};
	}

	public static implicit operator RitualTargetUseReport(TaggedString failReason)
	{
		return new RitualTargetUseReport
		{
			canUse = false,
			failReason = failReason
		};
	}
}
