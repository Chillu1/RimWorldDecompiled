using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld;

public class RoomContents_MechChargingRoom : RoomContentsWorker
{
	private static readonly FloatRange StandardRechargersPer10EdgeCells = new FloatRange(0.2f, 1f);

	private static readonly IntRange StandardRechargersGroupSize = new IntRange(1, 1);

	private static readonly FloatRange BasicRechargersPer10EdgeCells = new FloatRange(0.5f, 1.5f);

	private static readonly IntRange BasicRechargersGroupSize = new IntRange(1, 2);

	public override void FillRoom(Map map, LayoutRoom room, Faction faction, float? threatPoints = null)
	{
		base.FillRoom(map, room, faction, threatPoints);
		SpawnMechRechargers(map, room);
	}

	private static void SpawnMechRechargers(Map map, LayoutRoom room)
	{
		float num = (float)room.rects.Sum((CellRect r) => r.ContractedBy(1).EdgeCellsCount) / 10f;
		int count = Mathf.Max(Mathf.RoundToInt(StandardRechargersPer10EdgeCells.RandomInRange * num), 1);
		RoomGenUtility.FillAroundEdges(ThingDefOf.AncientStandardRecharger, count, StandardRechargersGroupSize, room, map);
		count = Mathf.Max(Mathf.RoundToInt(BasicRechargersPer10EdgeCells.RandomInRange * num), 1);
		RoomGenUtility.FillAroundEdges(ThingDefOf.AncientBasicRecharger, count, BasicRechargersGroupSize, room, map);
	}
}
