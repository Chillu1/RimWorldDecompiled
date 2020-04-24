using UnityEngine;

namespace Verse.Sound
{
	public class SoundFilterLowPass : SoundFilter
	{
		[EditSliderRange(50f, 20000f)]
		[Description("This filter will attenuate frequencies above this cutoff frequency.")]
		private float cutoffFrequency = 10000f;

		[EditSliderRange(1f, 10f)]
		[Description("The resonance Q value.")]
		private float lowpassResonaceQ = 1f;

		public override void SetupOn(AudioSource source)
		{
			AudioLowPassFilter orMakeFilterOn = SoundFilter.GetOrMakeFilterOn<AudioLowPassFilter>(source);
			orMakeFilterOn.cutoffFrequency = cutoffFrequency;
			orMakeFilterOn.lowpassResonanceQ = lowpassResonaceQ;
		}
	}
}
