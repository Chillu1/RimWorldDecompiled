using UnityEngine;
using Verse;

namespace RimWorld
{
	public class PawnCapacityOffset
	{
		public PawnCapacityDef capacity;

		public float scale = 1f;

		public float max = 9999f;

		public float GetOffset(float capacityEfficiency)
		{
			return (Mathf.Min(capacityEfficiency, max) - 1f) * scale;
		}
	}
}
