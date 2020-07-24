using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class RoomRequirement_ForbiddenBuildings : RoomRequirement
	{
		public List<string> buildingTags = new List<string>();

		public override bool Met(Room r, Pawn p = null)
		{
			foreach (Thing containedAndAdjacentThing in r.ContainedAndAdjacentThings)
			{
				if (containedAndAdjacentThing.def.building == null)
				{
					continue;
				}
				for (int i = 0; i < buildingTags.Count; i++)
				{
					string item = buildingTags[i];
					if (containedAndAdjacentThing.def.building.buildingTags.Contains(item))
					{
						return false;
					}
				}
			}
			return true;
		}
	}
}
