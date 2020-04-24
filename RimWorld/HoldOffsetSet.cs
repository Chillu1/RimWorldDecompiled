using Verse;

namespace RimWorld
{
	public class HoldOffsetSet
	{
		public HoldOffset northDefault;

		public HoldOffset east;

		public HoldOffset south;

		public HoldOffset west;

		public HoldOffset Pick(Rot4 rotation)
		{
			if (rotation == Rot4.North)
			{
				return northDefault;
			}
			if (rotation == Rot4.East)
			{
				return east;
			}
			if (rotation == Rot4.South)
			{
				return south;
			}
			if (rotation == Rot4.West)
			{
				return west;
			}
			return null;
		}
	}
}
