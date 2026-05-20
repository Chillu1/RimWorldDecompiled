using UnityEngine;

namespace Verse
{
	public struct FleckDrawData
	{
		public Vector3 pos;

		public float rotation;

		public Vector3 scale;

		public float alpha;

		public Color color;

		public int drawLayer;

		public Color? overrideColor;

		public DrawBatchPropertyBlock propertyBlock;

		public float ageSecs;

		public float id;

		public float calculatedShockwaveSpan;
	}
}
