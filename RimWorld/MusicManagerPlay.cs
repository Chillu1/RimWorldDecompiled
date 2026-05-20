using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LudeonTK;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using Verse.AI.Group;

namespace RimWorld;

public class MusicManagerPlay
{
	public enum MusicManagerState
	{
		Normal,
		Sequence,
		Fadeout
	}

	private AudioSource audioSource;

	private MusicManagerState state;

	private float fadeoutFactor = 1f;

	private float fadeinFactor = 1f;

	private float pausedVolumeFactor = 1f;

	private float musicOverridenFadeFactor = 1f;

	private float nextSongStartTime = 12f;

	private float pauseToggledTime = -9999f;

	private float pausedVolumeFactorAtToggle;

	private bool isPaused;

	public bool disabled;

	private bool songWasForced;

	private bool gameObjectCreated;

	private bool ignorePrefsVolumeThisSong;

	private bool manuallyTriggered;

	private bool preventDangerTransitionCheck;

	private bool initalized;

	public float subtleAmbienceSoundVolumeMultiplier = 1f;

	private MusicSequenceWorker sequenceWorker;

	private MusicTransition triggeredTransition;

	private float fadeoutDuration;

	private float fadeInDuration;

	private float fadeInStarted;

	private bool loopWhileFading;

	private SongDef currentSong;

	private float songEndTime;

	private readonly Queue<SongDef> recentSongs = new Queue<SongDef>();

	private readonly List<MusicTransition> transitions = new List<MusicTransition>();

	private static readonly FloatRange SongIntervalRelax = new FloatRange(85f, 105f);

	private static readonly FloatRange SongIntervalTension = new FloatRange(2f, 5f);

	private const float DefaultFadeout = 10f;

	private const float PauseVolumeFadeTime = 0.5f;

	private const int TransitionUpdatePeriodTicks = 60;

	public MusicManagerState State => state;

	public bool IsPlaying { get; private set; }

	public MusicTransition TriggeredTransition => triggeredTransition;

	public MusicSequenceWorker MusicSequenceWorker => sequenceWorker;

	public SongDef CurrentSong => currentSong;

	public float FadeoutPercent => 1f - fadeoutFactor;

	public float FadeInPercent => fadeinFactor;

	public float PausedVolumeFactor
	{
		get
		{
			MusicSequenceWorker musicSequenceWorker = sequenceWorker;
			if (musicSequenceWorker != null && musicSequenceWorker.def.pausedVolumeFactor.HasValue)
			{
				return Mathf.Lerp(sequenceWorker.def.pausedVolumeFactor.Value, 1f, pausedVolumeFactor);
			}
			return 1f;
		}
	}

	public float FadeoutDuration => fadeoutDuration;

	public float NextSongTimer => nextSongStartTime - Time.time;

	private float CurTime => Time.time;

	public float CurSanitizedVolume => AudioSourceUtility.GetSanitizedVolume(CurVolume, "MusicManagerPlay");

	public bool OverrideDangerMode { get; set; }

	public float SongTime => audioSource.time;

	public float SongDuration
	{
		get
		{
			if (!(audioSource.clip != null))
			{
				return 0f;
			}
			return audioSource.clip.length;
		}
	}

	public bool DangerMusicMode
	{
		get
		{
			if (OverrideDangerMode)
			{
				return true;
			}
			if (Find.Scenario.OverrideDangerMusic)
			{
				return true;
			}
			List<Map> maps = Find.Maps;
			for (int i = 0; i < maps.Count; i++)
			{
				if (maps[i].dangerWatcher.DangerRating == StoryDanger.High)
				{
					return true;
				}
			}
			return false;
		}
	}

	private float CurVolume
	{
		get
		{
			float num = (ignorePrefsVolumeThisSong ? Prefs.VolumeMaster : ((sequenceWorker?.def.overrideVolume * Prefs.VolumeMaster) ?? Prefs.VolumeMusic));
			if (currentSong == null)
			{
				return num;
			}
			return currentSong.volume * num * fadeoutFactor * fadeinFactor * musicOverridenFadeFactor * PausedVolumeFactor;
		}
	}

	public void ForceFadeoutAndSilenceFor(float time, float fadeoutTime = 10f, bool preventDangerTransition = false)
	{
		StopAndFadeoutSequence(fadeoutTime);
		nextSongStartTime = CurTime + time;
		preventDangerTransitionCheck = preventDangerTransition;
	}

