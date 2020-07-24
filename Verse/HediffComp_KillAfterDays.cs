namespace Verse
{
	public class HediffComp_KillAfterDays : HediffComp
	{
		private int addedTick;

		public HediffCompProperties_KillAfterDays Props => (HediffCompProperties_KillAfterDays)props;

		public override void CompPostPostAdd(DamageInfo? dinfo)
		{
			addedTick = Find.TickManager.TicksGame;
		}

		public override void CompPostTick(ref float severityAdjustment)
		{
			if (Find.TickManager.TicksGame - addedTick >= 60000 * Props.days)
			{
				base.Pawn.Kill(null, parent);
			}
		}

		public override void CompExposeData()
		{
			Scribe_Values.Look(ref addedTick, "addedTick", 0);
		}
	}
}
