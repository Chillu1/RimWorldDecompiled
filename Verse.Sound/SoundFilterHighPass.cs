using UnityEngine;

namespace Verse.Sound
{
	public class SoundFilterHighPass : SoundFilter
	{
		[EditSliderRange(50f, 20000f)]
		[Description("This filter will attenuate frequencies below this cutoff frequency.")]
		private float cutoffFrequency = 10000f;

		[EditSliderRange(1f, 10f)]
		[Description("The resonance Q value.")]
		private float highpassResonanceQ = 1f;

		public override void SetupOn(AudioSource source)
		{
			AudioHighPassFilter orMakeFilterOn = SoundFilter.GetOrMakeFilterOn<AudioHighPassFilter>(source);
			orMakeFilterOn.cutoffFrequency = cutoffFrequency;
			orMakeFilterOn.highpassResonanceQ = highpassResonanceQ;
		}
	}
}