	public void ForceSilenceFor(float time)
	{
		Stop();
		nextSongStartTime = CurTime + time;
	}

	public void MusicUpdate()
	{
		if (!gameObjectCreated)
		{
			InitializeMusicManager();
		}
		UpdateSubtleAmbienceSoundVolumeMultiplier();
		if (disabled)
		{
			return;
		}
		TickManager tickManager = Find.TickManager;
		if (tickManager.Paused != isPaused)
		{
			isPaused = tickManager.Paused;
			pauseToggledTime = Time.time;
			pausedVolumeFactorAtToggle = pausedVolumeFactor;
		}
		float t = Mathf.Clamp01((Time.time - pauseToggledTime) / 0.5f);
		pausedVolumeFactor = Mathf.Clamp01(Mathf.Lerp(pausedVolumeFactorAtToggle, (!isPaused) ? 1 : 0, t));
		if (songWasForced)
		{
			state = MusicManagerState.Normal;
			fadeoutFactor = 1f;
		}
		if (CurTime >= songEndTime)
		{
			if (loopWhileFading)
			{
				PlaySong(CurrentSong);
			}
			else
			{
				IsPlaying = false;
			}
		}
		if (IsPlaying && state == MusicManagerState.Normal && !songWasForced && ((DangerMusicMode && !currentSong.tense) || (!DangerMusicMode && currentSong.tense)))
		{
			StartFadeout(10f);
		}
		audioSource.volume = CurSanitizedVolume;
		if (state == MusicManagerState.Sequence)
		{
			UpdateSequenceState();
		}
		if (state != MusicManagerState.Fadeout)
		{
			CheckTransitionInterval();
		}
		if (state == MusicManagerState.Fadeout)
		{
			UpdateFadeout();
		}
		if (IsPlaying)
		{
			UpdateMusicFadeout();
			return;
		}
		if (!preventDangerTransitionCheck && DangerMusicMode && nextSongStartTime > CurTime + SongIntervalTension.max)
		{
			nextSongStartTime = CurTime + SongIntervalTension.RandomInRange;
		}
		if (nextSongStartTime < CurTime - 5f)
		{
			float num = (DangerMusicMode ? SongIntervalTension.RandomInRange : SongIntervalRelax.RandomInRange);
			nextSongStartTime = CurTime + num;
		}
		if (CurTime >= nextSongStartTime)
		{
			StartNewSong();
		}
	}

	private void CheckTransitionInterval()
	{
		if (GenTicks.IsTickInterval(60) || !initalized)
		{
			initalized = true;
			CheckTransitions();
		}
	}

	public void CheckTransitions()
	{
		bool flag = sequenceWorker != null && sequenceWorker.def.useTransitionForLifetime && !manuallyTriggered;
		bool flag2 = sequenceWorker == null || sequenceWorker.CanBeInterrupted();
		MusicTransition musicTransition = null;
		foreach (MusicTransition transition in transitions)
		{
			if (transition == triggeredTransition)
			{
				if (musicTransition == null || transition.def.priority >= musicTransition.def.priority)
				{
					musicTransition = transition;
				}
			}
			else if ((flag2 || transition.def.overridesInterruptions) && (triggeredTransition == null || triggeredTransition.def.priority <= transition.def.priority) && transition.IsTransitionSatisfied() && (musicTransition == null || transition.def.priority > musicTransition.def.priority))
			{
				musicTransition = transition;
			}
		}
		if (musicTransition != null && musicTransition != triggeredTransition)
		{
			triggeredTransition = musicTransition;
			StartSequence(musicTransition.def.sequence);
		}
		else if (triggeredTransition != null && flag && sequenceWorker.MinTimeSatisfied() && !triggeredTransition.IsTransitionSatisfied())
		{
			StopAndFadeoutSequence();
		}
	}

	private void UpdateSequenceState()
	{
		if (sequenceWorker.def.nextSequence != null && sequenceWorker.ShouldTransition())
		{
			StartSequence(sequenceWorker.def.nextSequence);
		}
		else if (sequenceWorker.ShouldEnd())
		{
			StopAndFadeoutSequence();
		}
		else
		{
			if (IsPlaying)
			{
				return;
			}
			if (sequenceWorker.ShouldLoop())
			{
				if (sequenceWorker.def.loopDelayRange != FloatRange.Zero)
				{
					if (nextSongStartTime < CurTime - 5f)
					{
						nextSongStartTime = CurTime + sequenceWorker.def.loopDelayRange.RandomInRange;
					}
					else if (CurTime >= nextSongStartTime)
					{
						sequenceWorker.timesLooped++;
						PlayNextSongInSequence();
					}
				}
				else
				{
					sequenceWorker.timesLooped++;
					PlayNextSongInSequence();
				}
			}
			else
			{
				Stop();
			}
		}
	}

