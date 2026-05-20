using Verse;

namespace RimWorld;

public class SpecialThingFilterWorker_NonSmeltable : SpecialThingFilterWorker
{
	public override bool Matches(Thing t)
	{
		return !t.Smeltable;
	}

	public override bool AlwaysMatches(ThingDef def)
	{
		return !def.PotentiallySmeltable;
	}
}
