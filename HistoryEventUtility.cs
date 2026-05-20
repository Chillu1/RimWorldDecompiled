using RimWorld;
using Verse;

public static class HistoryEventUtility
{
	public static bool IsKillingInnocentAnimal(Pawn executioner, Pawn victim)
	{
		if (!ModsConfig.IdeologyActive)
		{
			return false;
		}
		if (!victim.IsAnimal)
		{
			return false;
		}
		if (victim.Faction != null && executioner.Faction != null && victim.Faction.HostileTo(executioner.Faction))
		{
			return false;
		}
		if (victim.health.hediffSet.HasHediff(HediffDefOf.Scaria))
		{
			return false;
		}
		if (executioner.CurJob != null && executioner.CurJob.def == JobDefOf.PredatorHunt)
		{
			return false;
		}
		if (victim.CurJob != null && victim.CurJob.def == JobDefOf.PredatorHunt)
		{
			Pawn prey = ((JobDriver_PredatorHunt)victim.jobs.curDriver).Prey;
			if (prey != null)
			{
				if (prey.RaceProps.Humanlike)
				{
					return false;
				}
				if (prey.IsAnimal && prey.Faction != null && prey.Faction.def.humanlikeFaction)
				{
					return false;
				}
			}
		}
		if (!victim.InMentalState || victim.MentalState.causedByDamage || victim.MentalState.causedByPsycast)
		{
			return true;
		}
		return false;
	}
}
