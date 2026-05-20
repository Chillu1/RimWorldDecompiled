using Verse;

namespace RimWorld;

public static class AnomalyIncidentUtility
{
	private static readonly SimpleCurve CombatPointShardChanceCurve = new SimpleCurve
	{
		new CurvePoint(0f, 0.15f),
		new CurvePoint(500f, 0.3f),
		new CurvePoint(1000f, 0.5f),
		new CurvePoint(5000f, 0.8f)
	};

	public static bool IncidentShardChance(float combatPoints)
	{
		return Rand.Chance(CombatPointShardChanceCurve.Evaluate(combatPoints));
	}

	public static void PawnShardOnDeath(Pawn pawn)
	{
		pawn.health.AddHediff(HediffDefOf.ShardHolder);
	}
}
