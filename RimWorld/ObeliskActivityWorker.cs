using System.Text;
using Verse;

namespace RimWorld;

public class ObeliskActivityWorker : ActivityWorker
{
	private SimpleCurve ActivityGainPerDayFromHealthCurve = new SimpleCurve
	{
		new CurvePoint(0f, 47.9f),
		new CurvePoint(0.1f, 23.9f),
		new CurvePoint(0.8f, 0f)
	};

	public override float GetChangeRatePerDay(ThingWithComps thing)
	{
		CompActivity comp = thing.GetComp<CompActivity>();
		float num = base.GetChangeRatePerDay(thing);
		if (thing.IsOutside() && comp.Props.changePerDayOutside.HasValue)
		{
			num += comp.Props.changePerDayOutside.Value;
		}
		return num + ActivityGainPerDayFromHealthCurve.Evaluate((float)thing.HitPoints / (float)thing.MaxHitPoints);
	}

	public override void GetSummary(ThingWithComps thing, StringBuilder sb)
	{
		CompActivity comp = thing.GetComp<CompActivity>();
		base.GetSummary(thing, sb);
		float num = ActivityGainPerDayFromHealthCurve.Evaluate((float)thing.HitPoints / (float)thing.MaxHitPoints);
		if (num > 0f)
		{
			sb.Append(string.Format("\n - {0}: {1}", "HealthFactor".Translate(), num.ToStringPercent("0")));
		}
		if (thing.IsOutside() && comp.Props.changePerDayOutside.HasValue)
		{
			sb.Append(string.Format("\n - {0}: {1}", "NotInSealedRoom".Translate(), comp.Props.changePerDayOutside.Value.ToStringPercent("0")));
		}
	}
}
