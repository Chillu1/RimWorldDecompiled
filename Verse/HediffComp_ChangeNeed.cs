using RimWorld;

namespace Verse;

public class HediffComp_ChangeNeed : HediffComp
{
	private Need needCached;

	public HediffCompProperties_ChangeNeed Props => (HediffCompProperties_ChangeNeed)props;

	private Need Need
	{
		get
		{
			if (needCached == null)
			{
				base.Pawn.needs.TryGetNeed(out needCached);
			}
			return needCached;
		}
	}

	public override void CompPostTickInterval(ref float severityAdjustment, int delta)
	{
		if (Need != null)
		{
			Need.CurLevelPercentage += Props.percentPerDay / 60000f * (float)delta;
		}
	}
}
