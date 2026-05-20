using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld;

public abstract class GenStep_ScattererBestFit : GenStep_Scatterer
{
	private const int DistanceStep = 5;

	private const int AttemptsPerStep = 5;

	private static IntVec3? bestCellWithLeastOccupiedCollisions;

	private static int bestOccupiedScore;

	protected abstract IntVec2 Size { get; }

	public abstract bool CollisionAt(IntVec3 cell, Map map);

	protected override bool CanScatterAt(IntVec3 c, Map map)
	{
		if (!base.CanScatterAt(c, map))
		{
			return false;
		}
		if (!c.Standable(map))
		{
			return false;
		}
		if (c.Roofed(map))
		{
			return false;
		}
		if (!map.reachability.CanReachMapEdge(c, TraverseParms.For(TraverseMode.PassDoors)))
		{
			return false;
		}
		CellRect cellRect = CellRect.CenteredOn(c, Size.x, Size.z);
		if (!cellRect.FullyContainedWithin(CellRect.WholeMap(map)))
		{
			return false;
		}
		int num = 0;
		foreach (IntVec3 item in cellRect)
		{
			if (!item.InBounds(map))
			{
				return false;
			}
			List<Thing> thingList = item.GetThingList(map);
			for (int i = 0; i < thingList.Count; i++)
			{
				if (!thingList[i].def.destroyable)
				{
					return false;
				}
			}
			if (CollisionAt(item, map))
			{
				num++;
			}
		}
		if (num < bestOccupiedScore)
		{
			bestOccupiedScore = num;
			bestCellWithLeastOccupiedCollisions = c;
		}
		return num == 0;
	}

	protected override bool TryFindScatterCell(Map map, out IntVec3 result)
	{
		bestOccupiedScore = int.MaxValue;
		bestCellWithLeastOccupiedCollisions = null;
		if (CanScatterAt(map.Center, map))
		{
			result = map.Center;
			return true;
		}
		int num = Mathf.FloorToInt((float)Mathf.Min(map.Size.x, map.Size.z) / 2f);
		Vector3 v = IntVec3.North.ToVector3();
		for (int i = 5; i <= num; i += 5)
		{
			for (int j = 0; j < 5; j++)
			{
				float angle = Rand.Range(0f, 360f);
				Vector3 vector = v.RotatedBy(angle);
				int num2 = Rand.Range(i - 5, i);
				IntVec3 intVec = map.Center + (vector * num2).ToIntVec3();
				if (CanScatterAt(intVec, map))
				{
					result = intVec;
					return true;
				}
			}
		}
		if (bestCellWithLeastOccupiedCollisions.HasValue)
		{
			result = bestCellWithLeastOccupiedCollisions.Value;
			return true;
		}
		if (warnOnFail)
		{
			Log.Warning("Scatterer " + ToString() + " from def " + def.defName + " could not find cell to generate at.");
		}
		result = IntVec3.Invalid;
		return false;
	}
}
