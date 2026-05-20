using UnityEngine;
using Verse;

namespace RimWorld;

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

	public static float AsAngle(this Direction8Way dir)
	{
		return dir switch
		{
			Direction8Way.North => 0f, 
			Direction8Way.NorthEast => 45f, 
			Direction8Way.East => 90f, 
			Direction8Way.SouthEast => 135f, 
			Direction8Way.South => 180f, 
			Direction8Way.SouthWest => 225f, 
			Direction8Way.West => 270f, 
			Direction8Way.NorthWest => 315f, 
			_ => float.MaxValue, 
		};
	}

	public static Vector3 AsVector(this Direction8Way dir)
	{
		return Quaternion.AngleAxis(dir.AsAngle(), Vector3.up) * Vector3.forward;
	}
}
