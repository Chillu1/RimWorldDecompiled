using Verse;

namespace RimWorld;

public class AnalysisDetails : IExposable
{
	public int id;

	public int required;

	public int timesDone;

	public bool Satisfied => timesDone >= required;

	public void ExposeData()
	{
		Scribe_Values.Look(ref id, "id", 0);
		Scribe_Values.Look(ref required, "required", 0);
		Scribe_Values.Look(ref timesDone, "timesDone", 0);
	}
}
