using Verse;

namespace RimWorld;

public class SpecialThingFilterWorker_ChildOnlyApparel : SpecialThingFilterWorker
{
	public override bool AlwaysMatches(ThingDef def)
	{
		if (def.IsApparel)
		{
			return def.apparel.developmentalStageFilter == DevelopmentalStage.Child;
		}
		return false;
	}

	public override bool Matches(Thing t)
	{
		return AlwaysMatches(t.def);
	}

	public override bool CanEverMatch(ThingDef def)
	{
		return AlwaysMatches(def);
	}
}
