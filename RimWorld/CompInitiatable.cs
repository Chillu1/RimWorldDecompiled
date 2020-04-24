using Verse;

namespace RimWorld
{
	public class CompInitiatable : ThingComp
	{
		private int spawnedTick = -1;

		public int initiationDelayTicksOverride;

		public bool Initiated
		{
			get
			{
				if (spawnedTick >= 0)
				{
					return Find.TickManager.TicksGame >= spawnedTick + Delay;
				}
				return false;
			}
		}

		private int Delay
		{
			get
			{
				if (initiationDelayTicksOverride <= 0)
				{
					return Props.initiationDelayTicks;
				}
				return initiationDelayTicksOverride;
			}
		}

		private CompProperties_Initiatable Props => (CompProperties_Initiatable)props;

		public override void PostSpawnSetup(bool respawningAfterLoad)
		{
			base.PostSpawnSetup(respawningAfterLoad);
			if (!respawningAfterLoad)
			{
				spawnedTick = Find.TickManager.TicksGame;
			}
		}

		public override string CompInspectStringExtra()
		{
			if (!Initiated)
			{
				return "InitiatesIn".Translate() + ": " + (spawnedTick + Delay - Find.TickManager.TicksGame).ToStringTicksToPeriod();
			}
			return base.CompInspectStringExtra();
		}

		public override void PostExposeData()
		{
			base.PostExposeData();
			Scribe_Values.Look(ref spawnedTick, "spawnedTick", -1);
			Scribe_Values.Look(ref initiationDelayTicksOverride, "initiationDelayTicksOverride", 0);
		}
	}
}
