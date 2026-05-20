using Verse;

namespace RimWorld;

public abstract class SpecialThingFilterWorker_AllowResearch : SpecialThingFilterWorker
{
	private readonly ResearchTabDef tab;

	protected SpecialThingFilterWorker_AllowResearch(ResearchTabDef tab)
	{
		this.tab = tab;
	}

	public override bool Matches(Thing t)
	{
		if (t is Book book && book.BookComp.TryGetDoer<ReadingOutcomeDoerGainResearch>(out var doer))
		{
			return doer.Props.tab == tab;
		}
		return false;
	}

	public override bool CanEverMatch(ThingDef def)
	{
		return def.HasComp<CompBook>();
	}
}
