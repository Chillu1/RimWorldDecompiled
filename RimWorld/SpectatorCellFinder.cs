using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public static class SpectatorCellFinder
	{
		private const float MaxDistanceToSpectateRect = 14.5f;

		private static float[] scorePerSide = new float[4];

		private static List<IntVec3> usedCells = new List<IntVec3>();

		public static bool TryFindSpectatorCellFor(Pawn p, CellRect spectateRect, Map map, out IntVec3 cell, SpectateRectSide allowedSides = SpectateRectSide.All, int margin = 1, List<IntVec3> extraDisallowedCells = null, Precept_Ritual ritual = null, Func<IntVec3, Pawn, Map, bool> extraValidator = null)
		{
			spectateRect.ClipInsideMap(map);
			if (spectateRect.Area == 0 || allowedSides == SpectateRectSide.None)
			{
				cell = IntVec3.Invalid;
				return false;
			}
			if (ritual != null && ritual.ideo.RitualSeatDef != null)
			{
				foreach (Thing item in from t in p.Map.listerThings.ThingsOfDef(ritual.ideo.RitualSeatDef)
					orderby t.Position.DistanceTo(spectateRect.CenterCell)
					select t)
				{
					if (!GatheringsUtility.InGatheringArea(item.Position, spectateRect.CenterCell, p.Map) || !IsCorrectlyRotatedChair(item.Position, item.Rotation, item.def, spectateRect))
					{
						continue;
					}
					foreach (IntVec3 item2 in item.OccupiedRect())
					{
						IntVec3 intVec = spectateRect.ClosestCellTo(item2);
						if (!((float)intVec.DistanceToSquared(item2) > 210.25f) && (!(p.mindState.duty?.focusThird.Thing is Building building) || !(item2 == building.InteractionCell)) && p.CanReach(item2, PathEndMode.OnCell, Danger.Deadly) && p.CanReserveSittableOrSpot(item2) && GenSight.LineOfSight(intVec, item2, map, skipFirstCell: true))
						{
							cell = item2;
							return true;
						}
					}
				}
			}
			CellRect rectWithMargin = spectateRect.ExpandedBy(margin).ClipInsideMap(map);
			Predicate<IntVec3> predicate = delegate(IntVec3 x)
			{
				if (!x.InBounds(map))
				{
					return false;
				}
				if (!x.Standable(map))
				{
					return false;
				}
				if (x.Fogged(map))
				{
					return false;
				}
				if (rectWithMargin.Contains(x))
				{
					return false;
				}
				if ((x.z <= rectWithMargin.maxZ || (allowedSides & SpectateRectSide.Up) != SpectateRectSide.Up) && (x.x <= rectWithMargin.maxX || (allowedSides & SpectateRectSide.Right) != SpectateRectSide.Right) && (x.z >= rectWithMargin.minZ || (allowedSides & SpectateRectSide.Down) != SpectateRectSide.Down) && (x.x >= rectWithMargin.minX || (allowedSides & SpectateRectSide.Left) != SpectateRectSide.Left))
				{
					return false;
				}
				IntVec3 intVec5 = spectateRect.ClosestCellTo(x);
				if ((float)intVec5.DistanceToSquared(x) > 210.25f)
				{
					return false;
				}
				if (!GenSight.LineOfSight(intVec5, x, map, skipFirstCell: true))
				{
					return false;
				}
				if (x.GetThingList(map).Find((Thing y) => y is Pawn && y != p) != null)
				{
					return false;
				}
				if (p != null && !CellAccessibleForPawn(p, x, map))
				{
					return false;
				}
				if (extraDisallowedCells != null && extraDisallowedCells.Contains(x))
				{
					return false;
				}
				if (!CorrectlyRotatedChairAt(x, map, spectateRect) && !GoodCellToSpectateStanding(x, map, spectateRect))
				{
					return false;
				}
				Thing thing = p?.mindState?.duty?.focusThird.Thing;
				if (thing is Building building2 && thing.Spawned && x == building2.InteractionCell)
				{
					return false;
				}
				return (extraValidator == null || extraValidator(x, p, map)) ? true : false;
			};
			if (p != null && predicate(p.Position) && CorrectlyRotatedChairAt(p.Position, map, spectateRect))
			{
				cell = p.Position;
				return true;
			}
			IntVec2 intVec2 = new IntVec2((rectWithMargin.Width % 2 == 0) ? (-1) : 0, (rectWithMargin.Height % 2 == 0) ? (-1) : 0);
			for (int num = 0; num < 1000; num++)
			{
				IntVec3 intVec3 = rectWithMargin.CenterCell + GenRadial.RadialPattern[num];
				if (intVec3.x > rectWithMargin.CenterCell.x)
				{
					intVec3.x += intVec2.x;
				}
				if (intVec3.z > rectWithMargin.CenterCell.z)
				{
					intVec3.z += intVec2.z;
				}
				if (!predicate(intVec3))
				{
					continue;
				}
				if (!CorrectlyRotatedChairAt(intVec3, map, spectateRect))
				{
					for (int num2 = 0; num2 < 90; num2++)
					{
						IntVec3 intVec4 = intVec3 + GenRadial.RadialPattern[num2];
						if (CorrectlyRotatedChairAt(intVec4, map, spectateRect) && predicate(intVec4))
						{
							cell = intVec4;
							return true;
						}
					}
				}
				cell = intVec3;
				return true;
			}
			cell = IntVec3.Invalid;
			return false;
		}

		public static bool CorrectlyRotatedChairAt(IntVec3 x, Map map, CellRect spectateRect)
		{
			return GetCorrectlyRotatedChairAt(x, map, spectateRect) != null;
		}

		public static Building GetCorrectlyRotatedChairAt(IntVec3 x, Map map, CellRect spectateRect)
		{
			if (!x.InBounds(map))
			{
				return null;
			}
			Building edifice = x.GetEdifice(map);
			if (edifice == null || edifice.def.category != ThingCategory.Building || !edifice.def.building.isSittable)
			{
				return null;
			}
			if (IsCorrectlyRotatedChair(edifice.Position, edifice.Rotation, edifice.def, spectateRect))
			{
				return edifice;
			}
			return null;
		}

		public static bool IsCorrectlyRotatedChair(IntVec3 chairPos, Rot4 chairRot, ThingDef chairDef, CellRect spectateRect)
		{
			if (chairDef.building.sitIgnoreOrientation)
			{
				return true;
			}
			if (GenGeo.AngleDifferenceBetween(chairRot.AsAngle, (spectateRect.ClosestCellTo(chairPos) - chairPos).AngleFlat) > 75f)
			{
				return false;
			}
			return true;
		}

		private static int DistanceToClosestChair(IntVec3 from, IntVec3 step, Map map, int maxDist, CellRect spectateRect)
		{
			int num = 0;
			IntVec3 intVec = from;
			do
			{
				intVec += step;
				num++;
				if (!intVec.InBounds(map))
				{
					return -1;
				}
				if (CorrectlyRotatedChairAt(intVec, map, spectateRect))
				{
					return num;
				}
				if (!intVec.Walkable(map))
				{
					return -1;
				}
			}
			while (num < maxDist);
			return -1;
		}

		public static void DebugFlashPotentialSpectatorCells(CellRect spectateRect, Map map, SpectateRectSide allowedSides = SpectateRectSide.All, int margin = 1)
		{
			List<IntVec3> list = new List<IntVec3>();
			int num = 50;
			for (int i = 0; i < num; i++)
			{
				if (!TryFindSpectatorCellFor(null, spectateRect, map, out var cell, allowedSides, margin, list))
				{
					break;
				}
				list.Add(cell);
				float a = Mathf.Lerp(1f, 0.08f, (float)i / (float)num);
				Material mat = SolidColorMaterials.SimpleSolidColorMaterial(new Color(0f, 0.8f, 0f, a));
				map.debugDrawer.FlashCell(cell, mat, (i + 1).ToString());
			}
			SpectateRectSide spectateRectSide = FindSingleBestSide(spectateRect, map, allowedSides, margin);
			IntVec3 centerCell = spectateRect.CenterCell;
			switch (spectateRectSide)
			{
			case SpectateRectSide.Up:
				centerCell.z += spectateRect.Height / 2 + 10;
				break;
			case SpectateRectSide.Right:
				centerCell.x += spectateRect.Width / 2 + 10;
				break;
			case SpectateRectSide.Down:
				centerCell.z -= spectateRect.Height / 2 + 10;
				break;
			case SpectateRectSide.Left:
				centerCell.x -= spectateRect.Width / 2 + 10;
				break;
			}
			map.debugDrawer.FlashLine(spectateRect.CenterCell, centerCell);
		}

		private static bool GoodCellToSpectateStanding(IntVec3 cell, Map map, CellRect spectateRect)
		{
			if (CommonRitualCellPredicates.InDoor(map, cell) || CommonRitualCellPredicates.OnBed(map, cell))
			{
				return false;
			}
			int num = 0;
			for (int i = 0; i < GenAdj.AdjacentCells.Length; i++)
			{
				if (CorrectlyRotatedChairAt(cell + GenAdj.AdjacentCells[i], map, spectateRect))
				{
					num++;
				}
			}
			if (num >= 3)
			{
				return false;
			}
			int num2 = DistanceToClosestChair(cell, new IntVec3(-1, 0, 0), map, 4, spectateRect);
			if (num2 >= 0)
			{
				int num3 = DistanceToClosestChair(cell, new IntVec3(1, 0, 0), map, 4, spectateRect);
				if (num3 >= 0 && Mathf.Abs(num2 - num3) <= 1)
				{
					return false;
				}
			}
			int num4 = DistanceToClosestChair(cell, new IntVec3(0, 0, 1), map, 4, spectateRect);
			if (num4 >= 0)
			{
				int num5 = DistanceToClosestChair(cell, new IntVec3(0, 0, -1), map, 4, spectateRect);
				if (num5 >= 0 && Mathf.Abs(num4 - num5) <= 1)
				{
					return false;
				}
			}
			return true;
		}

		private static bool CellAccessibleForPawn(Pawn p, IntVec3 cell, Map map)
		{
			if (!p.CanReserveSittableOrSpot(cell) || !p.CanReach(cell, PathEndMode.OnCell, Danger.Some))
			{
				return false;
			}
			if (cell.IsForbidden(p))
			{
				return false;
			}
			if (cell.GetDangerFor(p, map) != Danger.None)
			{
				return false;
			}
			return true;
		}

		public static SpectateRectSide FindSingleBestSide(CellRect spectateRect, Map map, SpectateRectSide allowedSides = SpectateRectSide.All, int margin = 1, Func<IntVec3, SpectateRectSide, int, float> scoreOffset = null)
		{
			for (int i = 0; i < scorePerSide.Length; i++)
			{
				scorePerSide[i] = 0f;
			}
			usedCells.Clear();
			int num = 30;
			CellRect cellRect = spectateRect.ExpandedBy(margin).ClipInsideMap(map);
			for (int j = 0; j < num; j++)
			{
				if (!TryFindSpectatorCellFor(null, spectateRect, map, out var cell, allowedSides, margin, usedCells))
				{
					break;
				}
				usedCells.Add(cell);
				SpectateRectSide spectateRectSide = SpectateRectSide.None;
				if (cell.z > cellRect.maxZ)
				{
					spectateRectSide |= SpectateRectSide.Up;
				}
				if (cell.x > cellRect.maxX)
				{
					spectateRectSide |= SpectateRectSide.Right;
				}
				if (cell.z < cellRect.minZ)
				{
					spectateRectSide |= SpectateRectSide.Down;
				}
				if (cell.x < cellRect.minX)
				{
					spectateRectSide |= SpectateRectSide.Left;
				}
				float num2 = Mathf.Lerp(1f, 0.35f, (float)j / (float)num);
				float num3 = num2 + (scoreOffset?.Invoke(cell, spectateRectSide, j) ?? 0f);
				Building correctlyRotatedChairAt = GetCorrectlyRotatedChairAt(cell, map, spectateRect);
				foreach (SpectateRectSide allSelectedItem in spectateRectSide.GetAllSelectedItems<SpectateRectSide>())
				{
					if (allSelectedItem.ValidSingleSide() && allowedSides.HasFlag(allSelectedItem))
					{
						scorePerSide[allSelectedItem.AsInt()] += num3;
						if (correctlyRotatedChairAt != null && correctlyRotatedChairAt.Rotation == allSelectedItem.AsRot4())
						{
							scorePerSide[allSelectedItem.AsInt()] += 1.2f * num2;
						}
					}
				}
			}
			float num4 = 0f;
			int num5 = -1;
			for (int k = 0; k < scorePerSide.Length; k++)
			{
				if (scorePerSide[k] != 0f && (num5 < 0 || scorePerSide[k] > num4))
				{
					num5 = k;
					num4 = scorePerSide[k];
				}
			}
			usedCells.Clear();
			return num5.ToSpectatorSide();
		}

		public static bool ValidSingleSide(this SpectateRectSide side)
		{
			return side switch
			{
				SpectateRectSide.Up => true, 
				SpectateRectSide.Right => true, 
				SpectateRectSide.Down => true, 
				SpectateRectSide.Left => true, 
				_ => false, 
			};
		}

		public static Rot4 AsRot4(this SpectateRectSide side)
		{
			return side switch
			{
				SpectateRectSide.Up => Rot4.North, 
				SpectateRectSide.Right => Rot4.East, 
				SpectateRectSide.Down => Rot4.South, 
				SpectateRectSide.Left => Rot4.West, 
				_ => Rot4.Invalid, 
			};
		}

		public static int AsInt(this SpectateRectSide side)
		{
			return side switch
			{
				SpectateRectSide.Up => 0, 
				SpectateRectSide.Right => 1, 
				SpectateRectSide.Down => 2, 
				SpectateRectSide.Left => 3, 
				_ => 0, 
			};
		}

		public static SpectateRectSide ToSpectatorSide(this int side)
		{
			return side switch
			{
				0 => SpectateRectSide.Up, 
				1 => SpectateRectSide.Right, 
				2 => SpectateRectSide.Down, 
				3 => SpectateRectSide.Left, 
				_ => SpectateRectSide.None, 
			};
		}

		public static SpectateRectSide Opposite(this SpectateRectSide side)
		{
			return side switch
			{
				SpectateRectSide.Up => SpectateRectSide.Down, 
				SpectateRectSide.Down => SpectateRectSide.Up, 
				SpectateRectSide.Right => SpectateRectSide.Left, 
				SpectateRectSide.Left => SpectateRectSide.Right, 
				SpectateRectSide.Horizontal => SpectateRectSide.Vertical, 
				SpectateRectSide.Vertical => SpectateRectSide.Horizontal, 
				SpectateRectSide.All => SpectateRectSide.None, 
				SpectateRectSide.None => SpectateRectSide.All, 
				_ => side, 
			};
		}

		public static SpectateRectSide NextRotationClockwise(this SpectateRectSide side)
		{
			return side switch
			{
				SpectateRectSide.Up => SpectateRectSide.Right, 
				SpectateRectSide.Right => SpectateRectSide.Down, 
				SpectateRectSide.Down => SpectateRectSide.Left, 
				SpectateRectSide.Left => SpectateRectSide.Up, 
				SpectateRectSide.Horizontal => SpectateRectSide.Vertical, 
				SpectateRectSide.Vertical => SpectateRectSide.Horizontal, 
				_ => side, 
			};
		}

		public static SpectateRectSide Rotated(this SpectateRectSide side, Rot4 rot)
		{
			SpectateRectSide spectateRectSide = side;
			if (rot.IsValid && rot != Rot4.North)
			{
				for (int i = 0; i < rot.AsInt; i++)
				{
					spectateRectSide = spectateRectSide.NextRotationClockwise();
				}
			}
			return spectateRectSide;
		}

		public static Vector3 GraphicOffsetForRect(this SpectateRectSide side, IntVec3 center, CellRect rect, Rot4 rotation, Vector2 additionalOffset)
		{
			SpectateRectSide spectateRectSide = side.Rotated(rotation);
			Vector2 asVector = spectateRectSide.AsRot4().AsVector2;
			Vector2 vector = asVector;
			vector.Scale(new Vector3((float)rect.Width / 2f, (float)rect.Height / 2f));
			Vector2 vector2 = new Vector2((rect.Width % 2 == 0) ? 0.5f : 0f, (rect.Height % 2 == 0) ? 0.5f : 0f);
			if (rotation.AsInt > 1)
			{
				vector2 = -vector2;
			}
			if (rect.Height % 2 == 0 && rotation.IsHorizontal)
			{
				vector2.y = 0f - vector2.y;
			}
			vector2 += (((spectateRectSide & SpectateRectSide.Horizontal) != SpectateRectSide.Horizontal) ? asVector : (-asVector)) * additionalOffset;
			vector2 += new Vector2(((rect.Width % 2 != 0) ? 0.75f : 0.5f) * asVector.x, ((rect.Height % 2 != 0) ? 0.75f : 0.5f) * asVector.y);
			return center.ToVector3Shifted() + new Vector3(vector.x, 0f, vector.y) + new Vector3(vector2.x, 0f, vector2.y);
		}

		public static bool TryFindCircleSpectatorCellFor(Pawn p, CellRect spectateRect, float minDistance, float maxDistance, Map map, out IntVec3 cell, List<IntVec3> extraDisallowedCells = null, Func<IntVec3, Pawn, Map, bool> extraValidator = null)
		{
			float minDistanceSquared = minDistance * minDistance;
			float maxDistanceSquared = maxDistance * maxDistance;
			Predicate<IntVec3> predicate = delegate(IntVec3 x)
			{
				if (!x.InBounds(map))
				{
					return false;
				}
				if (spectateRect.Contains(x))
				{
					return false;
				}
				if (!x.Standable(map))
				{
					return false;
				}
				if (x.Fogged(map))
				{
					return false;
				}
				IntVec3 intVec3 = spectateRect.ClosestCellTo(x);
				if (!WanderUtility.InSameRoom(x, intVec3, map))
				{
					return false;
				}
				if ((float)intVec3.DistanceToSquared(x) > maxDistanceSquared)
				{
					return false;
				}
				if ((float)intVec3.DistanceToSquared(x) < minDistanceSquared)
				{
					return false;
				}
				if (!GenSight.LineOfSight(intVec3, x, map, skipFirstCell: true))
				{
					return false;
				}
				if (x.GetThingList(map).Find((Thing y) => y is Pawn && y != p) != null)
				{
					return false;
				}
				if (p != null && !CellAccessibleForPawn(p, x, map))
				{
					return false;
				}
				if (extraDisallowedCells != null && extraDisallowedCells.Contains(x))
				{
					return false;
				}
				if (!CorrectlyRotatedChairAt(x, map, spectateRect) && !GoodCellToSpectateStanding(x, map, spectateRect))
				{
					return false;
				}
				return (extraValidator == null || extraValidator(x, p, map)) ? true : false;
			};
			if (p != null && predicate(p.Position) && CorrectlyRotatedChairAt(p.Position, map, spectateRect))
			{
				cell = p.Position;
				return true;
			}
			for (int num = 0; num < GenRadial.NumCellsInRadius(maxDistance); num++)
			{
				IntVec3 intVec = spectateRect.CenterCell + GenRadial.RadialPattern[num];
				if (!predicate(intVec))
				{
					continue;
				}
				if (!CorrectlyRotatedChairAt(intVec, map, spectateRect))
				{
					for (int num2 = 0; num2 < 90; num2++)
					{
						IntVec3 intVec2 = intVec + GenRadial.RadialPattern[num2];
						if (CorrectlyRotatedChairAt(intVec2, map, spectateRect) && predicate(intVec2))
						{
							cell = intVec2;
							return true;
						}
					}
				}
				cell = intVec;
				return true;
			}
			cell = IntVec3.Invalid;
			return false;
		}
	}
}
