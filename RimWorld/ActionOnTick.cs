using Verse;

namespace RimWorld
{
	public abstract class ActionOnTick : IExposable
	{
		public int tick;

		public abstract void Apply(LordJob_Ritual ritual);

		public virtual void ExposeData()
		{
			Scribe_Values.Look(ref tick, "tick", 0);
		}
	}
}
