using UnityEngine;

namespace RimWorld.Planet;

public static class EnterCooldownCompUtility
{
	public static bool EnterCooldownBlocksEntering(this MapParent worldObject)
	{
		return worldObject.GetComponent<EnterCooldownComp>()?.BlocksEntering ?? false;
	}

	public static float EnterCooldownDaysLeft(this MapParent worldObject)
	{
		return worldObject.GetComponent<EnterCooldownComp>()?.DaysLeft ?? 0f;
	}

	public static float EnterCooldownHoursLeft(this MapParent worldObject)
	{
		EnterCooldownComp component = worldObject.GetComponent<EnterCooldownComp>();
		if (component == null)
		{
			return 0f;
		}
		return component.DaysLeft * 24f;
	}

	public static int EnterCooldownTicksLeft(this MapParent worldObject)
	{
		EnterCooldownComp component = worldObject.GetComponent<EnterCooldownComp>();
		return Mathf.CeilToInt((component != null) ? (component.DaysLeft * 60000f) : 0f);
	}
}
