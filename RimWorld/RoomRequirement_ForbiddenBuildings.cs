using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class RoomRequirement_ForbiddenBuildings : RoomRequirement
{
	public List<string> buildingTags = new List<string>();

	private HashSet<string> buildingTagsSet;

	private HashSet<string> BuildingTagsSet => buildingTagsSet ?? (buildingTagsSet = new HashSet<string>(buildingTags));

	public override bool Met(Room r, Pawn p = null)
	{
		foreach (Region region in r.Regions)
		{
			List<Thing> allThings = region.ListerThings.AllThings;
			for (int i = 0; i < allThings.Count; i++)
			{
				Thing thing = allThings[i];
				if (thing.def.building == null || thing.def.building.buildingTags == null)
				{
					continue;
				}
				foreach (string buildingTag in thing.def.building.buildingTags)
				{
					if (BuildingTagsSet.Contains(buildingTag))
					{
						return false;
					}
				}
			}
		}
		return true;
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Collections.Look(ref buildingTags, "buildingTags", LookMode.Undefined);
		if (Scribe.mode == LoadSaveMode.PostLoadInit)
		{
			buildingTagsSet = new HashSet<string>(buildingTags);
		}
	}
}
