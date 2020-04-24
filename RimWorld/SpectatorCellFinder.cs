using System;
using System.Collections.Generic;
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

		public static bool TryFindSpectatorCellFor(Pawn p, CellRect spectateRect, Map map, out IntVec3 cell, SpectateRectSide allowedSides = SpectateRectSide.All, int margin = 1, List<IntVec3> extraDisallowedCells = null)
		{
			spectateRect.ClipInsideMap(map);
			if (spectateRect.Area == 0 || allowedSides == SpectateRectSide.None)
			{
				cell = IntVec3.Invalid;
				return false;
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
				IntVec3 intVec3 = spectateRect.ClosestCellTo(x);
				if ((float)intVec3.DistanceToSquared(x) > 210.25f)
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
				if (p != null)
				{
					if (!p.CanReserveAndReach(x, PathEndMode.OnCell, Danger.Some))
					{
						return false;
					}
					Building edifice = x.GetEdifice(map);
					if (edifice != null && edifice.def.category == ThingCategory.Building && edifice.def.building.isSittable && !p.CanReserve(edifice))
					{
						return false;
					}
					if (x.IsForbidden(p))
					{
						return false;
					}
					if (x.GetDangerFor(p, map) != Danger.None)
					{
						return false;
					}
				}
				if (extraDisallowedCells != null && extraDisallowedCells.Contains(x))
				{
					return false;
				}
				if (!CorrectlyRotatedChairAt(x, map, spectateRect))
				{
					int num = 0;
					for (int k = 0; k < GenAdj.AdjacentCells.Length; k++)
					{
						if (CorrectlyRotatedChairAt(x + GenAdj.AdjacentCells[k], map, spectateRect))
						{
							num++;
						}
					}
					if (num >= 3)
					{
						return false;
					}
					int num2 = DistanceToClosestChair(x, new IntVec3(-1, 0, 0), map, 4, spectateRect);
					if (num2 >= 0)
					{
						int num3 = DistanceToClosestChair(x, new IntVec3(1, 0, 0), map, 4, spectateRect);
						if (num3 >= 0 && Mathf.Abs(num2 - num3) <= 1)
						{
							return false;
						}
					}
					int num4 = DistanceToClosestChair(x, new IntVec3(0, 0, 1), map, 4, spectateRect);
					if (num4 >= 0)
					{
						int num5 = DistanceToClosestChair(x, new IntVec3(0, 0, -1), map, 4, spectateRect);
						if (num5 >= 0 && Mathf.Abs(num4 - num5) <= 1)
						{
							return false;
						}
					}
				}
				return true;
			};
			if (p != null && predicate(p.Position) && CorrectlyRotatedChairAt(p.Position, map, spectateRect))
			{
				cell = p.Position;
				return true;
			}
			for (int i = 0; i < 1000; i++)
			{
				IntVec3 intVec = rectWithMargin.CenterCell + GenRadial.RadialPattern[i];
				if (!predicate(intVec))
				{
					continue;
				}
				if (!CorrectlyRotatedChairAt(intVec, map, spectateRect))
				{
					for (int j = 0; j < 90; j++)
					{
						IntVec3 intVec2 = intVec + GenRadial.RadialPattern[j];
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

		private static bool CorrectlyRotatedChairAt(IntVec3 x, Map map, CellRect spectateRect)
		{
			return GetCorrectlyRotatedChairAt(x, map, spectateRect) != null;
		}

		private static Building GetCorrectlyRotatedChairAt(IntVec3 x, Map map, CellRect spectateRect)
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
			if (GenGeo.AngleDifferenceBetween(edifice.Rotation.AsAngle, (spectateRect.ClosestCellTo(x) - edifice.Position).AngleFlat) > 75f)
			{
				return null;
			}
			return edifice;
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
				if (!TryFindSpectatorCellFor(null, spectateRect, map, out IntVec3 cell, allowedSides, margin, list))
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

		public static SpectateRectSide FindSingleBestSide(CellRect spectateRect, Map map, SpectateRectSide allowedSides = SpectateRectSide.All, int margin = 1)
		{
			return FindSingleBestSide_NewTemp(spectateRect, map, allowedSides, margin);
		}

		public static SpectateRectSide FindSingleBestSide_NewTemp(CellRect spectateRect, Map map, SpectateRectSide allowedSides = SpectateRectSide.All, int margin = 1, Func<IntVec3, SpectateRectSide, int, float> scoreOffset = null)
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
				if (!TryFindSpectatorCellFor(null, spectateRect, map, out IntVec3 cell, allowedSides, margin, usedCells))
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
					if (allSelectedItem > SpectateRectSide.None && allSelectedItem < SpectateRectSide.Vertical && allowedSides.HasFlag(allSelectedItem))
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

		public static Rot4 AsRot4(this SpectateRectSide side)
		{
			switch (side)
			{
			case SpectateRectSide.Up:
				return Rot4.North;
			case SpectateRectSide.Right:
				return Rot4.East;
			case SpectateRectSide.Down:
				return Rot4.South;
			case SpectateRectSide.Left:
				return Rot4.West;
			default:
				return Rot4.Invalid;
			}
		}

		public static int AsInt(this SpectateRectSide side)
		{
			switch (side)
			{
			case SpectateRectSide.Up:
				return 0;
			case SpectateRectSide.Right:
				return 1;
			case SpectateRectSide.Down:
				return 2;
			case SpectateRectSide.Left:
				return 3;
			default:
				return 0;
			}
		}

		public static SpectateRectSide ToSpectatorSide(this int side)
		{
			switch (side)
			{
			case 0:
				return SpectateRectSide.Up;
			case 1:
				return SpectateRectSide.Right;
			case 2:
				return SpectateRectSide.Down;
			case 3:
				return SpectateRectSide.Left;
			default:
				return SpectateRectSide.None;
			}
		}
	}
}
