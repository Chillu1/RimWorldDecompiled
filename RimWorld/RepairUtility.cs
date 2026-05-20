using Verse;

namespace RimWorld;

public static class RepairUtility
{
	public static bool PawnCanRepairEver(Pawn pawn, Thing t)
	{
		if (!(t is Building building))
		{
			return false;
		}
		if (!t.def.useHitPoints)
		{
			return false;
		}
		if (!building.def.building.repairable)
		{
			return false;
		}
		if (t.Faction != pawn.Faction)
		{
			return false;
		}
		return true;
	}

	public static bool PawnCanRepairNow(Pawn pawn, Thing t)
	{
		if (!PawnCanRepairEver(pawn, t))
		{
			return false;
		}
		if (!pawn.Map.listerBuildingsRepairable.Contains(pawn.Faction, (Building)t))
		{
			return false;
		}
		if (t.HitPoints == t.MaxHitPoints)
		{
			return false;
		}
		return true;
	}
}
