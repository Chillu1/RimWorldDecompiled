using System;
using UnityEngine;

namespace Verse
{
	public class CameraSwooper
	{
		public bool Swooping;

		private bool SwoopingTo;

		private float TimeSinceSwoopStart;

		private Vector3 FinalOffset;

		private float FinalOrthoSizeOffset;

		private float TotalSwoopTime;

		private SwoopCallbackMethod SwoopFinishedCallback;

		public void StartSwoopFromRoot(Vector3 FinalOffset, float FinalOrthoSizeOffset, float TotalSwoopTime, SwoopCallbackMethod SwoopFinishedCallback)
		{
			Swooping = true;
			TimeSinceSwoopStart = 0f;
			this.FinalOffset = FinalOffset;
			this.FinalOrthoSizeOffset = FinalOrthoSizeOffset;
			this.TotalSwoopTime = TotalSwoopTime;
			this.SwoopFinishedCallback = SwoopFinishedCallback;
			SwoopingTo = false;
		}

		public void StartSwoopToRoot(Vector3 FinalOffset, float FinalOrthoSizeOffset, float TotalSwoopTime, SwoopCallbackMethod SwoopFinishedCallback)
		{
			StartSwoopFromRoot(FinalOffset, FinalOrthoSizeOffset, TotalSwoopTime, SwoopFinishedCallback);
			SwoopingTo = true;
		}

		public void Update()
		{
			if (!Swooping)
			{
				return;
			}
			TimeSinceSwoopStart += Time.deltaTime;
			if (TimeSinceSwoopStart >= TotalSwoopTime)
			{
				Swooping = false;
				if (SwoopFinishedCallback != null)
				{
					SwoopFinishedCallback();
				}
			}
		}

		public void OffsetCameraFrom(GameObject camObj, Vector3 basePos, float baseSize)
		{
			float num = TimeSinceSwoopStart / TotalSwoopTime;
			if (!Swooping)
			{
				num = 0f;
			}
			else
			{
				num = TimeSinceSwoopStart / TotalSwoopTime;
				if (SwoopingTo)
				{
					num = 1f - num;
				}
				num = (float)Math.Pow(num, 1.7000000476837158);
			}
			camObj.transform.position = basePos + FinalOffset * num;
			Find.Camera.orthographicSize = baseSize + FinalOrthoSizeOffset * num;
		}
	}
}
