using System.Collections.Generic;
using RimWorld;

namespace Verse;

public static class CoverUtility
{
	public const float CoverPercent_Corner = 0.75f;

	public static List<CoverInfo> CalculateCoverGiverSet(LocalTargetInfo target, IntVec3 shooterLoc, Map map)
	{
		IntVec3 cell = target.Cell;
		List<CoverInfo> list = new List<CoverInfo>();
		if (target.HasThing && !target.Thing.def.CanBenefitFromCover)
		{
			return list;
		}
		for (int i = 0; i < 8; i++)
		{
			IntVec3 intVec = cell + GenAdj.AdjacentCells[i];
			if (intVec.InBounds(map) && TryFindAdjustedCoverInCell(shooterLoc, target, intVec, map, out var result) && result.BlockChance > 0f)
			{
				list.Add(result);
			}
		}
		return list;
	}

	public static float CalculateOverallBlockChance(LocalTargetInfo target, IntVec3 shooterLoc, Map map)
	{
		IntVec3 cell = target.Cell;
		float num = 0f;
		if (target.HasThing && !target.Thing.def.CanBenefitFromCover)
		{
			return num;
		}
		for (int i = 0; i < 8; i++)
		{
			IntVec3 intVec = cell + GenAdj.AdjacentCells[i];
			if (intVec.InBounds(map) && !(shooterLoc == intVec) && TryFindAdjustedCoverInCell(shooterLoc, target, intVec, map, out var result))
			{
				num += (1f - num) * result.BlockChance;
			}
		}
		return num;
	}

	private static bool TryFindAdjustedCoverInCell(IntVec3 shooterLoc, LocalTargetInfo target, IntVec3 adjCell, Map map, out CoverInfo result)
	{
		IntVec3 cell = target.Cell;
		Thing cover = adjCell.GetCover(map);
		if (cover == null || cover == target.Thing || shooterLoc == cell)
		{
			result = CoverInfo.Invalid;
			return false;
		}
		float angleFlat = (shooterLoc - cell).AngleFlat;
		float num = GenGeo.AngleDifferenceBetween((adjCell - cell).AngleFlat, angleFlat);
		if (!cell.AdjacentToCardinal(adjCell))
		{
			num *= 1.75f;
		}
		float num2 = cover.BaseBlockChance();
		if (num < 15f)
		{
			num2 *= 1f;
		}
		else if (num < 27f)
		{
			num2 *= 0.8f;
		}
		else if (num < 40f)
		{
			num2 *= 0.6f;
		}
		else if (num < 52f)
		{
			num2 *= 0.4f;
		}
		else
		{
			if (!(num < 65f))
			{
				result = CoverInfo.Invalid;
				return false;
			}
			num2 *= 0.2f;
		}
		float lengthHorizontal = (shooterLoc - adjCell).LengthHorizontal;
		if (lengthHorizontal < 1.9f)
		{
			num2 *= 0.3333f;
		}
		else if (lengthHorizontal < 2.9f)
		{
			num2 *= 0.66666f;
		}
		result = new CoverInfo(cover, num2);
		return true;
	}

	public static float BaseBlockChance(this ThingDef def)
	{
		if (def.Fillage == FillCategory.Full)
		{
			return 0.75f;
		}
		return def.fillPercent;
	}

	public static float BaseBlockChance(this Thing thing)
	{
		if (thing is Building_Door { Open: not false })
		{
			return 0f;
		}
		return thing.def.BaseBlockChance();
	}

	public static float TotalSurroundingCoverScore(IntVec3 c, Map map)
	{
		float num = 0f;
		for (int i = 0; i < 8; i++)
		{
			IntVec3 c2 = c + GenAdj.AdjacentCells[i];
			if (c2.InBounds(map))
			{
				Thing cover = c2.GetCover(map);
				if (cover != null)
				{
					num += cover.BaseBlockChance();
				}
			}
		}
		return num;
	}

	public static bool ThingCovered(Thing thing, Map map)
	{
		foreach (IntVec3 item in thing.OccupiedRect())
		{
			List<Thing> thingList = item.GetThingList(map);
			bool flag = false;
			for (int i = 0; i < thingList.Count; i++)
			{
				if (thingList[i] != thing && thingList[i].def.Fillage == FillCategory.Full && thingList[i].def.Altitude >= thing.def.Altitude)
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				return false;
			}
		}
		return true;
	}
}
