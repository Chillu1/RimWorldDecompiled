using Verse;

namespace RimWorld;

public class RoomContents_CryptosleepCasket : RoomContentsWorker
{
	private const string OpenedSignal = "OpenedSignal";

	public override void FillRoom(Map map, LayoutRoom room, Faction faction, float? threatPoints = null)
	{
		if (!room.TryGetRandomCellInRoom(map, out var cell, 3))
		{
			cell = room.rects[0].CenterCell;
		}
		int nextAncientCryptosleepCasketGroupID = Find.UniqueIDsManager.GetNextAncientCryptosleepCasketGroupID();
		PodContentsType type = Gen.RandomEnumValue<PodContentsType>(disallowFirstValue: true);
		Building_AncientCryptosleepCasket pod = RoomGenUtility.SpawnCryptoCasket(cell, map, Rot4.Random, nextAncientCryptosleepCasketGroupID, type, ThingSetMakerDefOf.MapGen_ScarlandsAncientPodContents);
		ThreatSignal = "OpenedSignal" + Find.UniqueIDsManager.GetNextSignalTagID();
		RoomGenUtility.SpawnOpenCryptoCasketSignal(pod, map, ThreatSignal);
		base.FillRoom(map, room, faction, threatPoints);
	}
}
