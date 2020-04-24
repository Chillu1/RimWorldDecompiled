using UnityEngine;

namespace Verse.Sound
{
	public static class SoundFilterUtility
	{
		public static void DisableAllFiltersOn(AudioSource source)
		{
			DisableFilterOn<AudioLowPassFilter>(source);
			DisableFilterOn<AudioHighPassFilter>(source);
			DisableFilterOn<AudioEchoFilter>(source);
			DisableFilterOn<AudioReverbFilter>(source);
			DisableFilterOn<AudioDistortionFilter>(source);
			DisableFilterOn<AudioChorusFilter>(source);
		}

		private static void DisableFilterOn<T>(AudioSource source) where T : Behaviour
		{
			T component = source.GetComponent<T>();
			if ((Object)component != (Object)null)
			{
				component.enabled = false;
			}
		}
	}
}
