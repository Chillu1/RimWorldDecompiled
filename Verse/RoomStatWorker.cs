namespace Verse
{
	public abstract class RoomStatWorker
	{
		public RoomStatDef def;

		public abstract float GetScore(Room room);
	}
}
