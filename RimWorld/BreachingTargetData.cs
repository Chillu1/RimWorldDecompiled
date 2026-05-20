using Verse;

namespace RimWorld
{
	public class BreachingTargetData : IExposable
	{
		public Thing target;

		public IntVec3 firingPosition;

		public BreachingTargetData()
		{
		}

		public BreachingTargetData(Thing target, IntVec3 firingPosition)
		{
			this.target = target;
			this.firingPosition = firingPosition;
		}

		public void ExposeData()
		{
			Scribe_References.Look(ref target, "target");
			Scribe_Values.Look(ref firingPosition, "firingPosition", IntVec3.Invalid);
		}
	}
}
