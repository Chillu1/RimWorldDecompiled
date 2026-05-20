using UnityEngine;
using Verse;

namespace RimWorld
{
	public class MusicManagerEntry
	{
		private AudioSource audioSource;

		private int silentTillFrame = -1;

		private float silenceMultiplier = 1f;

		private const string SourceGameObjectName = "MusicAudioSourceDummy";

		private const float SilenceMultiplierChangePerSecond = 1.75f;

		private float CurVolume => Prefs.VolumeMusic * SongDefOf.EntrySong.volume;

		public float CurSanitizedVolume => AudioSourceUtility.GetSanitizedVolume(CurVolume, "MusicManagerEntry");

		public void MusicManagerEntryUpdate()
		{
			if (audioSource == null || !audioSource.isPlaying)
			{
				StartPlaying();
			}
			float curSanitizedVolume = CurSanitizedVolume;
			if (Time.frameCount > silentTillFrame)
			{
				silenceMultiplier = Mathf.Clamp01(silenceMultiplier + 1.75f * Time.deltaTime);
			}
			else if (Time.frameCount <= silentTillFrame)
			{
				silenceMultiplier = Mathf.Clamp01(silenceMultiplier - 1.75f * Time.deltaTime);
			}
			audioSource.volume = curSanitizedVolume * silenceMultiplier;
		}

		private void StartPlaying()
		{
			if (audioSource != null && !audioSource.isPlaying)
			{
				audioSource.Play();
				return;
			}
			if (GameObject.Find("MusicAudioSourceDummy") != null)
			{
				Log.Error("MusicManagerEntry did StartPlaying but there is already a music source GameObject.");
				return;
			}
			GameObject gameObject = new GameObject("MusicAudioSourceDummy");
			gameObject.transform.parent = Camera.main.transform;
			audioSource = gameObject.AddComponent<AudioSource>();
			audioSource.bypassEffects = true;
			audioSource.bypassListenerEffects = true;
			audioSource.bypassReverbZones = true;
			audioSource.priority = 0;
			audioSource.clip = SongDefOf.EntrySong.clip;
			audioSource.volume = CurSanitizedVolume;
			audioSource.loop = true;
			audioSource.spatialBlend = 0f;
			audioSource.Play();
		}

		public void MaintainSilence()
		{
			silentTillFrame = Time.frameCount + 1;
		}
	}
}
