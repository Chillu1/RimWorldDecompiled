using UnityEngine;

namespace Verse.Noise
{
	public class DistanceFromPlanetViewCenter : ModuleBase
	{
		public Vector3 viewCenter;

		public float viewAngle;

		public bool invert;

		public DistanceFromPlanetViewCenter()
			: base(0)
		{
		}

		public DistanceFromPlanetViewCenter(Vector3 viewCenter, float viewAngle, bool invert = false)
			: base(0)
		{
			this.viewCenter = viewCenter;
			this.viewAngle = viewAngle;
			this.invert = invert;
		}

		public override double GetValue(double x, double y, double z)
		{
			float valueInt = GetValueInt(x, y, z);
			if (invert)
			{
				return 1f - valueInt;
			}
			return valueInt;
		}

		private float GetValueInt(double x, double y, double z)
		{
			if (viewAngle >= 180f)
			{
				return 0f;
			}
			return Mathf.Min(Vector3.Angle(viewCenter, new Vector3((float)x, (float)y, (float)z)) / viewAngle, 1f);
		}
	}
}
