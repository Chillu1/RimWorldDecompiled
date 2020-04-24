using UnityEngine;

namespace RimWorld.Planet
{
	public abstract class WorldCameraConfig
	{
		public float dollyRateKeys = 170f;

		public float dollyRateScreenEdge = 125f;

		public float camRotationDecayFactor = 0.9f;

		public float rotationSpeedScale = 0.3f;

		public float zoomSpeed = 2.6f;

		public float zoomPreserveFactor;

		public bool smoothZoom;

		public virtual void ConfigFixedUpdate_60(ref Vector2 rotationVelocity)
		{
		}

		public virtual void ConfigOnGUI()
		{
		}
	}
}
