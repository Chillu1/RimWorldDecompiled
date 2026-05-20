using Verse;

namespace RimWorld;

public class FoodPolicy : Policy
{
	public ThingFilter filter = new ThingFilter();

	protected override string LoadKey => "FoodPolicy";

	public FoodPolicy()
	{
	}

	public FoodPolicy(int id, string label)
		: base(id, label)
	{
	}

	public bool Allows(ThingDef def)
	{
		return filter.Allows(def);
	}

	public bool Allows(Thing thing)
	{
		return filter.Allows(thing);
	}

	public override void CopyFrom(Policy other)
	{
		if (other is FoodPolicy foodPolicy)
		{
			filter.CopyAllowancesFrom(foodPolicy.filter);
		}
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Deep.Look(ref filter, "filter");
	}
}
