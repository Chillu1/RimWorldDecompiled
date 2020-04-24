using UnityEngine;
using Verse;

namespace RimWorld
{
	public class MusicManagerEntry
	{
		private AudioSource audioSource;

		private const string SourceGameObjectName = "MusicAudioSourceDummy";

		private float CurVolume => Prefs.VolumeMusic * SongDefOf.EntrySong.volume;

		public float CurSanitizedVolume => AudioSourceUtility.GetSanitizedVolume(CurVolume, "MusicManagerEntry");

		public void MusicManagerEntryUpdate()
		{
			if (audioSource == null || !audioSource.isPlaying)
			{
				StartPlaying();
			}
			audioSource.volume = CurSanitizedVolume;
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
	}
}
