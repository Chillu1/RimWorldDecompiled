using Verse;

namespace RimWorld;

internal class FleshTendril : IExposable
{
	public IntVec3 sourceNode;

	public IntVec3 currentPos;

	public int length;

	public void ExposeData()
	{
		Scribe_Values.Look(ref sourceNode, "sourceNode");
		Scribe_Values.Look(ref currentPos, "currentPos");
		Scribe_Values.Look(ref length, "length", 0);
	}
}
