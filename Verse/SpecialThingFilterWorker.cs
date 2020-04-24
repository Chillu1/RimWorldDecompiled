namespace Verse
{
	public abstract class SpecialThingFilterWorker
	{
		public abstract bool Matches(Thing t);

		public virtual bool AlwaysMatches(ThingDef def)
		{
			return false;
		}

		public virtual bool CanEverMatch(ThingDef def)
		{
			return true;
		}
	}
}
