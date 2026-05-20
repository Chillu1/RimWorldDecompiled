using Verse;

namespace RimWorld;

public class SpecialThingFilterWorker_Unfinished : SpecialThingFilterWorker
{
	public override bool Matches(Thing t)
	{
		return t.def.isUnfinishedThing;
	}

	public override bool CanEverMatch(ThingDef def)
	{
		return def.isUnfinishedThing;
	}

	public override bool AlwaysMatches(ThingDef def)
	{
		return def.isUnfinishedThing;
	}
}
