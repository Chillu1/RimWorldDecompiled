using UnityEngine;

namespace Verse
{
	public class RememberedCameraPos : IExposable
	{
		public Vector3 rootPos;

		public float rootSize;

		public RememberedCameraPos(Map map)
		{
			rootPos = map.Center.ToVector3Shifted();
			rootSize = 24f;
		}

		public void ExposeData()
		{
			Scribe_Values.Look(ref rootPos, "rootPos");
			Scribe_Values.Look(ref rootSize, "rootSize", 0f);
		}
	}
}
