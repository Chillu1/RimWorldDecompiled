using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class MusicSequenceDef : Def
{
	public Type workerType = typeof(MusicSequenceWorker);

	public SongDef song;

	public List<SongDef> songs;

	public MusicSequenceDef nextSequence;

	public bool loop;

	public bool canBeInterrupted = true;

	public bool useTransitionForLifetime = true;

	public bool transitionOnDanger;

	public bool transitionOnNoDanger;

	public bool endOnNoDanger;

	public bool endOnDanger;

	public FloatRange loopDelayRange = FloatRange.Zero;

	public float fadeoutLastSongDuration = 10f;

	public float fadeInDuration;

	public bool loopFadeout;

	public float? overrideFadeout;

	public float? overrideVolume;

	public float? minTimeToPlay;

	public float? pausedVolumeFactor;

	public override IEnumerable<string> ConfigErrors()
	{
		if (workerType != null && !typeof(MusicSequenceWorker).IsAssignableFrom(workerType))
		{
			yield return "Music condition type is not a subclass of MusicSequenceWorker, type was: " + workerType.FullName;
		}
		if (song != null && songs != null && workerType == typeof(MusicSequenceWorker))
		{
			yield return "You are using both a single song and a list, multiple songs won't play with default worker implementation, use a custom worker or remove one of the two options. In sequence def: " + defName;
		}
		foreach (string item in base.ConfigErrors())
		{
			yield return item;
		}
	}
}
