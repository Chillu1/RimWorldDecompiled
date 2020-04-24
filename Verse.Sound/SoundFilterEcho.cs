using UnityEngine;

namespace Verse.Sound
{
	public class SoundFilterEcho : SoundFilter
	{
		[EditSliderRange(10f, 5000f)]
		[Description("Echo delay in ms. 10 to 5000. Default = 500.")]
		private float delay = 500f;

		[EditSliderRange(0f, 1f)]
		[Description("Echo decay per delay. 0 to 1. 1.0 = No decay, 0.0 = total decay (ie simple 1 line delay).")]
		private float decayRatio = 0.5f;

		[EditSliderRange(0f, 1f)]
		[Description("The volume of the echo signal to pass to output.")]
		private float wetMix = 1f;

		[EditSliderRange(0f, 1f)]
		[Description("The volume of the original signal to pass to output.")]
		private float dryMix = 1f;

		public override void SetupOn(AudioSource source)
		{
			AudioEchoFilter orMakeFilterOn = SoundFilter.GetOrMakeFilterOn<AudioEchoFilter>(source);
			orMakeFilterOn.delay = delay;
			orMakeFilterOn.decayRatio = decayRatio;
			orMakeFilterOn.wetMix = wetMix;
			orMakeFilterOn.dryMix = dryMix;
		}
	}
}
