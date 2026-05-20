using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class ThoughtWorker_Precept_AltarSharing : ThoughtWorker_Precept
{
	private static Dictionary<Room, Building> tmpPawnIdeoBuildingRooms = new Dictionary<Room, Building>();

	private static HashSet<Room> tmpDifferentIdeoStyleRooms = new HashSet<Room>();

	protected override ThoughtState ShouldHaveThought(Pawn p)
	{
		return SharedAltar(p) != null;
	}

	public override string PostProcessDescription(Pawn p, string description)
	{
		return description.Formatted(SharedAltar(p).Named("ALTAR"));
	}

	private Thing SharedAltar(Pawn pawn)
	{
		if (!pawn.Spawned || pawn.Ideo == null)
		{
			return null;
		}
		tmpPawnIdeoBuildingRooms.Clear();
		tmpDifferentIdeoStyleRooms.Clear();
		foreach (Thing item in pawn.Map.listerThings.ThingsMatching(ThingRequest.ForGroup(ThingRequestGroup.Styleable)))
		{
			CompStyleable compStyleable = ((ThingWithComps)item).compStyleable;
			if (compStyleable.Ideo == null || item.DestroyedOrNull() || item.Map != pawn.Map)
			{
				continue;
			}
			if (compStyleable.Ideo == pawn.Ideo)
			{
				if (!(item is Building building))
				{
					continue;
				}
				Room room = building.GetRoom();
				if (room != null && !room.TouchesMapEdge)
				{
					if (tmpDifferentIdeoStyleRooms.Contains(room))
					{
						return building;
					}
					tmpPawnIdeoBuildingRooms[room] = building;
				}
				continue;
			}
			Room room2 = item.GetRoom();
			if (room2 != null && !room2.TouchesMapEdge)
			{
				if (tmpPawnIdeoBuildingRooms.TryGetValue(room2, out var value))
				{
					return value;
				}
				tmpDifferentIdeoStyleRooms.Add(room2);
			}
		}
		return null;
	}
}
