namespace Verse.AI
{
	public static class WanderRoomUtility
	{
		public static bool IsValidWanderDest(Pawn pawn, IntVec3 loc, IntVec3 root)
		{
			Room room = root.GetRoom(pawn.Map);
			if (room == null || room.IsDoorway)
			{
				return true;
			}
			return WanderUtility.InSameRoom(root, loc, pawn.Map);
		}
	}
}
