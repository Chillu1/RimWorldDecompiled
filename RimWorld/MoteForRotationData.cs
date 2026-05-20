using Verse;

namespace RimWorld
{
	public class MoteForRotationData
	{
		public ThingDef north;

		public ThingDef south;

		public ThingDef east;

		public ThingDef west;

		public ThingDef GetForRotation(Rot4 rot)
		{
			return rot.AsInt switch
			{
				0 => north, 
				1 => east, 
				2 => south, 
				3 => west, 
				_ => null, 
			};
		}
	}
}
