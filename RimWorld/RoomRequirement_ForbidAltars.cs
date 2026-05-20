using Verse;

namespace RimWorld
{
	public class RoomRequirement_ForbidAltars : RoomRequirement
	{
		public override bool Met(Room r, Pawn p = null)
		{
			foreach (Thing containedAndAdjacentThing in r.ContainedAndAdjacentThings)
			{
				if (containedAndAdjacentThing.def.isAltar)
				{
					return false;
				}
			}
			return true;
		}
	}
}
