using UnityEngine;

namespace Verse
{
	public abstract class CameraMapConfig
	{
		public float dollyRateKeys = 50f;

		public float dollyRateScreenEdge = 35f;

		public float camSpeedDecayFactor = 0.85f;

		public float moveSpeedScale = 2f;

		public float zoomSpeed = 2.6f;

		public float minSize = 11f;

		public float zoomPreserveFactor;

		public bool smoothZoom;

		public virtual void ConfigFixedUpdate_60(ref Vector3 velocity)
		{
		}

		public virtual void ConfigOnGUI()
		{
		}
	}
}
