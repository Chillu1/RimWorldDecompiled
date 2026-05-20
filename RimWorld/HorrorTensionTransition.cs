using Verse;
using Verse.AI.Group;

namespace RimWorld;

public class HorrorTensionTransition : MusicTransition
{
	public override bool IsTransitionSatisfied()
	{
		if (!ModsConfig.AnomalyActive)
		{
			return false;
		}
		if (!base.IsTransitionSatisfied())
		{
			return false;
		}
		foreach (Map map in Find.Maps)
		{
			if (map.gameConditionManager.ConditionIsActive(GameConditionDefOf.UnnaturalDarkness) || map.gameConditionManager.ConditionIsActive(GameConditionDefOf.DeathPall) || map.gameConditionManager.ConditionIsActive(GameConditionDefOf.BloodRain))
			{
				return true;
			}
		}
		foreach (Map map2 in Find.Maps)
		{
			if (map2.listerThings.ThingsInGroup(ThingRequestGroup.BuildingGroundSpawner).Any())
			{
				return true;
			}
			foreach (Pawn item in map2.mapPawns.AllPawnsSpawned)
			{
				if (IsValidPawn(item))
				{
					return true;
				}
			}
		}
		return false;
	}

	private static bool IsValidPawn(Pawn pawn)
	{
		Lord lord = pawn.GetLord();
		if (lord != null)
		{
			if (!(lord.LordJob is LordJob_PsychicRitual))
			{
				return lord.LordJob is LordJob_HateChant;
			}
			return true;
		}
		return false;
	}
}
