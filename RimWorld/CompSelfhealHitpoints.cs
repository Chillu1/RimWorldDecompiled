using Verse;

namespace RimWorld
{
	public class CompSelfhealHitpoints : ThingComp
	{
		public int ticksPassedSinceLastHeal;

		public CompProperties_SelfhealHitpoints Props => (CompProperties_SelfhealHitpoints)props;

		public override void PostExposeData()
		{
			Scribe_Values.Look(ref ticksPassedSinceLastHeal, "ticksPassedSinceLastHeal", 0);
		}

		public override void CompTick()
		{
			ticksPassedSinceLastHeal++;
			if (ticksPassedSinceLastHeal == Props.ticksPerHeal)
			{
				ticksPassedSinceLastHeal = 0;
				if (parent.HitPoints < parent.MaxHitPoints)
				{
					parent.HitPoints++;
				}
			}
		}
	}
}