	private void StopAndFadeoutSequence(float duration = 10f)
	{
		if (sequenceWorker != null)
		{
			if (sequenceWorker.def.overrideFadeout.HasValue)
			{
				duration = sequenceWorker.def.overrideFadeout.Value;
			}
			if (sequenceWorker.def.loopFadeout)
			{
				loopWhileFading = true;
			}
		}
		sequenceWorker = null;
		triggeredTransition = null;
		manuallyTriggered = false;
		StartFadeout(duration);
	}

	public void Stop()
	{
		if (audioSource != null)
		{
			audioSource.Stop();
		}
		sequenceWorker = null;
		triggeredTransition = null;
		IsPlaying = false;
		loopWhileFading = false;
		manuallyTriggered = false;
		state = MusicManagerState.Normal;
		float num = (DangerMusicMode ? SongIntervalTension.RandomInRange : SongIntervalRelax.RandomInRange);
		nextSongStartTime = CurTime + num;
	}

	private void StartSequence(MusicSequenceDef def)
	{
		float curVolume = CurVolume;
		sequenceWorker = (MusicSequenceWorker)Activator.CreateInstance(def.workerType);
		sequenceWorker.InitializeWorker(def, this);
		if (sequenceWorker.ShouldEnd())
		{
			Stop();
			return;
		}
		if (IsPlaying && sequenceWorker.def.fadeoutLastSongDuration > 0f && curVolume > 0f)
		{
			StartFadeout(sequenceWorker.def.fadeoutLastSongDuration);
			return;
		}
		state = MusicManagerState.Sequence;
		PlayNextSongInSequence();
	}

	private void UpdateMusicFadeout()
	{
		if (fadeInStarted != 0f)
		{
			fadeinFactor = Mathf.Clamp01(Mathf.InverseLerp(fadeInStarted, fadeInStarted + fadeInDuration, CurTime));
		}
		else
		{
			fadeinFactor = 1f;
		}
		Map currentMap = Find.CurrentMap;
		if (currentMap != null && !WorldRendererUtility.WorldSelected && !Find.TickManager.Paused)
		{
			float num = 1f;
			Camera camera = Find.Camera;
			List<Thing> list = currentMap.listerThings.ThingsInGroup(ThingRequestGroup.MusicalInstrument);
			for (int i = 0; i < list.Count; i++)
			{
				Building_MusicalInstrument building_MusicalInstrument = (Building_MusicalInstrument)list[i];
				if (building_MusicalInstrument.IsBeingPlayed)
				{
					float fadeFactor = GetFadeFactor(building_MusicalInstrument.Position.ToVector3Shifted(), building_MusicalInstrument.SoundRange, camera);
					if (fadeFactor < num)
					{
						num = fadeFactor;
					}
				}
			}
			List<Thing> list2 = currentMap.listerThings.ThingsInGroup(ThingRequestGroup.MusicSource);
			for (int j = 0; j < list2.Count; j++)
			{
				Thing thing = list2[j];
				CompPlaysMusic compPlaysMusic = thing.TryGetComp<CompPlaysMusic>();
				if (compPlaysMusic.Playing)
				{
					float fadeFactor2 = GetFadeFactor(thing.Position.ToVector3Shifted(), compPlaysMusic.SoundRange, camera);
					if (fadeFactor2 < num)
					{
						num = fadeFactor2;
					}
				}
			}
			foreach (Lord lord in currentMap.lordManager.lords)
			{
				if (lord.LordJob is LordJob_Ritual { AmbiencePlaying: not null } lordJob_Ritual && !lordJob_Ritual.AmbiencePlaying.def.subSounds.NullOrEmpty())
				{
					float fadeFactor3 = GetFadeFactor(lordJob_Ritual.selectedTarget.CenterVector3, lordJob_Ritual.AmbiencePlaying.def.subSounds.First().distRange, camera);
					if (fadeFactor3 < num)
					{
						num = fadeFactor3;
					}
				}
			}
			musicOverridenFadeFactor = num;
		}
		else
		{
			musicOverridenFadeFactor = 1f;
		}
	}

