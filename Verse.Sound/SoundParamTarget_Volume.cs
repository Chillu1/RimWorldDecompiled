namespace Verse.Sound
{
	public class SoundParamTarget_Volume : SoundParamTarget
	{
		public override string Label => "Volume";

		public override void SetOn(Sample sample, float value)
		{
			sample.SignalMappedVolume(value, this);
		}
	}
}
