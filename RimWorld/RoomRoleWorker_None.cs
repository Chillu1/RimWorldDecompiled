using Verse;

namespace RimWorld
{
	public class RoomRoleWorker_None : RoomRoleWorker
	{
		public override float GetScore(Room room)
		{
			return -1f;
		}
	}
}
