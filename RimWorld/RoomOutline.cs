using Verse;

namespace RimWorld
{
	public class RoomOutline
	{
		public CellRect rect;

		public int CellsCountIgnoringWalls
		{
			get
			{
				if (rect.Width <= 2 || rect.Height <= 2)
				{
					return 0;
				}
				return (rect.Width - 2) * (rect.Height - 2);
			}
		}

		public RoomOutline(CellRect rect)
		{
			this.rect = rect;
		}
	}
}
