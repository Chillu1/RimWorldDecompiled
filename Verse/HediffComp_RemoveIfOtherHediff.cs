namespace Verse;

public class HediffComp_RemoveIfOtherHediff : HediffComp_MessageBase
{
	private const int MtbRemovalCheckInterval = 1000;

	protected HediffCompProperties_RemoveIfOtherHediff Props => (HediffCompProperties_RemoveIfOtherHediff)props;

	public override void CompPostTickInterval(ref float severityAdjustment, int delta)
	{
		if (ShouldRemove(delta))
		{
			Message();
			parent.pawn.health.RemoveHediff(parent);
		}
	}

	private bool ShouldRemove(int delta)
	{
		if (base.CompShouldRemove)
		{
			return true;
		}
		foreach (HediffDef hediff in Props.hediffs)
		{
			Hediff firstHediffOfDef = base.Pawn.health.hediffSet.GetFirstHediffOfDef(hediff);
			if (firstHediffOfDef != null && (!Props.stages.HasValue || Props.stages.Value.Includes(firstHediffOfDef.CurStageIndex)) && (Props.mtbHours <= 0 || (base.Pawn.IsHashIntervalTick(1000, delta) && Rand.MTBEventOccurs(Props.mtbHours, 2500f, 1000f))))
			{
				return true;
			}
		}
		return false;
	}
}
