using Verse;

namespace RimWorld
{
	public static class FeedPatientUtility
	{
		public static bool ShouldBeFed(Pawn p)
		{
			if (p.GetPosture() == PawnPosture.Standing)
			{
				return false;
			}
			if (p.NonHumanlikeOrWildMan())
			{
				Building_Bed building_Bed = p.CurrentBed();
				if (building_Bed == null || building_Bed.Faction != Faction.OfPlayer)
				{
					return false;
				}
			}
			else
			{
				if (p.Faction != Faction.OfPlayer && p.HostFaction != Faction.OfPlayer)
				{
					return false;
				}
				if (!p.InBed())
				{
					return false;
				}
			}
			if (!p.RaceProps.EatsFood)
			{
				return false;
			}
			if (p.Spawned && p.Map.designationManager.DesignationOn(p, DesignationDefOf.Slaughter) != null)
			{
				return false;
			}
			if (!HealthAIUtility.ShouldSeekMedicalRest(p))
			{
				return false;
			}
			if (p.HostFaction != null)
			{
				if (p.HostFaction != Faction.OfPlayer)
				{
					return false;
				}
				if (p.guest != null && !p.guest.CanBeBroughtFood)
				{
					return false;
				}
			}
			return true;
		}

		public static bool IsHungry(Pawn p)
		{
			if (p.needs != null && p.needs.food != null)
			{
				return p.needs.food.CurLevelPercentage <= p.needs.food.PercentageThreshHungry + 0.02f;
			}
			return false;
		}
	}
}