	private void UpdateFadeout()
	{
		fadeoutFactor -= Time.deltaTime / fadeoutDuration;
		if (fadeoutFactor <= 0f)
		{
			IsPlaying = false;
			loopWhileFading = false;
			audioSource.Stop();
			fadeoutFactor = 1f;
			if (sequenceWorker != null)
			{
				PlayNextSongInSequence();
				state = MusicManagerState.Sequence;
			}
			else
			{
				state = MusicManagerState.Normal;
			}
		}
	}

	private void StartFadeout(float duration)
	{
		state = MusicManagerState.Fadeout;
		fadeoutDuration = duration;
	}

	private float GetFadeFactor(Vector3 pos, FloatRange soundRange, Camera camera)
	{
		Vector3 vector = camera.transform.position - pos;
		vector.y = Mathf.Max(vector.y - 15f, 0f);
		vector.y *= 3.5f;
		return Mathf.Min(Mathf.Max(vector.magnitude - soundRange.min, 0f) / (soundRange.max - soundRange.min), 1f);
	}

	private void InitializeMusicManager()
	{
		gameObjectCreated = true;
		GameObject gameObject = new GameObject("MusicAudioSourceDummy");
		gameObject.transform.parent = Find.Root.soundRoot.sourcePool.sourcePoolCamera.cameraSourcesContainer.transform;
		audioSource = gameObject.AddComponent<AudioSource>();
		audioSource.bypassEffects = true;
		audioSource.bypassListenerEffects = true;
		audioSource.bypassReverbZones = true;
		audioSource.priority = 0;
		CacheTransitions();
		if (!disabled && !IsPlaying)
		{
			CheckTransitionInterval();
			if (!IsPlaying)
			{
				StartNewSong();
			}
		}
	}

	private void CacheTransitions()
	{
		transitions.Clear();
		List<MusicTransitionDef> list = DefDatabase<MusicTransitionDef>.AllDefsListForReading.OrderByDescending((MusicTransitionDef x) => x.priority).ToList();
		for (int num = 0; num < list.Count; num++)
		{
			MusicTransitionDef musicTransitionDef = list[num];
			MusicTransition musicTransition = (MusicTransition)Activator.CreateInstance(musicTransitionDef.workerType);
			musicTransition.InitializeWorker(musicTransitionDef, this);
			transitions.Add(musicTransition);
		}
	}

	private void UpdateSubtleAmbienceSoundVolumeMultiplier()
	{
		if (IsPlaying && CurSanitizedVolume > 0.001f)
		{
			subtleAmbienceSoundVolumeMultiplier -= Time.deltaTime * 0.1f;
		}
		else
		{
			subtleAmbienceSoundVolumeMultiplier += Time.deltaTime * 0.1f;
		}
		subtleAmbienceSoundVolumeMultiplier = Mathf.Clamp01(subtleAmbienceSoundVolumeMultiplier);
	}

	public void StartNewSong()
	{
		PlaySong(ChooseNextSong());
	}

	public void ScheduleNewSong()
	{
		nextSongStartTime = CurTime + 1f;
	}

	private void PlayNextSongInSequence()
	{
		if (sequenceWorker == null)
		{
			Log.Error("Attempted to select a song with a null sequence");
			return;
		}
		if (sequenceWorker.def.fadeInDuration > 0f && sequenceWorker.timesLooped == 0)
		{
			fadeInDuration = sequenceWorker.def.fadeInDuration;
			fadeInStarted = CurTime;
			fadeinFactor = 0f;
		}
		PlaySong(sequenceWorker.SelectSong());
	}

	private void PlaySong(SongDef song, bool forced = false, bool ignorePrefsVolume = false)
	{
		songWasForced = forced;
		currentSong = song;
		audioSource.clip = song.clip;
		audioSource.volume = CurSanitizedVolume;
		audioSource.spatialBlend = 0f;
		ignorePrefsVolumeThisSong = ignorePrefsVolume;
		songEndTime = CurTime + song.clip.length;
		audioSource.Play();
		IsPlaying = true;
		preventDangerTransitionCheck = false;
		recentSongs.Enqueue(song);
	}

	public void ForceTriggerNextSongOrSequence()
	{
		if (sequenceWorker != null)
		{
			if (sequenceWorker.def.nextSequence != null)
			{
				StartSequence(sequenceWorker.def.nextSequence);
			}
			else if (sequenceWorker.ShouldLoop())
			{
				sequenceWorker.timesLooped++;
				StartFadeout(10f);
			}
		}
	}

