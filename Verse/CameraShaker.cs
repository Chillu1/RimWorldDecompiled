using UnityEngine;

namespace Verse
{
	public class CameraShaker
	{
		private float curShakeMag;

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
				curShakeMag = Mathf.Clamp(value, 0f, 0.2f);
			}
		}

		public Vector3 ShakeOffset
		{
			get
			{
				float x = Mathf.Sin(Time.realtimeSinceStartup * 24f) * curShakeMag;
				float y = Mathf.Sin(Time.realtimeSinceStartup * 24f * 1.05f) * curShakeMag;
				float z = Mathf.Sin(Time.realtimeSinceStartup * 24f * 1.1f) * curShakeMag;
				return new Vector3(x, y, z);
			}
		}

		public void DoShake(float mag)
		{
			if (!(mag <= 0f))
			{
				CurShakeMag += mag;
			}
		}

		public void SetMinShake(float mag)
		{
			CurShakeMag = Mathf.Max(CurShakeMag, mag);
		}

		public void Update()
		{
			curShakeMag -= 0.5f * RealTime.realDeltaTime;
			if (curShakeMag < 0f)
			{
				curShakeMag = 0f;
			}
		}
	}
}
