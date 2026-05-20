using Verse;

namespace RimWorld;

public class ReadingPolicy : Policy
{
	public ThingFilter defFilter = new ThingFilter();

	public ThingFilter effectFilter;

	protected override string LoadKey => "ReadingPolicy";

	public ReadingPolicy()
	{
	}

	public ReadingPolicy(int id, string label)
		: base(id, label)
	{
		defFilter.SetAllowAll(null, includeNonStorable: true);
		effectFilter = new ThingFilter(ThingCategoryDefOf.BookEffects, onlySpecialFilters: true);
	}

	public override void CopyFrom(Policy other)
	{
		if (other is ReadingPolicy readingPolicy)
		{
			defFilter.CopyAllowancesFrom(readingPolicy.defFilter);
			effectFilter.CopyAllowancesFrom(readingPolicy.effectFilter);
		}
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Deep.Look(ref defFilter, "defFilter");
		Scribe_Deep.Look(ref effectFilter, "effectFilter");
	}
}
