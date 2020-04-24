using Verse;

namespace RimWorld.Planet
{
	public class TimeoutComp : WorldObjectComp
	{
		private int timeoutEndTick = -1;

		public bool Active => timeoutEndTick != -1;

		public bool Passed
		{
			get
			{
				if (Active)
				{
					return Find.TickManager.TicksGame >= timeoutEndTick;
				}
				return false;
			}
		}

		private bool ShouldRemoveWorldObjectNow
		{
			get
			{
				if (Passed)
				{
					return !base.ParentHasMap;
				}
				return false;
			}
		}

		public int TicksLeft
		{
			get
			{
				if (!Active)
				{
					return 0;
				}
				return timeoutEndTick - Find.TickManager.TicksGame;
			}
		}

		public void StartTimeout(int ticks)
		{
			timeoutEndTick = Find.TickManager.TicksGame + ticks;
		}

		public void StopTimeout()
		{
			timeoutEndTick = -1;
		}

		public override void CompTick()
		{
			base.CompTick();
			if (ShouldRemoveWorldObjectNow)
			{
				parent.Destroy();
			}
		}

		public override void PostExposeData()
		{
			base.PostExposeData();
			Scribe_Values.Look(ref timeoutEndTick, "timeoutEndTick", 0);
		}

		public override string CompInspectStringExtra()
		{
			if (Active && !base.ParentHasMap)
			{
				return "WorldObjectTimeout".Translate(TicksLeft.ToStringTicksToPeriod());
			}
			return null;
		}
	}
}
