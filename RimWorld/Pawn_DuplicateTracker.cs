using Verse;

namespace RimWorld;

public class Pawn_DuplicateTracker : IExposable
{
	private Pawn pawn;

	public int duplicateOf = int.MinValue;

	public Pawn_DuplicateTracker()
	{
	}

	public Pawn_DuplicateTracker(Pawn pawn)
	{
		this.pawn = pawn;
	}

	public void Notify_PawnKilled()
	{
		Find.PawnDuplicator.RemoveDuplicate(duplicateOf, pawn);
	}

	public void ExposeData()
	{
		Scribe_Values.Look(ref duplicateOf, "duplicateOf", int.MinValue);
	}
}
