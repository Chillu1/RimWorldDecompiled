using System.Text;
using Verse;

namespace RimWorld;

public class NociosphereActivityWorker : PawnActivityWorker
{
	private const float UnstableChangePerDay = -0.05f;

	public override float GetChangeRatePerDay(ThingWithComps thing)
	{
		CompNociosphere comp = thing.GetComp<CompNociosphere>();
		float num = base.GetChangeRatePerDay(thing);
		if (comp.IsUnstable)
		{
			num += -0.05f;
		}
		return num;
	}

	public override void GetSummary(ThingWithComps thing, StringBuilder sb)
	{
		CompNociosphere comp = thing.GetComp<CompNociosphere>();
		base.GetSummary(thing, sb);
		if (comp.IsUnstable)
		{
			sb.Append(string.Format("\n - {0}: {1}", "Unstable".Translate(), (-0.05f).ToStringPercent("0")));
		}
	}
}
