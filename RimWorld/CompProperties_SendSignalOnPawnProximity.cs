using Verse;

namespace RimWorld
{
	public class CompProperties_SendSignalOnPawnProximity : CompProperties
	{
		public bool triggerOnPawnInRoom;

		public float radius;

		public int enableAfterTicks;

		public bool onlyHumanlike;

		public string signalTag;

		public CompProperties_SendSignalOnPawnProximity()
		{
			compClass = typeof(CompSendSignalOnPawnProximity);
		}
	}
}
