using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class PlaceWorker_DrawLinesToDeathrestCaskets : PlaceWorker
	{
		private static List<Thing> tmpLinkedThings = new List<Thing>();

		public override void DrawGhost(ThingDef def, IntVec3 center, Rot4 rot, Color ghostCol, Thing thing = null)
		{
			if (!ModsConfig.BiotechActive)
			{
				return;
			}
			Room room = center.GetRoom(Find.CurrentMap);
			if (room == null)
			{
				return;
			}
			foreach (Region region in room.Regions)
			{
				foreach (Thing item in region.ListerThings.ThingsInGroup(ThingRequestGroup.BuildingArtificial))
				{
					if (item.def == ThingDefOf.DeathrestCasket && GenSight.LineOfSight(center, item.OccupiedRect().CenterCell, Find.CurrentMap))
					{
						GenDraw.DrawLineBetween(center.ToVector3Shifted(), item.TrueCenter());
					}
				}
			}
		}

		public override void DrawPlaceMouseAttachments(float curX, ref float curY, BuildableDef bdef, IntVec3 center, Rot4 rot)
		{
			if (ModsConfig.BiotechActive)
			{
				tmpLinkedThings.Clear();
				Room room = center.GetRoom(Find.CurrentMap);
				if (room != null)
				{
					foreach (Region region in room.Regions)
					{
						foreach (Thing item in region.ListerThings.ThingsInGroup(ThingRequestGroup.BuildingArtificial))
						{
							if (!tmpLinkedThings.Contains(item) && item.def == ThingDefOf.DeathrestCasket && GenSight.LineOfSight(center, item.OccupiedRect().CenterCell, Find.CurrentMap))
							{
								tmpLinkedThings.Add(item);
								if (tmpLinkedThings.Count == 1)
								{
									DrawTextLine(ref curY, "FacilityPotentiallyLinkedTo".Translate() + ":");
								}
								DrawTextLine(ref curY, "  - " + item.LabelCap);
							}
						}
					}
				}
			}
			base.DrawPlaceMouseAttachments(curX, ref curY, bdef, center, rot);
			void DrawTextLine(ref float y, string text)
			{
				float lineHeight = Text.LineHeight;
				Widgets.Label(new Rect(curX, y, 999f, lineHeight), text);
				y += lineHeight;
			}
		}
	}
}