	public void ForceTriggerTransition(MusicTransitionDef transition)
	{
		foreach (MusicTransition transition2 in transitions)
		{
			if (transition2.def == transition)
			{
				manuallyTriggered = true;
				triggeredTransition = transition2;
				StartSequence(transition2.def.sequence);
				break;
			}
		}
	}

	public void ForcePlaySong(SongDef song, bool ignorePrefsVolume)
	{
		PlaySong(song, forced: true, ignorePrefsVolume);
	}

	private SongDef ChooseNextSong()
	{
		while (recentSongs.Count > 7)
		{
			recentSongs.Dequeue();
		}
		List<SongDef> list = DefDatabase<SongDef>.AllDefs.Where(AppropriateNow).ToList();
		if (list.Empty())
		{
			recentSongs.Clear();
			list = DefDatabase<SongDef>.AllDefs.Where(AppropriateNow).ToList();
		}
		if (list.Empty())
		{
			Log.Error("Could not get any appropriate song. Getting random and logging song selection data.");
			SongSelectionData();
			return DefDatabase<SongDef>.GetRandom();
		}
		return list.RandomElementByWeight((SongDef s) => s.commonality);
	}

	private bool AppropriateNow(SongDef song)
	{
		if (!song.playOnMap)
		{
			return false;
		}
		if (DangerMusicMode)
		{
			if (!song.tense)
			{
				return false;
			}
		}
		else if (song.tense)
		{
			return false;
		}
		Map map = Find.AnyPlayerHomeMap ?? Find.CurrentMap;
		if (!song.allowedSeasons.NullOrEmpty())
		{
			if (map == null)
			{
				return false;
			}
			if (!song.allowedSeasons.Contains(GenLocalDate.Season(map)))
			{
				return false;
			}
		}
		if (song.minRoyalTitle != null && !PawnsFinder.AllMapsCaravansAndTravellingTransporters_Alive_Colonists.Any((Pawn p) => p.royalty != null && p.royalty.AllTitlesForReading.Any() && p.royalty.MostSeniorTitle.def.seniority >= song.minRoyalTitle.seniority && !p.IsQuestLodger()))
		{
			return false;
		}
		if (recentSongs.Contains(song))
		{
			return false;
		}
		if (song.allowedTimeOfDay != TimeOfDay.Any)
		{
			if (map == null)
			{
				return true;
			}
			if (song.allowedTimeOfDay == TimeOfDay.Night)
			{
				if (!(GenLocalDate.DayPercent(map) < 0.2f))
				{
					return GenLocalDate.DayPercent(map) > 0.7f;
				}
				return true;
			}
			if (GenLocalDate.DayPercent(map) > 0.2f)
			{
				return GenLocalDate.DayPercent(map) < 0.7f;
			}
			return false;
		}
		return true;
	}

	public void OnApplicationFocus()
	{
	}

	public string DebugString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine("MusicManagerMap");
		stringBuilder.AppendLine("state: " + state);
		stringBuilder.AppendLine("lastStartedSong: " + currentSong);
		stringBuilder.AppendLine("fadeoutFactor: " + fadeoutFactor);
		stringBuilder.AppendLine("nextSongStartTime: " + nextSongStartTime);
		stringBuilder.AppendLine("CurTime: " + CurTime);
		stringBuilder.AppendLine("recentSongs: " + recentSongs.Select((SongDef s) => s.defName).ToCommaList(useAnd: true));
		stringBuilder.AppendLine("disabled: " + disabled);
		return stringBuilder.ToString();
	}

	[DebugOutput]
	public void SongSelectionData()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine("Most recent song: " + ((currentSong != null) ? currentSong.defName : "None"));
		stringBuilder.AppendLine();
		stringBuilder.AppendLine("Songs appropriate to play now:");
		foreach (SongDef item in DefDatabase<SongDef>.AllDefs.Where(AppropriateNow))
		{
			stringBuilder.AppendLine("   " + item.defName);
		}
		stringBuilder.AppendLine();
		stringBuilder.AppendLine("Recently played songs:");
		foreach (SongDef recentSong in recentSongs)
		{
			stringBuilder.AppendLine("   " + recentSong.defName);
		}
		Log.Message(stringBuilder.ToString());
	}
}
