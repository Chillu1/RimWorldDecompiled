namespace Verse
{
	public class PostTickVisuals
	{
		private Map map;

		public PostTickVisuals(Map map)
		{
			this.map = map;
		}

		public void ProcessPostTickVisuals()
		{
			int ticksThisFrame = Find.TickManager.TicksThisFrame;
			if (ticksThisFrame <= 0)
			{
				return;
			}
			CellRect viewRect = Find.CameraDriver.CurrentViewRect.ExpandedBy(3);
			foreach (Pawn item in map.mapPawns.AllPawnsSpawned)
			{
				item.ProcessPostTickVisuals(ticksThisFrame, viewRect);
			}
		}
	}
}
