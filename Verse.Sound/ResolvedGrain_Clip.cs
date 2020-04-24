using UnityEngine;

namespace Verse.Sound
{
	public class ResolvedGrain_Clip : ResolvedGrain
	{
		public AudioClip clip;

		public ResolvedGrain_Clip(AudioClip clip)
		{
			this.clip = clip;
			duration = clip.length;
		}

		public override string ToString()
		{
			return "Clip:" + clip.name;
		}

		public override bool Equals(object obj)
		{
			if (obj == null)
			{
				return false;
			}
			ResolvedGrain_Clip resolvedGrain_Clip = obj as ResolvedGrain_Clip;
			if (resolvedGrain_Clip == null)
			{
				return false;
			}
			return resolvedGrain_Clip.clip == clip;
		}

		public override int GetHashCode()
		{
			if (clip == null)
			{
				return 0;
			}
			return clip.GetHashCode();
		}
	}
}
