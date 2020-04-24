namespace Verse
{
	public class HediffComp_SelfHeal : HediffComp
	{
		public int ticksSinceHeal;

		public HediffCompProperties_SelfHeal Props => (HediffCompProperties_SelfHeal)props;

		public override void CompExposeData()
		{
			Scribe_Values.Look(ref ticksSinceHeal, "ticksSinceHeal", 0);
		}

		public override void CompPostTick(ref float severityAdjustment)
		{
			ticksSinceHeal++;
			if (ticksSinceHeal > Props.healIntervalTicksStanding)
			{
				severityAdjustment -= Props.healAmount;
				ticksSinceHeal = 0;
			}
		}
	}
}
