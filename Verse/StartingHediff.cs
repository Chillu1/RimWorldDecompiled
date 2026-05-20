namespace Verse;

public class StartingHediff
{
	public HediffDef def;

	public float? severity;

	public float? chance;

	public IntRange? durationTicksRange;

	public bool HasHediff(Pawn p)
	{
		return p.health.hediffSet.HasHediff(def);
	}
}
