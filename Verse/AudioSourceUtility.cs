using UnityEngine;

namespace Verse;

public static class AudioSourceUtility
{
	public static float GetSanitizedVolume(float volume, object debugInfo)
	{
		if (float.IsNegativeInfinity(volume))
		{
			Log.ErrorOnce("Volume was negative infinity (" + debugInfo?.ToString() + ")", 863653423);
			return 0f;
		}
		if (float.IsPositiveInfinity(volume))
		{
			Log.ErrorOnce("Volume was positive infinity (" + debugInfo?.ToString() + ")", 954354323);
			return 1f;
		}
		if (float.IsNaN(volume))
		{
			Log.ErrorOnce("Volume was NaN (" + debugInfo?.ToString() + ")", 231846572);
			return 1f;
		}
		return Mathf.Clamp(volume, 0f, 1000f);
	}

	public static float GetSanitizedPitch(float pitch, object debugInfo)
	{
		if (float.IsNegativeInfinity(pitch))
		{
			Log.ErrorOnce("Pitch was negative infinity (" + debugInfo?.ToString() + ")", 546475990);
			return 0.0001f;
		}
		if (float.IsPositiveInfinity(pitch))
		{
			Log.ErrorOnce("Pitch was positive infinity (" + debugInfo?.ToString() + ")", 309856435);
			return 1f;
		}
		if (float.IsNaN(pitch))
		{
			Log.ErrorOnce("Pitch was NaN (" + debugInfo?.ToString() + ")", 800635427);
			return 1f;
		}
		if (pitch < 0f)
		{
			Log.ErrorOnce("Pitch was negative " + pitch + " (" + debugInfo?.ToString() + ")", 384765707);
			return 0.0001f;
		}
		return Mathf.Clamp(pitch, 0.0001f, 1000f);
	}
}
