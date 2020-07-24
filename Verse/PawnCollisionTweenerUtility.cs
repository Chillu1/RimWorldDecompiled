using RimWorld;
using System.Collections.Generic;
using UnityEngine;

namespace Verse
{
	public static class PawnCollisionTweenerUtility
	{
		private const float Radius = 0.32f;

		public static Vector3 PawnCollisionPosOffsetFor(Pawn pawn)
		{
			if (pawn.GetPosture() != 0)
			{
				return Vector3.zero;
			}
			bool flag = pawn.Spawned && pawn.pather.MovingNow;
			if (!flag || pawn.pather.nextCell == pawn.pather.Destination.Cell)
			{
				if (!flag && pawn.Drawer.leaner.ShouldLean())
				{
					return Vector3.zero;
				}
				IntVec3 at = (!flag) ? pawn.Position : pawn.pather.nextCell;
				GetPawnsStandingAtOrAboutToStandAt(at, pawn.Map, out int pawnsCount, out int pawnsWithLowerIdCount, out bool forPawnFound, pawn);
				if (!forPawnFound)
				{
					return Vector3.zero;
				}
				return GenGeo.RegularPolygonVertexPositionVec3(pawnsCount, pawnsWithLowerIdCount) * 0.32f;
			}
			IntVec3 nextCell = pawn.pather.nextCell;
			if (CanGoDirectlyToNextCell(pawn))
			{
				return Vector3.zero;
			}
			int num = pawn.thingIDNumber % 2;
			if (nextCell.x != pawn.Position.x)
			{
				if (num == 0)
				{
					return new Vector3(0f, 0f, 0.32f);
				}
				return new Vector3(0f, 0f, -0.32f);
			}
			if (num == 0)
			{
				return new Vector3(0.32f, 0f, 0f);
			}
			return new Vector3(-0.32f, 0f, 0f);
		}

		private static void GetPawnsStandingAtOrAboutToStandAt(IntVec3 at, Map map, out int pawnsCount, out int pawnsWithLowerIdCount, out bool forPawnFound, Pawn forPawn)
		{
			pawnsCount = 0;
			pawnsWithLowerIdCount = 0;
			forPawnFound = false;
			foreach (IntVec3 item in CellRect.SingleCell(at).ExpandedBy(1))
			{
				if (!item.InBounds(map))
				{
					continue;
				}
				List<Thing> thingList = item.GetThingList(map);
				for (int i = 0; i < thingList.Count; i++)
				{
					Pawn pawn = thingList[i] as Pawn;
					if (pawn == null || pawn.GetPosture() != 0)
					{
						continue;
					}
					if (item != at)
					{
						if (!pawn.pather.MovingNow || pawn.pather.nextCell != pawn.pather.Destination.Cell || pawn.pather.Destination.Cell != at)
						{
							continue;
						}
					}
					else if (pawn.pather.MovingNow)
					{
						continue;
					}
					if (pawn == forPawn)
					{
						forPawnFound = true;
					}
					pawnsCount++;
					if (pawn.thingIDNumber < forPawn.thingIDNumber)
					{
						pawnsWithLowerIdCount++;
					}
				}
			}
		}

		private static bool CanGoDirectlyToNextCell(Pawn pawn)
		{
			IntVec3 nextCell = pawn.pather.nextCell;
			foreach (IntVec3 item in CellRect.FromLimits(nextCell, pawn.Position).ExpandedBy(1))
			{
				if (!item.InBounds(pawn.Map))
				{
					continue;
				}
				List<Thing> thingList = item.GetThingList(pawn.Map);
				for (int i = 0; i < thingList.Count; i++)
				{
					Pawn pawn2 = thingList[i] as Pawn;
					if (pawn2 == null || pawn2 == pawn || pawn2.GetPosture() != 0)
					{
						continue;
					}
					if (pawn2.pather.MovingNow)
					{
						if (((pawn2.Position == nextCell && WillBeFasterOnNextCell(pawn, pawn2)) || pawn2.pather.nextCell == nextCell || pawn2.Position == pawn.Position || (pawn2.pather.nextCell == pawn.Position && WillBeFasterOnNextCell(pawn2, pawn))) && pawn2.thingIDNumber < pawn.thingIDNumber)
						{
							return false;
						}
					}
					else if (pawn2.Position == pawn.Position || pawn2.Position == nextCell)
					{
						return false;
					}
				}
			}
			return true;
		}

		private static bool WillBeFasterOnNextCell(Pawn p1, Pawn p2)
		{
			if (p1.pather.nextCellCostLeft == p2.pather.nextCellCostLeft)
			{
				return p1.thingIDNumber < p2.thingIDNumber;
			}
			return p1.pather.nextCellCostLeft < p2.pather.nextCellCostLeft;
		}
	}
}
