using UnityEngine;

namespace Verse.Sound;

public static class AudioSourceMaker
{
	private const AudioRolloffMode WorldRolloffMode = AudioRolloffMode.Linear;

	public static AudioSource NewAudioSourceOn(GameObject go)
	{
		if (go.GetComponent<AudioSource>() != null)
		{
			Log.Warning("Adding audio source on " + go?.ToString() + " that already has one.");
			return go.GetComponent<AudioSource>();
		}
		AudioSource audioSource = go.AddComponent<AudioSource>();
		audioSource.rolloffMode = AudioRolloffMode.Linear;
		audioSource.dopplerLevel = 0f;
		audioSource.playOnAwake = false;
		return audioSource;
	}
}
