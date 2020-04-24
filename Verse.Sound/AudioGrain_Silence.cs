using System.Collections.Generic;

namespace Verse.Sound
{
	public class AudioGrain_Silence : AudioGrain
	{
		[EditSliderRange(0f, 5f)]
		public FloatRange durationRange = new FloatRange(1f, 2f);

		public override IEnumerable<ResolvedGrain> GetResolvedGrains()
		{
			yield return new ResolvedGrain_Silence(this);
		}

		public override int GetHashCode()
		{
			return durationRange.GetHashCode();
		}
	}
}
