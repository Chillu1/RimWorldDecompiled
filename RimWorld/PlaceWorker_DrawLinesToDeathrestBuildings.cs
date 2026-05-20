using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class PlaceWorker_DrawLinesToDeathrestBuildings : PlaceWorker
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
					if (CanShowConnectionTo(def, item, center))
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
							if (CanShowConnectionTo((ThingDef)bdef, item, center) && !tmpLinkedThings.Contains(item))
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

		private bool CanShowConnectionTo(ThingDef def, Thing t, IntVec3 center)
		{
			if (t.def == def)
			{
				return false;
			}
			CompDeathrestBindable compDeathrestBindable = t.TryGetComp<CompDeathrestBindable>();
			if (compDeathrestBindable == null)
			{
				return false;
			}
			if (compDeathrestBindable.BoundPawn != null)
			{
				return false;
			}
			if (!GenSight.LineOfSight(center, t.OccupiedRect().CenterCell, Find.CurrentMap))
			{
				return false;
			}
			return true;
		}
	}
}
