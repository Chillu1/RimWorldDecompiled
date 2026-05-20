using System.Collections.Generic;
using UnityEngine;

namespace Verse;

public class CameraShaker
{
	private float curShakeMag;

	private List<ExtendedShakeRequest> extendedShakeRequests = new List<ExtendedShakeRequest>();

	private List<ExtendedShakeRequest> unpausedExtendedShakeRequests = new List<ExtendedShakeRequest>();

	private const float ShakeDecayRate = 0.5f;

	private const float ShakeFrequency = 24f;

	private const float MaxShakeMag = 0.2f;

	public float CurShakeMag
	{
		get
		{
			return curShakeMag;
		}
		set
		{
			curShakeMag = Mathf.Clamp(value, 0f, GetMaxShakeMag());
		}
	}

	public Vector3 ShakeOffset
	{
		get
		{
			if (Current.ProgramState != ProgramState.Playing)
			{
				return Vector3.zero;
			}
			if (Find.TickManager.Paused && !WorldComponent_GravshipController.CutsceneInProgress)
			{
				return Vector3.zero;
			}
			float x = Mathf.Sin(Time.realtimeSinceStartup * 24f) * curShakeMag;
			float z = Mathf.Sin(Time.realtimeSinceStartup * 24f * 1.1f) * curShakeMag;
			return new Vector3(x, 0f, z);
		}
	}

	public void DoShake(float mag)
	{
		if (!(mag <= 0f))
		{
			CurShakeMag += mag;
		}
	}

	public void DoShake(float mag, int durationTicks)
	{
		extendedShakeRequests.Add(new ExtendedShakeRequest(mag, durationTicks));
	}

	public float GetMaxShakeMag()
	{
		return 0.2f * Prefs.ScreenShakeIntensity;
	}

	public void SetMinShake(float mag)
	{
		CurShakeMag = Mathf.Max(CurShakeMag, mag);
	}

	public void StopAllShaking()
	{
		CurShakeMag = 0f;
	}

	public void Update()
	{
		for (int num = extendedShakeRequests.Count - 1; num >= 0; num--)
		{
			ExtendedShakeRequest extendedShakeRequest = extendedShakeRequests[num];
			if (Find.TickManager.TicksGame > extendedShakeRequest.StartTick + extendedShakeRequest.Duration)
			{
				extendedShakeRequests.RemoveAt(num);
			}
			else
			{
				SetMinShake(extendedShakeRequest.Mag);
			}
		}
		curShakeMag -= 0.5f * RealTime.realDeltaTime;
		if (curShakeMag < 0f)
		{
			curShakeMag = 0f;
		}
	}

	public void Expose()
	{
		Scribe_Collections.Look(ref extendedShakeRequests, "extendedShakeRequests", LookMode.Deep);
		Scribe_Collections.Look(ref unpausedExtendedShakeRequests, "unpausedExtendedShakeRequests", LookMode.Deep);
		if (extendedShakeRequests == null)
		{
			extendedShakeRequests = new List<ExtendedShakeRequest>();
		}
		if (unpausedExtendedShakeRequests == null)
		{
			unpausedExtendedShakeRequests = new List<ExtendedShakeRequest>();
		}
	}
}
