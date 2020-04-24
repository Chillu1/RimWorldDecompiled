using UnityEngine;

namespace Verse
{
	public class ShadowData
	{
		public Vector3 volume = Vector3.one;

		public Vector3 offset = Vector3.zero;

		public float BaseX => volume.x;

		public float BaseY => volume.y;

		public float BaseZ => volume.z;
	}
}
