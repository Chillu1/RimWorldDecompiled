using System.Collections.Generic;
using LudeonTK;

namespace Verse;

public static class ShootLeanUtility
{
	private static readonly List<IntVec3> tempSourceList = new List<IntVec3>();

	private static readonly List<IntVec3> tempDestList = new List<IntVec3>();

	public static void LeanShootingSourcesFromTo(IntVec3 shooterLoc, IntVec3 targetPos, Map map, List<IntVec3> listToFill)
	{
		listToFill.Clear();
		float angleFlat = (targetPos - shooterLoc).AngleFlat;
		bool flag = angleFlat > 270f || angleFlat < 90f;
		bool flag2 = angleFlat > 90f && angleFlat < 270f;
		bool flag3 = angleFlat > 180f;
		bool flag4 = angleFlat < 180f;
		ByteBits byteBits = default(ByteBits);
		for (int i = 0; i < 8; i++)
		{
			byteBits[i] = !(shooterLoc + GenAdj.AdjacentCells[i]).CanBeSeenOver(map);
		}
		if (!byteBits[1] && ((byteBits[0] && !byteBits[5] && flag) || (byteBits[2] && !byteBits[4] && flag2)))
		{
			listToFill.Add(shooterLoc + IntVec3.East);
		}
		if (!byteBits[3] && ((byteBits[0] && !byteBits[6] && flag) || (byteBits[2] && !byteBits[7] && flag2)))
		{
			listToFill.Add(shooterLoc + IntVec3.West);
		}
		if (!byteBits[2] && ((byteBits[3] && !byteBits[7] && flag3) || (byteBits[1] && !byteBits[4] && flag4)))
		{
			listToFill.Add(shooterLoc + IntVec3.South);
		}
		if (!byteBits[0] && ((byteBits[3] && !byteBits[6] && flag3) || (byteBits[1] && !byteBits[5] && flag4)))
		{
			listToFill.Add(shooterLoc + IntVec3.North);
		}
		if (shooterLoc.CanBeSeenOver(map))
		{
			listToFill.Add(shooterLoc);
		}
		for (int j = 0; j < 4; j++)
		{
			if (byteBits[j])
			{
				continue;
			}
			switch (j)
			{
			case 0:
				if (!flag)
				{
					continue;
				}
				break;
			case 1:
				if (!flag4)
				{
					continue;
				}
				break;
			case 2:
				if (!flag2)
				{
					continue;
				}
				break;
			case 3:
				if (!flag3)
				{
					continue;
				}
				break;
			}
			if ((shooterLoc + GenAdj.AdjacentCells[j]).GetCover(map) != null)
			{
				listToFill.Add(shooterLoc + GenAdj.AdjacentCells[j]);
			}
		}
	}

	public static void CalcShootableCellsOf(List<IntVec3> outCells, Thing target, IntVec3 shooterPos)
	{
		outCells.Clear();
		if (target is Pawn)
		{
			LeanShootingSourcesFromTo(target.Position, shooterPos, target.Map, outCells);
			return;
		}
		outCells.Add(target.Position);
		if (target.def.size.x == 1 && target.def.size.z == 1)
		{
			return;
		}
		foreach (IntVec3 item in target.OccupiedRect())
		{
			if (item != target.Position)
			{
				outCells.Add(item);
			}
		}
	}

	public static bool CellCanSeeCell(IntVec3 source, IntVec3 dest, Map map)
	{
		try
		{
			if (!source.InBounds(map) || !dest.InBounds(map))
			{
				return false;
			}
			if (!source.CanBeSeenOver(map) || !dest.CanBeSeenOver(map))
			{
				return false;
			}
			LeanShootingSourcesFromTo(dest, source, map, tempDestList);
			for (int i = 0; i < tempDestList.Count; i++)
			{
				if (GenSight.LineOfSight(source, dest, map, skipFirstCell: true))
				{
					return true;
				}
			}
			LeanShootingSourcesFromTo(source, dest, map, tempSourceList);
			for (int j = 0; j < tempSourceList.Count; j++)
			{
				for (int k = 0; k < tempDestList.Count; k++)
				{
					if (GenSight.LineOfSight(tempSourceList[j], tempDestList[k], map, skipFirstCell: true))
					{
						return true;
					}
				}
			}
			return false;
		}
		finally
		{
			tempSourceList.Clear();
			tempDestList.Clear();
		}
	}
}
