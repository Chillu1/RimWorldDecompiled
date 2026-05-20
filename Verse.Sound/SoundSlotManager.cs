using System.Collections.Generic;
using UnityEngine;

namespace Verse.Sound;

public static class SoundSlotManager
{
	private static Dictionary<string, float> allowedPlayTimes = new Dictionary<string, float>();

	public static bool CanPlayNow(string slotName)
	{
		if (slotName == "")
		{
			return true;
		}
		float value = 0f;
		if (allowedPlayTimes.TryGetValue(slotName, out value) && Time.realtimeSinceStartup < allowedPlayTimes[slotName])
		{
			return false;
		}
		return true;
	}

	public static void Notify_Played(string slot, float duration)
	{
		if (!(slot == ""))
		{
			if (allowedPlayTimes.TryGetValue(slot, out var value))
			{
				allowedPlayTimes[slot] = Mathf.Max(value, Time.realtimeSinceStartup + duration);
			}
			else
			{
				allowedPlayTimes[slot] = Time.realtimeSinceStartup + duration;
			}
		}
	}
}
