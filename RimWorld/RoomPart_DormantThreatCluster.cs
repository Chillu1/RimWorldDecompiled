using Verse;

namespace RimWorld;

public class RoomPart_DormantThreatCluster : RoomPartWorker
{
	public RoomPart_DormantThreatCluster(RoomPartDef def)
		: base(def)
	{
	}

	public override void FillRoom(Map map, LayoutRoom room, Faction faction, float threatPoints)
	{
		RoomGenUtility.SpawnDormantThreatCluster(room, map, threatPoints, room.ThreatSignal);
	}
}
