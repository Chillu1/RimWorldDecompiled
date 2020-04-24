using Verse;

namespace RimWorld
{
	public class RoomRequirement_HasAssignedThroneAnyOf : RoomRequirement_ThingAnyOf
	{
		public override bool Met(Room r, Pawn p = null)
		{
			if (p == null)
			{
				return false;
			}
			foreach (Thing containedAndAdjacentThing in r.ContainedAndAdjacentThings)
			{
				if (things.Contains(containedAndAdjacentThing.def) && p.ownership.AssignedThrone == containedAndAdjacentThing)
				{
					return true;
				}
			}
			return false;
		}
	}
}
