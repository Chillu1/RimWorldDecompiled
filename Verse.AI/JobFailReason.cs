namespace Verse.AI
{
	public static class JobFailReason
	{
		private static string lastReason;

		private static string lastCustomJobString;

		public static string Reason => lastReason;

		public static bool HaveReason => lastReason != null;

		public static string CustomJobString => lastCustomJobString;

		public static void Is(string reason, string customJobString = null)
		{
			lastReason = reason;
			lastCustomJobString = customJobString;
		}

		public static void Clear()
		{
			lastReason = null;
			lastCustomJobString = null;
		}
	}
}
