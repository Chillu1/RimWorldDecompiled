using Verse;
using Verse.AI.Group;

namespace RimWorld;

public class HorrorCombatTransition : MusicTransition
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
			if (!IsValidMap(map))
			{
				continue;
			}
			foreach (Pawn item in map.mapPawns.AllPawnsSpawned)
			{
				if (IsValidPawn(item))
				{
					return true;
				}
				if (IsValidEntity(item))
				{
					return true;
				}
			}
		}
		return false;
	}

	private bool IsValidMap(Map map)
	{
		return map.mapPawns.ColonistsSpawnedCount > 0;
	}

	private bool IsValidPawn(Pawn pawn)
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

	private bool IsValidEntity(Pawn pawn)
	{
		if ((pawn.RaceProps.IsAnomalyEntity || pawn.IsSubhuman) && !pawn.Downed && !pawn.IsOnHoldingPlatform && !pawn.IsPsychologicallyInvisible() && !pawn.Fogged())
		{
			return pawn.HostileTo(Faction.OfPlayer);
		}
		return false;
	}
}
