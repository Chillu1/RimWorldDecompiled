using Verse;

namespace RimWorld;

public class RoomContents_Harbinger : RoomContentsWorker
{
	private static readonly IntRange DirtAmount = new IntRange(10, 14);

	private static readonly FloatRange TreeGrowthRange = new FloatRange(0.5f, 0.9f);

	public override void FillRoom(Map map, LayoutRoom room, Faction faction, float? threatPoints = null)
	{
		base.FillRoom(map, room, faction, threatPoints);
		if (!room.TryGetRandomCellInRoom(map, out var cell, 2))
		{
			return;
		}
		((Plant)GenSpawn.Spawn(ThingMaker.MakeThing(ThingDefOf.Plant_TreeHarbinger), cell, map)).Growth = TreeGrowthRange.RandomInRange;
		foreach (IntVec3 item in GridShapeMaker.IrregularLump(cell, map, DirtAmount.RandomInRange, (IntVec3 x) => room.Contains(x, 1)))
		{
			map.terrainGrid.SetTerrain(item, TerrainDefOf.Soil);
		}
	}
}
