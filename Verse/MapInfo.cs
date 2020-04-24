using RimWorld.Planet;

namespace Verse
{
	public sealed class MapInfo : IExposable
	{
		private IntVec3 sizeInt;

		public MapParent parent;

		public int Tile => parent.Tile;

		public int NumCells => Size.x * Size.y * Size.z;

		public IntVec3 Size
		{
			get
			{
				return sizeInt;
			}
			set
			{
				sizeInt = value;
			}
		}

		public void ExposeData()
		{
			Scribe_Values.Look(ref sizeInt, "size");
			Scribe_References.Look(ref parent, "parent");
		}
	}
}
