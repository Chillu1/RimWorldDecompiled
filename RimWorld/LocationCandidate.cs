using Verse;

namespace RimWorld;

public struct LocationCandidate
{
	public IntVec3 cell;

	public float score;

	public LocationCandidate(IntVec3 cell, float score)
	{
		this.cell = cell;
		this.score = score;
	}
}
