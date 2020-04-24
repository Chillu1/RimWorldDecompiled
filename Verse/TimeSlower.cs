using UnityEngine;

namespace Verse
{
	public class TimeSlower
	{
		private int forceNormalSpeedUntil;

		private const int ForceTicksStandard = 800;

		private const int ForceTicksShort = 240;

		public bool ForcedNormalSpeed => Find.TickManager.TicksGame < forceNormalSpeedUntil;

		public void SignalForceNormalSpeed()
		{
			forceNormalSpeedUntil = Mathf.Max(Find.TickManager.TicksGame + 800);
		}

		public void SignalForceNormalSpeedShort()
		{
			forceNormalSpeedUntil = Mathf.Max(forceNormalSpeedUntil, Find.TickManager.TicksGame + 240);
		}
	}
}
