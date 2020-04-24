using System.Collections.Generic;
using UnityEngine;

namespace Verse.Sound
{
	public class AudioGrain_Clip : AudioGrain
	{
		[NoTranslate]
		public string clipPath = "";

		public override IEnumerable<ResolvedGrain> GetResolvedGrains()
		{
			AudioClip audioClip = ContentFinder<AudioClip>.Get(clipPath);
			if (audioClip != null)
			{
				yield return new ResolvedGrain_Clip(audioClip);
			}
			else
			{
				Log.Error("Grain couldn't resolve: Clip not found at " + clipPath);
			}
		}
	}
}
