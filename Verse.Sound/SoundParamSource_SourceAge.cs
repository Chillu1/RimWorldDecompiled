using UnityEngine;

namespace Verse.Sound
{
	public class SoundParamSource_SourceAge : SoundParamSource
	{
		public TimeType timeType;

		public override string Label => "Sustainer age";

		public override float ValueFor(Sample samp)
		{
			if (timeType == TimeType.RealtimeSeconds)
			{
				return Time.realtimeSinceStartup - samp.ParentStartRealTime;
			}
			if (timeType == TimeType.Ticks && Find.TickManager != null)
			{
				return (float)Find.TickManager.TicksGame - samp.ParentStartTick;
			}
			return 0f;
		}
	}
}
