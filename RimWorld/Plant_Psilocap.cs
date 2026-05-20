using Verse;

namespace RimWorld;

public class Plant_Psilocap : Plant
{
	private const float HediffSeverityIncreasePerSecond = 0.02f;

	private const float HediffSeverityIncreasePerTick = 0.00033333333f;

	private const float HediffSeverityMaxFromAmbient = 0.2f;

	protected override void TickInterval(int delta)
	{
		base.TickInterval(delta);
		if (base.Destroyed)
		{
			return;
		}
		for (int i = 0; i < base.Map.mapPawns.AllPawnsSpawned.Count; i++)
		{
			Pawn pawn = base.Map.mapPawns.AllPawnsSpawned[i];
			if (pawn.Position.DistanceTo(base.Position) > 2f)
			{
				continue;
			}
			RaceProperties raceProps = pawn.RaceProps;
			if ((raceProps.Humanlike || raceProps.Animal) && raceProps.IsFlesh)
			{
				Hediff orAddHediff = pawn.health.GetOrAddHediff(HediffDefOf.PsilocapHigh);
				if (!(orAddHediff.Severity > 0.2f))
				{
					orAddHediff.Severity += 0.00033333333f * (float)delta;
				}
			}
		}
	}
}
