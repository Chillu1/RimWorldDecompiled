using Verse;

namespace RimWorld
{
	public class RitualPosition_BehindThingCenter : RitualPosition_VerticalThingCenter
	{
		protected override CellRect GetRect(CellRect thingRect)
		{
			IntVec3 intVec = IntVec3.North + offset;
			return new CellRect(thingRect.minX + intVec.x, thingRect.maxZ + intVec.z, thingRect.Width, 1);
		}
	}
}
