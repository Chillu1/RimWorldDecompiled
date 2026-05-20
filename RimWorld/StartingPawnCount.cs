using Verse;

namespace RimWorld
{
	public abstract class StartingPawnCount : IExposable
	{
		public int count = 1;

		public bool requiredAtStart;

		[Unsaved(false)]
		public string countBuffer;

		public abstract string Summary { get; }

		public virtual void ExposeData()
		{
			Scribe_Values.Look(ref count, "count", 1);
			Scribe_Values.Look(ref requiredAtStart, "requiredAtStart", defaultValue: false);
		}
	}
}
