using Verse;

namespace RimWorld;

public class RoomPart_DormantInsectCluster : RoomPartWorker
{
	public RoomPart_DormantInsectCluster(RoomPartDef def)
		: base(def)
	{
	}

	public override void FillRoom(Map map, LayoutRoom room, Faction faction, float threatPoints)
	{
		if (Faction.OfInsects != null)
		{
			RoomGenUtility.SpawnDormantInsectCluster(room, map, threatPoints, room.ThreatSignal);
		}
	}
}
