using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld;

public class PlaceWorker_DrawLinesToBookcasesInRoom : PlaceWorker
{
	private static readonly List<Thing> tmpLinkedThings = new List<Thing>();

	public override void DrawGhost(ThingDef def, IntVec3 center, Rot4 rot, Color ghostCol, Thing thing = null)
	{
		Room room = center.GetRoom(Find.CurrentMap);
		if (room == null || !room.ProperRoom || room.PsychologicallyOutdoors)
		{
			return;
		}
		room.DrawFieldEdges();
		foreach (Region region in room.Regions)
		{
			foreach (Thing item in region.ListerThings.ThingsInGroup(ThingRequestGroup.BuildingArtificial))
			{
				if (item is Building_Bookcase)
				{
					GenDraw.DrawLineBetween(center.ToVector3Shifted(), item.TrueCenter());
				}
			}
		}
	}

	public override void DrawPlaceMouseAttachments(float curX, ref float curY, BuildableDef bdef, IntVec3 center, Rot4 rot)
	{
		tmpLinkedThings.Clear();
		Room room = center.GetRoom(Find.CurrentMap);
		if (room != null)
		{
			foreach (Region region in room.Regions)
			{
				foreach (Thing item in region.ListerThings.ThingsInGroup(ThingRequestGroup.BuildingArtificial))
				{
					if (!tmpLinkedThings.Contains(item) && item is Building_Bookcase)
					{
						tmpLinkedThings.Add(item);
						if (tmpLinkedThings.Count == 1)
						{
							PlaceWorker.DrawTextLine(curX, ref curY, "FacilityPotentiallyLinkedTo".Translate() + ":");
						}
						PlaceWorker.DrawTextLine(curX, ref curY, "  - " + item.LabelCap);
					}
				}
			}
		}
		base.DrawPlaceMouseAttachments(curX, ref curY, bdef, center, rot);
	}
}
