namespace Verse;

public class PawnRitualReference : IExposable
{
	public Pawn pawn;

	public PawnRitualReference()
	{
	}

	public PawnRitualReference(Pawn p)
	{
		pawn = p;
	}

	public void ExposeData()
	{
		Scribe_References.Look(ref pawn, "pawn");
	}
}
