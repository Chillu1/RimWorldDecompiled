using Verse;

namespace RimWorld;

public class RoomPart_DormantMechCluster : RoomPartWorker
{
	public RoomPart_DormantMechCluster(RoomPartDef def)
		: base(def)
	{
	}

	public override void FillRoom(Map map, LayoutRoom room, Faction faction, float threatPoints)
	{
		if (Faction.OfMechanoids != null)
		{
			RoomGenUtility.SpawnDormantMechCluster(room, map, threatPoints, room.ThreatSignal);
		}
	}
}
