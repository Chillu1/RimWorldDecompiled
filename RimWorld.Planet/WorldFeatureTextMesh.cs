using UnityEngine;

namespace RimWorld.Planet
{
	public abstract class WorldFeatureTextMesh
	{
		public abstract bool Active
		{
			get;
		}

		public abstract Vector3 Position
		{
			get;
		}

		public abstract Color Color
		{
			get;
			set;
		}

		public abstract string Text
		{
			get;
			set;
		}

		public abstract float Size
		{
			set;
		}

		public abstract Quaternion Rotation
		{
			get;
			set;
		}

		public abstract Vector3 LocalPosition
		{
			get;
			set;
		}

		public abstract void SetActive(bool active);

		public abstract void Destroy();

		public abstract void Init();

		public abstract void WrapAroundPlanetSurface();
	}
}
