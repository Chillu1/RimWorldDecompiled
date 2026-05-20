using System.Collections.Generic;
using UnityEngine;

namespace Verse.Sound;

public class AudioSourcePoolWorld
{
	private List<AudioSource> sourcesWorld = new List<AudioSource>();

	private const int NumSourcesWorld = 32;

	public AudioSourcePoolWorld()
	{
		GameObject gameObject = new GameObject("OneShotSourcesWorldContainer");
		gameObject.transform.position = Vector3.zero;
		for (int i = 0; i < 32; i++)
		{
			GameObject gameObject2 = new GameObject("OneShotSource_" + i);
			gameObject2.transform.parent = gameObject.transform;
			gameObject2.transform.localPosition = Vector3.zero;
			sourcesWorld.Add(AudioSourceMaker.NewAudioSourceOn(gameObject2));
		}
	}

	public AudioSource GetSourceWorld()
	{
		foreach (AudioSource item in sourcesWorld)
		{
			if (!item.isPlaying)
			{
				SoundFilterUtility.DisableAllFiltersOn(item);
				return item;
			}
		}
		return null;
	}
}
