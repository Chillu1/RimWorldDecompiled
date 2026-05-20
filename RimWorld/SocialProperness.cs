using Verse;

namespace RimWorld;

public static class SocialProperness
{
	public static bool IsSociallyProper(this Thing t, Pawn p)
	{
		return t.IsSociallyProper(p, p.IsPrisonerOfColony);
	}

	public static bool IsSociallyProper(this Thing t, Pawn p, bool forPrisoner, bool animalsCare = false)
	{
		if (!animalsCare && p != null && !p.RaceProps.Humanlike)
		{
			return true;
		}
		if (!t.def.socialPropernessMatters)
		{
			return true;
		}
		if (!t.Spawned)
		{
			return true;
		}
		IntVec3 intVec = (t.def.hasInteractionCell ? t.InteractionCell : t.Position);
		if (forPrisoner)
		{
			if (p != null)
			{
				return intVec.GetRoom(t.Map) == p.GetRoom();
			}
			return true;
		}
		if (ModsConfig.BiotechActive && t.def == ThingDefOf.HemogenPack)
		{
			return !BloodfeedingPrisonerInRoom(t.GetRoom());
		}
		return !intVec.IsInPrisonCell(t.Map);
	}

	public static bool BloodfeedingPrisonerInRoom(Room r)
	{
		if (r == null || !r.IsPrisonCell)
		{
			return false;
		}
		foreach (Pawn owner in r.Owners)
		{
			if (owner.IsBloodfeeder())
			{
				return true;
			}
		}
		return false;
	}
}
