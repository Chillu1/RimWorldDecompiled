using Verse;

namespace RimWorld
{
	public class GasProperties
	{
		public bool blockTurretTracking;

		public float accuracyPenalty;

		public FloatRange expireSeconds = new FloatRange(30f, 30f);

		public float rotationSpeed;
	}
}
