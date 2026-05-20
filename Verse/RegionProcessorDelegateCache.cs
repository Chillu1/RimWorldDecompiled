namespace Verse
{
	public abstract class RegionProcessorDelegateCache
	{
		private RegionProcessor regionProcessor;

		private RegionEntryPredicate regionEntryPredicate;

		public RegionProcessor RegionProcessorDelegate => regionProcessor;

		public RegionEntryPredicate RegionEntryPredicateDelegate => regionEntryPredicate;

		public RegionProcessorDelegateCache()
		{
			regionProcessor = RegionProcessor;
			regionEntryPredicate = RegionEntryPredicate;
		}

		protected abstract bool RegionEntryPredicate(Region from, Region to);

		protected abstract bool RegionProcessor(Region reg);
	}
}
