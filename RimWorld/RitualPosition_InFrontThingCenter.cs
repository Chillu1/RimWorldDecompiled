using Verse;

namespace RimWorld
{
	public class RitualPosition_InFrontThingCenter : RitualPosition_VerticalThingCenter
	{
		protected override CellRect GetRect(CellRect thingRect)
		{
			IntVec3 intVec = IntVec3.South + offset;
			return new CellRect(thingRect.minX + intVec.x, thingRect.minZ + intVec.z, thingRect.Width, 1);
		}
	}
}
