using RimWorld;

namespace Verse.AI.Group;

public class Trigger_PawnExperiencingAnomalousWeather : Trigger
{
	private float weatherHediffThreshold = 0.15f;

	public override bool ActivateOn(Lord lord, TriggerSignal signal)
	{
		if (!ModsConfig.AnomalyActive)
		{
			return false;
		}
		if (signal.type == TriggerSignalType.Tick && Find.TickManager.TicksGame % 197 == 0)
		{
			for (int i = 0; i < lord.ownedPawns.Count; i++)
			{
				Pawn pawn = lord.ownedPawns[i];
				if (pawn.Spawned && !pawn.Dead && !pawn.Downed)
				{
					Hediff firstHediffOfDef = pawn.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.BloodRage);
					if (firstHediffOfDef != null && firstHediffOfDef.Severity > weatherHediffThreshold)
					{
						return true;
					}
				}
			}
		}
		return false;
	}
}
