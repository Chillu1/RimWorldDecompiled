namespace Verse.Sound
{
	public class SoundParamSource_AggregateSize : SoundParamSource
	{
		public override string Label => "Aggregate size";

		public override float ValueFor(Sample samp)
		{
			if (samp.ExternalParams.sizeAggregator == null)
			{
				return 0f;
			}
			return samp.ExternalParams.sizeAggregator.AggregateSize;
		}
	}
}
