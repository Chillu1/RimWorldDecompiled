using Verse;

namespace RimWorld;

public class RoomContents_HermeticCrate : RoomContentsWorker
{
	private const string OpenedSignal = "OpenedSignal";

	public virtual ThingSetMakerDef ThingSetMakerDef { get; } = ThingSetMakerDefOf.MapGen_ScarlandsHermeticCrate;

	public override void FillRoom(Map map, LayoutRoom room, Faction faction, float? threatPoints = null)
	{
		ThreatSignal = "OpenedSignal" + Find.UniqueIDsManager.GetNextSignalTagID();
		RoomGenUtility.SpawnHermeticCrateInRoom(room, map, ThingSetMakerDef, addRewards: true, ThreatSignal, Rot4.Random);
		base.FillRoom(map, room, faction, threatPoints);
	}
}
