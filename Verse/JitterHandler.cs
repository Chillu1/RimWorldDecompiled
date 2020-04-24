using UnityEngine;

namespace Verse
{
	public class JitterHandler
	{
		private Vector3 curOffset = new Vector3(0f, 0f, 0f);

		private float DamageJitterDistance = 0.17f;

		private float DeflectJitterDistance = 0.1f;

		private float JitterDropPerTick = 0.018f;

		private float JitterMax = 0.35f;

		public Vector3 CurrentOffset => curOffset;

		public void JitterHandlerTick()
		{
			if (curOffset.sqrMagnitude < JitterDropPerTick * JitterDropPerTick)
			{
				curOffset = new Vector3(0f, 0f, 0f);
			}
			else
			{
				curOffset -= curOffset.normalized * JitterDropPerTick;
			}
		}

		public void Notify_DamageApplied(DamageInfo dinfo)
		{
			if (dinfo.Def.hasForcefulImpact)
			{
				AddOffset(DamageJitterDistance, dinfo.Angle);
			}
		}

		public void Notify_DamageDeflected(DamageInfo dinfo)
		{
			if (dinfo.Def.hasForcefulImpact)
			{
				AddOffset(DeflectJitterDistance, dinfo.Angle);
			}
		}

		public void AddOffset(float dist, float dir)
		{
			curOffset += Quaternion.AngleAxis(dir, Vector3.up) * Vector3.forward * dist;
			if (curOffset.sqrMagnitude > JitterMax * JitterMax)
			{
				curOffset *= JitterMax / curOffset.magnitude;
			}
		}
	}
}
