using Verse;

namespace RimWorld
{
	public class RoomRoleWorker_Room : RoomRoleWorker
	{
		public override float GetScore(Room room)
		{
			return 0.99f;
		}
	}
}
