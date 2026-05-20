using Verse;

namespace RimWorld;

public class RoomContents_LaunchPad : RoomContentsWorker
{
	private const float PodSpawnChance = 0.3f;

	public override void FillRoom(Map map, LayoutRoom room, Faction faction, float? threatPoints = null)
	{
		CellRect cellRect = room.rects[0];
		CellRect cellRect2 = cellRect.ContractedBy(2);
		for (int num = cellRect2.Width - 1; num >= 0; num--)
		{
			for (int num2 = cellRect2.Height - 1; num2 >= 0; num2--)
			{
				IntVec3 intVec = new IntVec3(num, 0, num2);
				IntVec3 intVec2 = cellRect2.Min + intVec;
				map.terrainGrid.SetTerrain(intVec2, TerrainDefOf.AncientTile);
				if (intVec.z % 2 == 0 && intVec.x % 2 == 0 && Rand.Chance(0.3f))
				{
					GenSpawn.Spawn(ThingDefOf.MechanoidDropPod, intVec2, map);
				}
			}
		}
		foreach (IntVec3 corner in cellRect.ContractedBy(1).Corners)
		{
			GenSpawn.Spawn(ThingDefOf.AncientShipBeacon, corner, map);
		}
		base.FillRoom(map, room, faction, threatPoints);
	}
}
