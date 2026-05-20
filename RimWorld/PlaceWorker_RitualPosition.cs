using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld;

public class PlaceWorker_RitualPosition : PlaceWorker
{
	public const float RitualFocusRange = 4.9f;

	public const SimpleColor RitualFocusConnectionColor = SimpleColor.Green;

	public const SimpleColor RitualSeatConnectionColor = SimpleColor.Yellow;

	private static List<Thing> tmpRitualSeatsInRoom = new List<Thing>();

	public static List<Thing> GetRitualFocusInRange(IntVec3 ritualPosition, Thing ritualPositionThing = null)
	{
		return Find.CurrentMap.listerBuildingWithTagInProximity.GetForCell(ritualPosition, 4.9f, "RitualFocus", ritualPositionThing);
	}

	public static IEnumerable<Thing> RitualSeatsInRange(ThingDef def, IntVec3 center, Rot4 rot, Thing thing = null)
	{
		try
		{
			List<Thing> buildings;
			if (GatheringsUtility.UseWholeRoomAsGatheringArea(center, Find.CurrentMap))
			{
				buildings = tmpRitualSeatsInRoom;
				foreach (Thing containedAndAdjacentThing in center.GetRoom(Find.CurrentMap).ContainedAndAdjacentThings)
				{
					if (containedAndAdjacentThing.TryGetComp<CompRitualSeat>() != null)
					{
						buildings.Add(containedAndAdjacentThing);
					}
				}
			}
			else
			{
				buildings = Find.CurrentMap.listerBuildingWithTagInProximity.GetForCell(center, 18f, "RitualSeat", thing);
			}
			for (int i = 0; i < buildings.Count; i++)
			{
				Thing thing2 = buildings[i];
				if (GatheringsUtility.InGatheringArea(thing2.Position, center, Find.CurrentMap) && SpectatorCellFinder.CorrectlyRotatedChairAt(thing2.Position, Find.CurrentMap, GenAdj.OccupiedRect(center, rot, def.size)))
				{
					yield return thing2;
				}
			}
		}
		finally
		{
			tmpRitualSeatsInRoom.Clear();
		}
	}

	public static void DrawRitualSeatConnections(ThingDef def, IntVec3 center, Rot4 rot, Thing thing = null, List<Thing> except = null)
	{
		foreach (Thing item in RitualSeatsInRange(def, center, rot, thing))
		{
			if (except == null || !except.Contains(item))
			{
				GenDraw.DrawLineBetween(GenThing.TrueCenter(center, rot, def.size, AltitudeLayer.MetaOverlays.AltitudeFor()), item.TrueCenter(), SimpleColor.Yellow);
			}
		}
	}

	public override void DrawGhost(ThingDef def, IntVec3 center, Rot4 rot, Color ghostCol, Thing thing = null)
	{
		List<Thing> ritualFocusInRange = GetRitualFocusInRange(center, thing);
		for (int i = 0; i < ritualFocusInRange.Count; i++)
		{
			Thing thing2 = ritualFocusInRange[i];
			if (thing2.def != def)
			{
				GenDraw.DrawLineBetween(GenThing.TrueCenter(center, Rot4.North, def.size, def.Altitude), thing2.TrueCenter(), SimpleColor.Green);
			}
		}
		DrawRitualSeatConnections(def, center, rot, thing);
	}
}
