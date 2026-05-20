using System.Collections.Generic;

namespace Verse;

public static class MessagesRepeatAvoider
{
	private static Dictionary<string, float> lastShowTimes = new Dictionary<string, float>();

	public static void Reset()
	{
		lastShowTimes.Clear();
	}

	public static bool MessageShowAllowed(string tag, float minSecondsSinceLastShow)
	{
		if (!lastShowTimes.TryGetValue(tag, out var value))
		{
			value = -99999f;
		}
		bool num = RealTime.LastRealTime > value + minSecondsSinceLastShow;
		if (num)
		{
			lastShowTimes[tag] = RealTime.LastRealTime;
		}
		return num;
	}
}
