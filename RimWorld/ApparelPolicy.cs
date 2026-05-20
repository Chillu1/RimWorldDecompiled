using Verse;

namespace RimWorld;

public class ApparelPolicy : Policy
{
	public ThingFilter filter = new ThingFilter();

	protected override string LoadKey => "ApparelPolicy";

	public ApparelPolicy()
	{
	}

	public ApparelPolicy(int id, string label)
		: base(id, label)
	{
	}

	public override void CopyFrom(Policy other)
	{
		if (other is ApparelPolicy apparelPolicy)
		{
			filter.CopyAllowancesFrom(apparelPolicy.filter);
		}
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Deep.Look(ref filter, "filter");
	}
}
