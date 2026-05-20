using UnityEngine;

namespace Verse.Sound;

public abstract class SoundFilter
{
	public abstract void SetupOn(AudioSource source);

	protected static T GetOrMakeFilterOn<T>(AudioSource source) where T : Behaviour
	{
		T val = source.gameObject.GetComponent<T>();
		if (val != null)
		{
			val.enabled = true;
		}
		else
		{
			val = source.gameObject.AddComponent<T>();
		}
		return val;
	}
}
