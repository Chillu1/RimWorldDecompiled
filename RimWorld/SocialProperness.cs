using Verse;

namespace RimWorld
{
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
			IntVec3 intVec = t.def.hasInteractionCell ? t.InteractionCell : t.Position;
			if (forPrisoner)
			{
				if (p != null)
				{
					return intVec.GetRoom(t.Map) == p.GetRoom();
				}
				return true;
			}
			return !intVec.IsInPrisonCell(t.Map);
		}
	}
}
