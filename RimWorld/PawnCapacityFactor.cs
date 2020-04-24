using UnityEngine;
using Verse;

namespace RimWorld
{
	public class PawnCapacityFactor
	{
		public PawnCapacityDef capacity;

		public float weight = 1f;

		public float max = 9999f;

		public bool useReciprocal;

		public float allowedDefect;

		private const float MaxReciprocalFactor = 5f;

		public float GetFactor(float capacityEfficiency)
		{
			float num = capacityEfficiency;
			if (allowedDefect != 0f && num < 1f)
			{
				num = Mathf.InverseLerp(0f, 1f - allowedDefect, num);
			}
			if (num > max)
			{
				num = max;
			}
			if (useReciprocal)
			{
				num = ((!(Mathf.Abs(num) < 0.001f)) ? Mathf.Min(1f / num, 5f) : 5f);
			}
			return num;
		}
	}
}
