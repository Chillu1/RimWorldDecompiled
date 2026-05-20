using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimWorld;

public class GravshipAudio
{
	private class TriggeredOneOff
	{
		private SoundDef def;

		private bool played;

		private float playedTime;

		public bool Finished
		{
			get
			{
				if (played)
				{
					return Time.time > playedTime + def.Duration.max;
				}
				return false;
			}
		}

		public float CurrentTimestamp
		{
			get
			{
				if (played)
				{
					return Time.time - playedTime;
				}
				return 0f;
			}
		}

		public TriggeredOneOff(SoundDef def)
		{
			this.def = def;
		}

		public void Trigger()
		{
			if (!played)
			{
				played = true;
				playedTime = Time.time;
				def.PlayOneShotOnCamera();
			}
		}
	}

	private class ManagedSustainer
	{
		private Sustainer sustainer;

		private bool alive = true;

		private bool hasFadedIn;

		private bool hasFadedOut;

		private bool fading;

		private float fadeTime;

		private float fadeDuration;

		private float fadeInitialVolume;

		private float fadeTarget;

		public Sustainer Sustainer => sustainer;

		public bool Fading => fading;

		public bool Alive => alive;

		public ManagedSustainer(SoundDef def)
		{
			sustainer = def.TrySpawnSustainer(SoundInfo.OnCamera(MaintenanceType.PerFrame));
			sustainer.info.volumeFactor = 0f;
		}

		public void Update()
		{
			if (fading || alive)
			{
				sustainer.Maintain();
			}
			if (fading)
			{
				fadeTime += Time.deltaTime;
				sustainer.info.volumeFactor = Mathf.Lerp(fadeInitialVolume, fadeTarget, fadeTime / fadeDuration);
				if (fadeTime >= fadeDuration)
				{
					fading = false;
				}
			}
		}

		private void Fade(float duration, float volume)
		{
			if (alive)
			{
				fading = true;
				fadeInitialVolume = sustainer.info.volumeFactor;
				fadeTarget = volume;
				fadeTime = 0f;
				fadeDuration = duration;
			}
		}

		public void FadeIn(float duration)
		{
			if (!hasFadedIn)
			{
				hasFadedIn = true;
				Fade(duration, 1f);
			}
		}

		public void FadeOut(float duration)
		{
			if (hasFadedIn && !hasFadedOut)
			{
				hasFadedOut = true;
				Fade(duration, 0f);
				alive = false;
			}
		}

		public void Stop()
		{
			if (alive)
			{
				alive = false;
				sustainer.End();
				sustainer = null;
			}
		}
	}

	private bool active;

	private ManagedSustainer engineLoop;

	private ManagedSustainer antigravLoop;

	private ManagedSustainer retroThrusterLoop;

	private TriggeredOneOff landingServos;

	private TriggeredOneOff touchdown;

	private TriggeredOneOff engineStop;

	private Queue<TriggeredOneOff> retroThrusterQueue = new Queue<TriggeredOneOff>();

	private const float ProgressThrustersDyingDown = 0.9f;

	private const float ProgressDustCloudsVisible = 0.15f;

	private const float ProgressTouchdown = 0.8f;

	private const float ThrusterSFXInterval = 0.5f;

	private SoundDef[] GetRetroThrusterSounds()
	{
		return new SoundDef[3]
		{
			SoundDefOf.Gravship_RetroThruster_Oneshot_01,
			SoundDefOf.Gravship_RetroThruster_Oneshot_02,
			SoundDefOf.Gravship_RetroThruster_Oneshot_03
		};
	}

	public void BeginTakeoff()
	{
		active = true;
		if (Prefs.GravshipCutscenes)
		{
			SoundDefOf.Gravship_Engine_Start.PlayOneShotOnCamera();
			InitialiseSustainers();
			engineLoop.FadeIn(1f);
		}
	}

	public void BeginLanding()
	{
		active = true;
		if (!Prefs.GravshipCutscenes)
		{
			return;
		}
		InitialiseSustainers();
		landingServos = new TriggeredOneOff(SoundDefOf.Gravship_LandingServos);
		touchdown = new TriggeredOneOff(SoundDefOf.Gravship_Touchdown);
		engineStop = new TriggeredOneOff(SoundDefOf.Gravship_Engine_Stop);
		retroThrusterQueue.Clear();
		foreach (SoundDef item in GetRetroThrusterSounds().InRandomOrder())
		{
			retroThrusterQueue.Enqueue(new TriggeredOneOff(item));
		}
		engineLoop.FadeIn(0.5f);
		antigravLoop.FadeIn(0.5f);
		retroThrusterLoop.FadeIn(0.5f);
	}

	public void InitialiseSustainers()
	{
		engineLoop = new ManagedSustainer(SoundDefOf.Gravship_Engine_Loop);
		antigravLoop = new ManagedSustainer(SoundDefOf.Gravship_Antigrav_Loop);
		retroThrusterLoop = new ManagedSustainer(SoundDefOf.Gravship_RetroThruster_Loop);
	}

	public void Update(float cutsceneTime, float progress, bool isTakeoff)
	{
		if (!active)
		{
			return;
		}
		engineLoop?.Update();
		antigravLoop?.Update();
		retroThrusterLoop?.Update();
		if (isTakeoff)
		{
			if (cutsceneTime > 1.5f)
			{
				antigravLoop?.FadeIn(0.1f);
			}
		}
		else
		{
			if (progress > 0.15f)
			{
				retroThrusterLoop?.FadeIn(0f);
				if (retroThrusterQueue.Count > 0)
				{
					TriggeredOneOff triggeredOneOff = retroThrusterQueue.Peek();
					if (triggeredOneOff.CurrentTimestamp > 0.5f)
					{
						retroThrusterQueue.Dequeue();
					}
					else
					{
						triggeredOneOff.Trigger();
					}
				}
			}
			if (progress > 0.5f)
			{
				landingServos?.Trigger();
			}
			if (progress > 0.8f)
			{
				touchdown?.Trigger();
				retroThrusterLoop?.FadeOut(0.5f);
			}
			if (progress > 0.9f)
			{
				engineLoop?.FadeOut(1.2f);
				antigravLoop?.FadeOut(1.2f);
				engineStop?.Trigger();
			}
		}
		if (Find.GravshipController.IsGravshipTravelling || isTakeoff)
		{
			return;
		}
		ManagedSustainer managedSustainer = engineLoop;
		if (managedSustainer != null && managedSustainer.Alive)
		{
			return;
		}
		ManagedSustainer managedSustainer2 = antigravLoop;
		if (managedSustainer2 == null || !managedSustainer2.Alive)
		{
			TriggeredOneOff triggeredOneOff2 = engineStop;
			if (triggeredOneOff2 == null || triggeredOneOff2.Finished)
			{
				active = false;
			}
		}
	}

	public void EndTakeoff()
	{
		engineLoop?.Stop();
		antigravLoop?.Stop();
		retroThrusterLoop?.Stop();
		engineLoop = null;
		antigravLoop = null;
		retroThrusterLoop = null;
	}
}
