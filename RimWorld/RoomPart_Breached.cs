using UnityEngine;
using Verse;

namespace RimWorld;

public class RoomPart_Breached : RoomPartWorker
{
	private static readonly IntRange CellsToDestroy = new IntRange(2, 4);

	private static readonly IntRange RadialDamageRange = new IntRange(2, 4);

	private static readonly FloatRange DamageRangePercent = new FloatRange(0.4f, 0.8f);

	private static readonly IntRange FilthGroupRange = new IntRange(3, 6);

	private static readonly IntRange FilthDistanceRange = new IntRange(3, 6);

	public override bool FillOnPost => true;

	public RoomPart_Breached(RoomPartDef def)
		: base(def)
	{
	}

	public override void FillRoom(Map map, LayoutRoom room, Faction faction, float threatPoints)
	{
		if (!CanRemoveWalls(room))
		{
			return;
		}
		foreach (CellRect rect in room.rects)
		{
			int num = Rand.Range(0, 4);
			for (int i = 0; i < 4; i++)
			{
				Rot4 rot = new Rot4(i + num);
				CellRect edgeRect = rect.GetEdgeRect(rot);
				bool flag = true;
				foreach (IntVec3 cell in edgeRect.Cells)
				{
					for (int j = 0; j < 4; j++)
					{
						IntVec3 intVec = cell + GenAdj.CardinalDirections[j];
						if (!room.Contains(intVec) && room.sketch.structureLayout.TryGetRoom(intVec, out var room2) && !CanRemoveWalls(room2))
						{
							flag = false;
							break;
						}
					}
				}
				if (flag)
				{
					DestroyWall(map, edgeRect, rot);
					return;
				}
			}
		}
	}

	private static bool CanRemoveWalls(LayoutRoom room)
	{
		if (room.requiredDef != null)
		{
			return false;
		}
		foreach (LayoutRoomDef def in room.defs)
		{
			if (def.canRemoveBorderDoors || def.canRemoveBorderWalls)
			{
				return false;
			}
		}
		return true;
	}

	private void DestroyWall(Map map, CellRect edge, Rot4 rot)
	{
		int num = edge.GetSideLength(rot) / 2 - 2;
		if (num <= 2)
		{
			return;
		}
		int num2 = Mathf.Min(CellsToDestroy.RandomInRange, num);
		int num3 = Mathf.Min(CellsToDestroy.RandomInRange, num);
		IntVec3 first = edge.CenterCell - rot.RighthandCell * num2;
		IntVec3 second = edge.CenterCell + rot.RighthandCell * num3;
		CellRect cellRect = CellRect.FromLimits(first, second);
		foreach (IntVec3 cell in cellRect.Cells)
		{
			Building edifice = cell.GetEdifice(map);
			if (edifice != null)
			{
				if (cellRect.IsCorner(cell))
				{
					edifice.HitPoints = (int)((float)edifice.MaxHitPoints * DamageRangePercent.RandomInRange);
				}
				else
				{
					edifice.Destroy();
				}
			}
		}
		int randomInRange = RadialDamageRange.RandomInRange;
		CellRect cellRect2 = cellRect.CenterCell.RectAbout(randomInRange, randomInRange);
		foreach (IntVec3 cell2 in cellRect2.Cells)
		{
			if (!cellRect2.IsCorner(cell2))
			{
				Building edifice2 = cell2.GetEdifice(map);
				if (edifice2 != null)
				{
					edifice2.HitPoints = (int)((float)edifice2.MaxHitPoints * DamageRangePercent.RandomInRange);
				}
			}
		}
		GenSpawn.SpawnIrregularLump(ThingDefOf.Filth_RubbleBuilding, cellRect.CenterCell, map, FilthGroupRange, FilthDistanceRange);
		GenSpawn.SpawnIrregularLump(ThingDefOf.Filth_RubbleBuilding, cellRect.RandomCell, map, FilthGroupRange, FilthDistanceRange);
		FilthMaker.TryMakeFilth(cellRect.CenterCell, map, ThingDefOf.Filth_BlastMark);
	}
}
