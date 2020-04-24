using UnityEngine;

namespace Verse.Sound
{
	public class SoundFilterReverb : SoundFilter
	{
		[Description("The base setup for this filter.\n\nOnly used if no parameters ever affect this filter.")]
		private ReverbSetup baseSetup = new ReverbSetup();

		public override void SetupOn(AudioSource source)
		{
			AudioReverbFilter orMakeFilterOn = SoundFilter.GetOrMakeFilterOn<AudioReverbFilter>(source);
			baseSetup.ApplyTo(orMakeFilterOn);
		}
	}
}
