namespace Verse
{
	public class MoteCounter
	{
		private int moteCount;

		private const int SaturatedCount = 250;

		public int MoteCount => moteCount;

		public float Saturation => (float)moteCount / 250f;

		public bool Saturated => Saturation > 1f;

		public bool SaturatedLowPriority => Saturation > 0.8f;

		public void Notify_MoteSpawned()
		{
			moteCount++;
		}

		public void Notify_MoteDespawned()
		{
			moteCount--;
		}
	}
}
