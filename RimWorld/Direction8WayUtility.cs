using Verse;

namespace RimWorld
{
	public static class Direction8WayUtility
	{
		public static string LabelShort(this Direction8Way dir)
		{
			return dir switch
			{
				Direction8Way.North => "Direction8Way_North_Short".Translate(), 
				Direction8Way.NorthEast => "Direction8Way_NorthEast_Short".Translate(), 
				Direction8Way.East => "Direction8Way_East_Short".Translate(), 
				Direction8Way.SouthEast => "Direction8Way_SouthEast_Short".Translate(), 
				Direction8Way.South => "Direction8Way_South_Short".Translate(), 
				Direction8Way.SouthWest => "Direction8Way_SouthWest_Short".Translate(), 
				Direction8Way.West => "Direction8Way_West_Short".Translate(), 
				Direction8Way.NorthWest => "Direction8Way_NorthWest_Short".Translate(), 
				_ => "Unknown Direction8Way", 
			};
		}
	}
}
