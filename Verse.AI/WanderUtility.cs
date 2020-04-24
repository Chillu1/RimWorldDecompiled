namespace Verse.AI
{
	public static class WanderUtility
	{
		public static IntVec3 BestCloseWanderRoot(IntVec3 trueWanderRoot, Pawn pawn)
		{
			for (int i = 0; i < 50; i++)
			{
				IntVec3 intVec = (i >= 8) ? (trueWanderRoot + GenRadial.RadialPattern[i - 8 + 1] * 7) : (trueWanderRoot + GenRadial.RadialPattern[i]);
				if (intVec.InBounds(pawn.Map) && intVec.Walkable(pawn.Map) && pawn.CanReach(intVec, PathEndMode.OnCell, Danger.Some))
				{
					return intVec;
				}
			}
			return IntVec3.Invalid;
		}

		public static bool InSameRoom(IntVec3 locA, IntVec3 locB, Map map)
		{
			Room room = locA.GetRoom(map, RegionType.Set_All);
			if (room == null)
			{
				return true;
			}
			return room == locB.GetRoom(map, RegionType.Set_All);
		}
	}
}
