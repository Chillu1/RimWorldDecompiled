using RimWorld;

namespace Verse
{
	public class HediffComp_Disappears : HediffComp
	{
		public int ticksToDisappear;

		public HediffCompProperties_Disappears Props => (HediffCompProperties_Disappears)props;

		public override bool CompShouldRemove
		{
			get
			{
				if (!base.CompShouldRemove)
				{
					return ticksToDisappear <= 0;
				}
				return true;
			}
		}

		public override string CompLabelInBracketsExtra
		{
			get
			{
				if (!Props.showRemainingTime)
				{
					return base.CompLabelInBracketsExtra;
				}
				return ticksToDisappear.ToStringTicksToPeriod(allowSeconds: true, shortForm: true);
			}
		}

		public override void CompPostMake()
		{
			base.CompPostMake();
			ticksToDisappear = Props.disappearsAfterTicks.RandomInRange;
		}

		public override void CompPostTick(ref float severityAdjustment)
		{
			ticksToDisappear--;
		}

		public override void CompPostMerged(Hediff other)
		{
			base.CompPostMerged(other);
			HediffComp_Disappears hediffComp_Disappears = other.TryGetComp<HediffComp_Disappears>();
			if (hediffComp_Disappears != null && hediffComp_Disappears.ticksToDisappear > ticksToDisappear)
			{
				ticksToDisappear = hediffComp_Disappears.ticksToDisappear;
			}
		}

		public override void CompExposeData()
		{
			Scribe_Values.Look(ref ticksToDisappear, "ticksToDisappear", 0);
		}

		public override string CompDebugString()
		{
			return "ticksToDisappear: " + ticksToDisappear;
		}
	}
}
