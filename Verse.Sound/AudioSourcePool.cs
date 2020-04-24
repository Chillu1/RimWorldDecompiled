using UnityEngine;

namespace Verse.Sound
{
	public class AudioSourcePool
	{
		public AudioSourcePoolCamera sourcePoolCamera;

		public AudioSourcePoolWorld sourcePoolWorld;

		public AudioSourcePool()
		{
			sourcePoolCamera = new AudioSourcePoolCamera();
			sourcePoolWorld = new AudioSourcePoolWorld();
		}

		public AudioSource GetSource(bool onCamera)
		{
			if (onCamera)
			{
				return sourcePoolCamera.GetSourceCamera();
			}
			return sourcePoolWorld.GetSourceWorld();
		}
	}
}
