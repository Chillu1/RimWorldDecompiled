namespace Verse.AI.Group;

public class PsychicRitualParticipant : IExposable
{
	public Pawn pawn;

	public IntVec3 location;

	public PsychicRitualParticipant()
	{
	}

	public PsychicRitualParticipant((Pawn, IntVec3) pair)
	{
		(pawn, location) = pair;
	}

	public void Deconstruct(out Pawn pawn, out IntVec3 location)
	{
		pawn = this.pawn;
		location = this.location;
	}

	public void ExposeData()
	{
		Scribe_References.Look(ref pawn, "pawn");
		Scribe_Values.Look(ref location, "location");
	}
}
