using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld;

public class RoomContents_StockpileEntrance : RoomContentsWorker
{
	private static readonly IntRange TurretsRange = new IntRange(1, 2);

	public override void FillRoom(Map map, LayoutRoom room, Faction faction, float? threatPoints = null)
	{
		SpawnExit(map, room);
		SpawnTurrets(map, room, faction);
		base.FillRoom(map, room, faction, threatPoints);
	}

	private void SpawnExit(Map map, LayoutRoom room)
	{
		List<Thing> list = new List<Thing>();
		ThingDef ancientHatchExit = ThingDefOf.AncientHatchExit;
		List<Thing> spawned = list;
		RoomGenUtility.FillWithPadding(ancientHatchExit, 1, room, map, null, null, spawned, 3);
		MapGenerator.PlayerStartSpot = list.First().Position;
	}

	private void SpawnTurrets(Map map, LayoutRoom room, Faction faction)
	{
		RoomGenUtility.FillAroundEdges(ThingDefOf.AncientSecurityTurret, TurretsRange.RandomInRange, IntRange.One, room, map, null, null, 1, 0, null, avoidDoors: true, RotationDirection.Opposite, null, faction);
	}
}
