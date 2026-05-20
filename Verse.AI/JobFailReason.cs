namespace Verse.AI;

public static class JobFailReason
{
	private static string lastReason;

	private static string lastCustomJobString;

	private static bool silent;

	public static string Reason => lastReason;

	public static bool HaveReason => lastReason != null;

	public static bool Silent => silent;

	public static string CustomJobString => lastCustomJobString;

	public static void Is(string reason, string customJobString = null)
	{
		lastReason = reason;
		lastCustomJobString = customJobString;
	}

	public static void IsSilent()
	{
		silent = true;
	}

	public static void Clear()
	{
		lastReason = null;
		lastCustomJobString = null;
		silent = false;
	}
}
