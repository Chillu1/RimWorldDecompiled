using UnityEngine;
using Verse.Noise;

namespace Verse.Sound;

public class SoundParamSource_Perlin : SoundParamSource
{
	[Description("The type of time on which this perlin randomizer will work. If you use Ticks, it will freeze when paused and speed up in fast forward.")]
	public TimeType timeType;

	[Description("The frequency of the perlin output. The input time is multiplied by this amount.")]
	public float perlinFrequency = 1f;

	[Description("Whether to synchronize the Perlin output across different samples. If set to desync, each playing sample will get a separate Perlin output.")]
	public PerlinMappingSyncType syncType;

	private static Perlin perlin = new Perlin(0.009999999776482582, 2.0, 0.5, 4, Rand.Range(0, int.MaxValue), QualityMode.Medium);

	public override string Label => "Perlin noise";

	public override float ValueFor(Sample samp)
	{
		float num = ((syncType != PerlinMappingSyncType.Sync) ? ((float)(samp.GetHashCode() % 100)) : (samp.ParentHashCode % 100f));
		if (timeType == TimeType.Ticks && Current.ProgramState == ProgramState.Playing)
		{
			float num2 = ((syncType != PerlinMappingSyncType.Sync) ? ((float)(Find.TickManager.TicksGame - samp.startTick)) : ((float)Find.TickManager.TicksGame - samp.ParentStartTick));
			num2 /= 60f;
			num += num2;
		}
		else
		{
			float num3 = ((syncType != PerlinMappingSyncType.Sync) ? (Time.realtimeSinceStartup - samp.startRealTime) : (Time.realtimeSinceStartup - samp.ParentStartRealTime));
			num += num3;
		}
		num *= perlinFrequency;
		return ((float)perlin.GetValue(num, 0.0, 0.0) * 2f + 1f) / 2f;
	}
}
