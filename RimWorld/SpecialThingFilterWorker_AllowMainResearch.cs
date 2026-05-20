namespace RimWorld;

public class SpecialThingFilterWorker_AllowMainResearch : SpecialThingFilterWorker_AllowResearch
{
	public SpecialThingFilterWorker_AllowMainResearch()
		: base(ResearchTabDefOf.Main)
	{
	}
}
