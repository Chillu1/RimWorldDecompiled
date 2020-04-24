namespace RimWorld.Planet
{
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
	}
}
