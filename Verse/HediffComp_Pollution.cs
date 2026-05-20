namespace Verse
{
	public class HediffComp_Pollution : HediffComp
	{
		public HediffCompProperties_Pollution Props => (HediffCompProperties_Pollution)props;

		public override void CompPostTick(ref float severityAdjustment)
		{
			Pawn pawn = parent.pawn;
			if (pawn.IsHashIntervalTick(Props.interval))
			{
				if (pawn.Spawned && pawn.Position.IsPolluted(pawn.Map))
				{
					severityAdjustment += Props.pollutedSeverity;
				}
				else
				{
					severityAdjustment += Props.unpollutedSeverity;
				}
			}
		}
	}
}
