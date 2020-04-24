using System;
using UnityEngine;

namespace Verse
{
	public static class Pulser
	{
		public static float PulseBrightness(float frequency, float amplitude)
		{
			return PulseBrightness(frequency, amplitude, Time.realtimeSinceStartup);
		}

		public static float PulseBrightness(float frequency, float amplitude, float time)
		{
			float num = time;
			num *= (float)Math.PI * 2f;
			num *= frequency;
			float t = (1f - Mathf.Cos(num)) * 0.5f;
			return Mathf.Lerp(1f - amplitude, 1f, t);
		}
	}
}
