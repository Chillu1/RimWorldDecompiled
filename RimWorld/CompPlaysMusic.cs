using Verse;

namespace RimWorld
{
	public abstract class CompPlaysMusic : ThingComp
	{
		public abstract bool Playing { get; }

		public abstract FloatRange SoundRange { get; }
	}
}
