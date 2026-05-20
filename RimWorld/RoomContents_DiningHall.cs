using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld;

public class RoomContents_DiningHall : RoomContentsWorker
{
	private static readonly FloatRange ChairsPerTableRange = new FloatRange(-1f, 2f);

	protected virtual ThingDef TableDef => ThingDefOf.Table2x4c;

	protected virtual ThingDef TableStuffDef => ThingDefOf.Steel;

	protected virtual ThingDef ChairDef => ThingDefOf.DiningChair;

	protected virtual ThingDef ChairStuffDef => ThingDefOf.Steel;

	public override void FillRoom(Map map, LayoutRoom room, Faction faction, float? threatPoints = null)
	{
		Rot4 value = Rot4.North;
		CellRect boundary = room.Boundary;
		if (boundary.Height > boundary.Width)
		{
			value = Rot4.East;
		}
		List<Thing> spawned = new List<Thing>();
		RoomGenUtility.FillWithPadding(TableDef, GetCount(room), room, map, value, null, spawned, 1, TableStuffDef);
		SpawnChairs(map, spawned);
		base.FillRoom(map, room, faction, threatPoints);
	}

	private void SpawnChairs(Map map, List<Thing> spawned)
	{
		foreach (Thing item in spawned)
		{
			CellRect cellRect = item.OccupiedRect().ExpandedBy(1);
			float randomInRange = ChairsPerTableRange.RandomInRange;
			int num = 0;
			foreach (IntVec3 item2 in cellRect.EdgeCellsNoCorners.InRandomOrder())
			{
				if (!((float)num >= randomInRange))
				{
					Rot4 rot = Rot4.FromAngleFlat((item.Position - item2).AngleFlat);
					if (GenSpawn.CanSpawnAt(ChairDef, item2, map, rot))
					{
						GenSpawn.Spawn(ThingMaker.MakeThing(ChairDef, ChairStuffDef), item2, map, rot);
						num++;
					}
				}
			}
		}
	}

	private int GetCount(LayoutRoom room)
	{
		float num = (float)room.Area / 100f;
		return Mathf.Max(Mathf.RoundToInt((base.RoomDef.itemsPer100CellsRange?.RandomInRange ?? 1f) * num), 1);
	}
}
